using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class ComObjectGenerator : GeneratorBase<ObjectDefinition>
    {
        readonly ParameterHelper _parameterHelper;

        protected override string FileName => "ComObjects.g.cs";
        protected override string? CustomNamespace => $"{Context.Options.RootNamespace}.Com";

        public ComObjectGenerator(IServiceProvider serviceProvider)
           : base(serviceProvider)
        {
            _parameterHelper = serviceProvider.GetRequiredService<ParameterHelper>();
        }

        public override IEnumerable<string> Usings()
        {
            yield return "System";
            yield return "System.Diagnostics";
            yield return "System.Runtime.InteropServices";
            yield return "System.Runtime.CompilerServices";
            yield return "System.Runtime.InteropServices.Marshalling";
            yield return "System.Runtime.Versioning";
            if (Context.Options.GenerateFramework)
                yield return $"{Context.Options.RootNamespace}.Framework";
            else
                yield return "BindingsGenerator.Framework";
        }

        protected override void GenerateDefinitions(IEnumerable<ObjectDefinition> definitions)
        {
            if (!Context.Options.GenerateCOMBindings)
                return;

            //COM-Framework
            {
                //IUnknown
                WriteGeneratedCodeAttribute();
                WriteLine("[GeneratedComInterface]");
                WriteLine("[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]");
                WriteLine("[Guid(\"00000000-0000-0000-c000-000000000046\")]");
                WriteLine("[SupportedOSPlatform(\"windows\")]");
                WriteLine("public unsafe partial interface IUnknown");
                using (BeginBlock())
                {
                    WriteLine("[PreserveSig]");
                    WriteLine("internal int QueryInterface(ref System.Guid @riid, ref void* @ppvObject);");
                    WriteLine("[PreserveSig]");
                    WriteLine("internal uint AddRef();");
                    WriteLine("[PreserveSig]");
                    WriteLine("internal uint Release();");
                }
                WriteLine();

                //ComHelper
                WriteGeneratedCodeAttribute();
                WriteLine("[SupportedOSPlatform(\"windows\")]");
                WriteLine("public static partial class ComHelper");
                using (BeginBlock())
                {
                    WriteLine("static readonly ComWrappers _comWrapper = new StrategyBasedComWrappers();");
                    WriteLine("public static Guid GetGuid<TUnknown>() where TUnknown : class, IUnknown");
                    using (BeginBlock())
                    {
                        WriteLine("var guidAttr = typeof(TUnknown).GetCustomAttributes(typeof(GuidAttribute), false)?.FirstOrDefault() as GuidAttribute;");
                        WriteLine("return Guid.Parse(guidAttr.Value);");
                    }
                    WriteLine("public static unsafe TUnknown CreateCOM<TUnknown>() where TUnknown : class, IUnknown");
                    using (BeginBlock())
                    {
                        WriteLine("var guid = GetGuid<TUnknown>();");
                        WriteLine("return (TUnknown)Activator.CreateInstance(Type.GetTypeFromCLSID(guid, throwOnError: true)!)!;");
                    }
                    WriteLine("public static unsafe TUnknown CreateCOM<TUnknown>(void* ptr) where TUnknown : class, IUnknown");
                    using (BeginBlock())
                    {
                        WriteLine("var guid = GetGuid<TUnknown>();");
                        WriteLine("return (TUnknown)_comWrapper.GetOrCreateObjectForComInstance((nint)ptr, CreateObjectFlags.None);");
                    }
                    WriteLine("public static void Release<TUnknown>(TUnknown unknown) where TUnknown : class, IUnknown");
                    using (BeginBlock())
                    {
                        WriteLine("var ptrUnknown = _comWrapper.GetOrCreateComInterfaceForObject(unknown, CreateComInterfaceFlags.None);");
                        WriteLine("Marshal.Release(ptrUnknown);");
                    }
                    WriteLine("public static void SafeRelease<TUnknown>(ref TUnknown? unknown) where TUnknown : class, IUnknown");
                    using (BeginBlock())
                    {
                        WriteLine("if (unknown == null) return;");
                        WriteLine("Release(unknown);");
                        WriteLine("unknown = null;");
                    }
                }
                WriteLine();
            }

            base.GenerateDefinitions(definitions);
        }

        protected override void GenerateDefinition(ObjectDefinition @class)
        {
            if (@class.IsComObject == null)
                return;
            if (@class.Name == "IUnknown")
                return; //Generated in framework
            if (!GetInheritance(@class, traverseBase: true).Any(x => x == "IUnknown"))
                return; //Com must inherit from IUnknown

            using (BeginNamespace(@class))
            {
                GenerateComInterface(@class);
            }
        }

        private void GenerateComInterface(ObjectDefinition @class)
        {
            WriteSummary(@class);
            if (!@class.IsCompleted)
                WriteLine("/// <remarks>This interface is incomplete.</remarks>");
            WriteGeneratedCodeAttribute();
            WriteObsoletion(@class);
            WriteLine("[GeneratedComInterface]");
            WriteLine("[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]");
            WriteLine($"[Guid(\"{@class.IsComObject!.Value}\")]");
            WriteLine("[SupportedOSPlatform(\"windows\")]");

            var inheritanceString = string.Empty;
            var inheritance = GetInheritance(@class).ToArray();
            if (inheritance.Length != 0)
                inheritanceString = ": " + string.Join(", ", inheritance);
            WriteLine($"public unsafe partial interface {@class.Name} {ReplaceIUnknown(inheritanceString)}");
            using (BeginBlock())
            {
                //Functions
                foreach (var function in @class.Functions)
                {
                    if (function.AccessSpecifier == CppSharp.AST.AccessSpecifier.Private)
                        continue; //Irrelavent
                    if (function.IsStatic)
                        continue; //Static functions will be evaluated later
                    if (function.Overridden != null)
                        continue; //Function is overridden -> dont add to interface
                    var functionType = function.FunctionType.FinalDefinition;
                    if (functionType == null)
                        continue;

                    WriteSummary(function);
                    WriteObsoletion(function);
                    WriteLine("[PreserveSig]");

                    var returnType = _parameterHelper.GetReturnType(functionType, out var returnAttribute, customUsage: AttributeUsage.COM);
                    if (!string.IsNullOrEmpty(returnAttribute))
                        WriteLine(returnAttribute);

                    var parameters = _parameterHelper.GetParameters(functionType.Parameters.Skip(1), withAttributes: true, customUsage: AttributeUsage.COM);
                    var parameterNames = _parameterHelper.GetParamterNames(functionType.Parameters.Skip(1));

                    var functionName = function.Name;
                    WriteLine($"{ReplaceIUnknown(returnType)} {functionName}({ReplaceIUnknown(parameters)});");
                }
            }
        }

        #region Helper
        private string[] GetInheritance(ObjectDefinition @class, bool traverseBase = false)
        {
            List<string> inherited = new();
            foreach (var @base in @class.Bases)
            {
                if (@base.AccessSpecifier != CppSharp.AST.AccessSpecifier.Public)
                    continue;
                inherited.Add(@base.Type.Definition.Name);
                if (traverseBase && @base.Type.FinalDefinition != null)
                {
                    inherited.AddRange(GetInheritance(@base.Type.FinalDefinition, traverseBase: traverseBase));
                }
            }
            return inherited.ToHashSet().ToArray();
        }
        private string BuildInheritanceString(ObjectDefinition @class, bool includeSelf = false)
        {
            List<string> inherited = new()
            {
                "IUnknown"
            };
            if (includeSelf)
                inherited.Add(@class.Name);
            inherited.AddRange(GetInheritance(@class));

            inherited = inherited.ToHashSet().ToList();
            if (inherited.Count == 0)
                return string.Empty;
            return ": " + string.Join(", ", inherited);
        }
        private string ReplaceIUnknown(string str)
        {
            var unknownName = $"{Context.Options.RootNamespace}.Com.IUnknown";
            return str.Replace("IUnknown", unknownName);
        }
        #endregion

    }
}
