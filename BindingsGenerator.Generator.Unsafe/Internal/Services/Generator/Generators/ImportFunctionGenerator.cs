using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Generator.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using CppCallingConvention = global::CppSharp.AST.CallingConvention;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class ImportFunctionGenerator : GeneratorBase<ImportFunctionDefinitionBase>
    {
        readonly ParameterHelper _paramHelper;
        readonly TypeHelper _typeHelper;

        protected override string FileName => "Imports.g.cs";
        protected override bool UseNestedTypeGeneration => true;
        protected override string? CustomTypeName => "imports";

        public ImportFunctionGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _paramHelper = serviceProvider.GetRequiredService<ParameterHelper>();
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }

        public override IEnumerable<string> Usings()
        {
            yield return "System";
            yield return "System.Security";
            yield return "System.Runtime.InteropServices";
            if (Context.Options.GenerateFramework)
                yield return $"{Context.Options.RootNamespace}.Framework";
            else
                yield return "BindingsGenerator.Framework";
            yield return $"static {Context.Options.RootNamespace}.vectors";
        }

        protected override TypeMapping? GenerateTypeMapping(IDefinition definition)
        {
            if (definition?.Name == "char*")
            {
                return new TypeMapping()
                {
                    Typename = "string",
                    MarshalAs = new TypeAttribute()
                    {
                        Usage = AttributeUsage.Parameter | AttributeUsage.ReturnValue | AttributeUsage.Field | AttributeUsage.COM,
                        Attribute = "MarshalAs(UnmanagedType.LPWStr)"
                    }
                };
            }
            if (definition?.Name == "sbyte*")
            {
                return new TypeMapping()
                {
                    Typename = "string",
                    MarshalAs = new TypeAttribute()
                    {
                        Usage = AttributeUsage.Parameter | AttributeUsage.ReturnValue | AttributeUsage.Field | AttributeUsage.COM,
                        Attribute = "MarshalAs(UnmanagedType.LPStr)"
                    }
                };
            }
            return null;
        }

        protected override void GenerateDefinitions(IEnumerable<ImportFunctionDefinitionBase> functions)
        {
            //Preprocess signatures
            PreprocessImportFunctions(functions);

            //Remove unexported functions
            var importableFunctions = ExportedFunctions(functions).ToArray();

            base.GenerateDefinitions(importableFunctions);

            //Inject Initialize() to setup vectors
            WriteLine("internal unsafe static void Initialize()");
            using (BeginBlock())
            {
                foreach (var function in functions)
                {
                    var functionName = _typeHelper.GetFullTypeName(function, useMapping: false).ToFullName();
                    if (importableFunctions.Contains(function))
                        WriteLine($"vectors._{functionName} = {function.GetFunctionName()};");
                    else
                        WriteLine($"vectors._{functionName} = null;");
                }
            }
        }
        protected override void GenerateDefinition(ImportFunctionDefinitionBase function)
        {
            WriteGeneratedCodeAttribute();

            if (Context.Options.SuppressUnmanagedCodeSecurity)
                WriteLine("[SuppressUnmanagedCodeSecurity]");

            WriteLine($"[DllImport(\"{function.LibraryName}\", EntryPoint = \"{function.FunctionSignature}\", CallingConvention = CallingConvention.{Convert(function.CallingConvention)})]");

            var returnType = _paramHelper.GetReturnType(function, out var returnAttribute);
            if (!string.IsNullOrEmpty(returnAttribute))
                WriteLine(returnAttribute);
            var parameters = _paramHelper.GetParameters(function.Parameters, withAttributes: true);

            WriteLine($"internal static extern {returnType} {function.GetFunctionName()}({parameters});");
            WriteLine();
        }

        private void PreprocessImportFunctions(IEnumerable<ImportFunctionDefinitionBase> functions)
        {
            foreach (var function in functions)
            {
                //Not importable
                if (string.IsNullOrEmpty(function.LibraryName))
                    continue;

                //Check if function is exported via signature
                if (Context.ExportMap.Values.Any(f => f.FunctionSignature == function.FunctionSignature))
                    continue; //Nothing to do

                //Check if function is exported via c-signature
                if (Context.ExportMap.Values.Any(f => f.FunctionSignature.ToFullName() == function.Name))
                {
                    //Override signature
                    var setMethod = function.GetType().GetProperty(nameof(ImportFunctionDefinitionBase.FunctionSignature)).SetMethod;
                    setMethod?.Invoke(function, new object[] { function.Name });
                    continue;
                }
            }
        }
        private IEnumerable<ImportFunctionDefinitionBase> ExportedFunctions(IEnumerable<ImportFunctionDefinitionBase> functions)
        {
            foreach (var function in functions)
            {
                if (!string.IsNullOrEmpty(function.LibraryName) &&
                    Context.ExportMap.Values.Any(f => f.FunctionSignature == function.FunctionSignature))
                    yield return function;
            }
        }
        private static CallingConvention Convert(CppCallingConvention callingConvention)
        {
            switch (callingConvention)
            {
                case CppCallingConvention.Default:
                    return CallingConvention.Winapi;
                case CppCallingConvention.C:
                    return CallingConvention.Cdecl;
                case CppCallingConvention.StdCall:
                    return CallingConvention.StdCall;
                case CppCallingConvention.ThisCall:
                    return CallingConvention.ThisCall;
                case CppCallingConvention.FastCall:
                    return CallingConvention.FastCall;
                default:
                    throw new NotSupportedException($"Unknown calling convention: {callingConvention}");
            }
        }
    }
}
