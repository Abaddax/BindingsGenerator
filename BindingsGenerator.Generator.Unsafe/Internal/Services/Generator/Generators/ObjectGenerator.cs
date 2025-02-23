using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Generator.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class ObjectGenerator : GeneratorBase<ObjectDefinition>
    {
        readonly TypeHelper _typeHelper;

        protected override string FileName => "Objects.g.cs";

        public ObjectGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }

        public override IEnumerable<string> Usings()
        {
            yield return "System";
            yield return "System.Diagnostics";
            yield return "System.Runtime.InteropServices";
            yield return "System.Runtime.CompilerServices";
            if (Context.Options.GenerateFramework)
                yield return $"{Context.Options.RootNamespace}.Framework";
            else
                yield return "BindingsGenerator.Unsafe.Framework";
            yield return $"static {Context.Options.RootNamespace}.vectors";
        }

        protected override TypeMapping? GenerateTypeMapping(IDefinition definition, Usage usage)
        {
            if (definition is TypeDefinition typeDefinition)
                definition = typeDefinition.GetNestedType();

            return definition switch
            {
                ObjectDefinition @class when definition is ObjectDefinition => @class switch
                {
                    //No special alias
                    _ when @class.IsPOD() && Context.Options.IsFinal => null,
                    //Direct instance
                    _ => new TypeMapping()
                    {
                        TypeName = $"{@class.Name}_Instance",
                        TypeUsage = Usage.All
                    }
                },
                PointerDefinition pointer when definition is PointerDefinition => pointer switch
                {
                    //No special alias
                    _ when pointer.GetPointedType().TryGetNestedType() is ObjectDefinition @class &&
                        (@class.IsPOD() && Context.Options.IsFinal) => null,
                    //Interface
                    _ when pointer.Type.Definition?.TryGetNestedType() is ObjectDefinition @class &&
                        !(usage.HasFlag(Usage.COM) || usage.HasFlag(Usage.Field)) => new TypeMapping()
                        {
                            TypeName = @class.Name,
                            TypeUsage = Usage.Parameter | Usage.ReturnValue,
                            MarshalAs =
                            [
                                new TypeAttribute()
                                {
                                    Usage = Usage.Parameter | Usage.ReturnValue,
                                    Attribute = $"MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NativeInstanceMarshaler<{_typeHelper.GetFullTypeName(@class, useMapping: false)}_Impl>))"
                                }
                            ]
                        },
                    //Instance pointer
                    _ when pointer.GetPointedType().TryGetNestedType() is ObjectDefinition @class => new TypeMapping()
                    {
                        TypeName = $"{@class.Name}_Instance" + new string('*', pointer.GetPointerDepth()),
                        TypeUsage = Usage.Field | Usage.COM
                    },
                    //Unknown
                    _ => null
                },
                //Unknown
                _ => null
            };
        }
        protected override NameScope? GenerateTypeScope(ObjectDefinition @class, Usage usage)
        {
            if (@class.IsPOD() && Context.Options.IsFinal)
            {
                return new NameScope()
                {
                    ScopeName = @class.Name,
                    IsNamespace = false,
                    ScopePrefix = "public unsafe partial struct",
                    ParentScope = TryGetScope(@class.Namespace?.Definition, usage)
                };
            }
            else
            {
                return new NameScope()
                {
                    ScopeName = @class.Name,
                    IsNamespace = false,
                    ScopePrefix = "public unsafe partial interface",
                    ParentScope = TryGetScope(@class.Namespace?.Definition, usage)
                };
            }
        }

        protected override void GenerateDefinition(ObjectDefinition @class)
        {
            using (BeginNamespace(@class))
            {
                if (@class.IsPOD() && Context.Options.IsFinal && @class.IsComObject == null)
                {
                    GeneratePOD(@class);
                }
                else
                {
                    GenerateInterface(@class);
                    WriteLine();
                    GenerateInternalImpl(@class);
                    WriteLine();
                    GenerateInstanceImpl(@class);
                }
            }
        }

        private void GeneratePOD(ObjectDefinition @class, string suffix = null)
        {
            WriteSummary(@class);
            if (!@class.IsCompleted)
                WriteLine("/// <remarks>This struct is incomplete.</remarks>");
            WriteGeneratedCodeAttribute();
            WriteObsoletion(@class);
            WriteSupportedOsPlatformAttribute();
            WriteLine("[StructLayout(LayoutKind.Explicit)]");
            WriteLine($"public unsafe partial struct {@class.Name}{suffix}");

            using (BeginBlock())
            {
                foreach (var field in @class.Fields)
                {
                    WriteSummary(field);
                    WriteObsoletion(field);
                    if (!field.IsStatic)
                        WriteLine($"[FieldOffset({field.FieldOffset})]");

                    var typeName = _typeHelper.GetFullTypeName(field.FieldType, useMapping: true, usage: Usage.Field);
                    var marshalAs = _typeHelper.GetTypeMarshalAs(field.FieldType, Usage.Field);
                    if (!string.IsNullOrEmpty(marshalAs))
                        WriteLine($"[{marshalAs}]");

                    if (!field.IsStatic)
                        WriteLine($"{field.AccessSpecifier.Get()} {typeName} @{field.Name};");
                    else
                        WriteLine($"static {field.AccessSpecifier.Get()} {typeName} @{field.Name};");
                }
            }
        }

        private void GenerateInterface(ObjectDefinition @class)
        {
            WriteSummary(@class);
            if (!@class.IsCompleted)
                WriteLine("/// <remarks>This interface is incomplete.</remarks>");
            WriteGeneratedCodeAttribute();
            WriteObsoletion(@class);
            WriteSupportedOsPlatformAttribute();
            WriteLine($"public unsafe partial interface {@class.Name} {BuildInheritanceString(@class)}");

            using (BeginBlock())
            {
                //GUID
                if (@class.IsComObject != null)
                    WriteLine($"public static System.Guid Guid {{ get; }} = Guid.Parse(\"{@class.IsComObject}\");");

                //Fields
                foreach (var field in @class.Fields)
                {
                    var typeName = _typeHelper.GetFullTypeName(field.FieldType, useMapping: true);

                    if (field.AccessSpecifier == CppSharp.AST.AccessSpecifier.Private)
                        continue; //Irrelavent
                    if (field.IsStatic)
                        continue; //Static fields will be evaluated later

                    WriteSummary(field);
                    WriteObsoletion(field);

                    if (!field.IsVTable)
                        WriteLine($"{field.AccessSpecifier.Get()} {typeName} @{field.Name} {{ get; set; }}");
                    else
                        WriteLine($"internal {typeName} @{field.Name} {{ get; }}");
                }

                //Functions
                foreach (var function in @class.Functions)
                {
                    var typeName = _typeHelper.GetFullTypeName(function.FunctionType, useMapping: true);

                    if (function.AccessSpecifier == CppSharp.AST.AccessSpecifier.Private)
                        continue; //Irrelavent
                    if (function.IsStatic)
                        continue; //Static functions will be evaluated later
                    if (function.Overridden != null)
                        continue; //Function is overridden -> dont add to interface

                    WriteSummary(function);
                    WriteObsoletion(function);

                    WriteLine($"internal {typeName} @{function.GetFunctionName()}_Func {{ get; }}");
                }

                //Static constructor
                WriteLine($"public static {@class.Name} Create(void* instance) => new {@class.Name}_Impl() {{ @instance = ({@class.Name}_Instance*)instance}};");
            }
        }
        private void GenerateInternalImpl(ObjectDefinition @class)
        {
            WriteSummary(@class);
            if (!@class.IsCompleted)
                WriteLine("/// <remarks>This struct is incomplete.</remarks>");
            WriteGeneratedCodeAttribute();
            WriteObsoletion(@class);
            WriteSupportedOsPlatformAttribute();
            WriteLine($"internal unsafe partial struct {@class.Name}_Impl {BuildInheritanceString(@class, true)}");

            using (BeginBlock())
            {
                //Add instance field
                WriteLine($"internal {@class.Name}_Instance* @instance;");

                //Add properties
                //WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                WriteLine($"void* INativeInstance.@Instance");
                using (BeginBlock())
                {
                    WriteLine($"init {{ @instance = ({@class.Name}_Instance*)value; }}");
                }
                //Add converter
                WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]");
                WriteLine($"void* INativeInstance.GetInstance<T>()");
                using (BeginBlock())
                {
                    foreach (var inherited in ListBaseOffsets(@class))
                    {
                        WriteLine($"if(typeof(T) == typeof({inherited.@base}_Impl))");
                        using (BeginBlock())
                        {
                            WriteLine($"return (byte*)@instance + {inherited.offset};");
                        }
                    }
                    WriteLine("throw new InvalidCastException($\"Unable to cast {this.GetType()} to {typeof(T)}\");");
                }

                //Build fields
                foreach (var field in ListAllFields(@class))
                {
                    if (field.field.IsStatic)
                        continue; //Static fields will be evaluated later

                    var typeName = _typeHelper.GetFullTypeName(field.field.FieldType, useMapping: true);
                    if (!field.field.IsVTable)
                    {
                        //Wire up to instance
                        WriteLine($"{typeName} {field.@base}.@{field.field.Name}");
                        using (BeginBlock())
                        {
                            WriteLine($"get {{ return @instance->@{field.field.Name}; }}");
                            WriteLine($"set {{ @instance->@{field.field.Name} = value; }}");
                        }
                    }
                    else
                    {
                        //Wire up VTable
                        //WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");                       
                        WriteLine($"{typeName} {field.@base}.@{field.field.Name}");
                        using (BeginBlock())
                        {
                            WriteLine($"get {{ return (({field.@base}_Instance*)((byte*)@instance + {field.offset}))->{field.@base}_{field.field.Name}; }}");
                        }
                    }
                }

                //Build functions
                foreach (var function in ListAllFunctions(@class))
                {
                    if (function.function.IsStatic)
                        continue;//Static functions will be evaluated later

                    var typeName = _typeHelper.GetFullTypeName(function.function.FunctionType, useMapping: true);

                    //Default func
                    if (!function.function.IsVirtual)
                    {
                        var fullname = _typeHelper.GetFullTypeName(function.function.FunctionType.Definition, useMapping: false).ToFullName();
                        //Wire up to vectors
                        //WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        WriteLine($"{typeName} {function.@base}.@{function.function.GetFunctionName()}_Func => vectors.{fullname};");
                    }
                    //Virtual func
                    else
                    {
                        //Wire up to vtable
                        //Should always be in "VTable1Ptr" for the base
                        //WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        WriteLine($"{typeName} {function.@base}.@{function.function.GetFunctionName()}_Func => Marshal.GetDelegateForFunctionPointer<{typeName}>((IntPtr)(({function.@base})this).@VTable1Ptr->@{function.function.GetFunctionName()});");
                    }
                }
            }
        }
        private void GenerateInstanceImpl(ObjectDefinition @class)
        {
            WriteSummary(@class);
            if (!@class.IsCompleted)
                WriteLine("/// <remarks>This struct is incomplete.</remarks>");
            WriteGeneratedCodeAttribute();
            WriteObsoletion(@class);
            WriteSupportedOsPlatformAttribute();
            WriteLine("[StructLayout(LayoutKind.Explicit)]");
            WriteLine($"public unsafe partial struct {@class.Name}_Instance");

            using (BeginBlock())
            {
                foreach (var field in ListAllFields(@class))
                {
                    WriteSummary(field.field);
                    WriteObsoletion(field.field);
                    if (!field.field.IsStatic)
                        WriteLine($"[FieldOffset({field.offset})]");

                    var typeName = _typeHelper.GetFullTypeName(field.field.FieldType, useMapping: true);

                    if (!field.field.IsStatic && field.field.IsVTable)
                        WriteLine($"internal {typeName} @{field.@base}_{field.field.Name};");
                    else if (!field.field.IsStatic)
                        WriteLine($"{field.field.AccessSpecifier.Get()} {typeName} @{field.field.Name};");
                    else
                        WriteLine($"static {field.field.AccessSpecifier.Get()} {typeName} @{field.field.Name};");
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
                "INativeInstance"
            };
            if (includeSelf)
                inherited.Add(@class.Name);
            inherited.AddRange(GetInheritance(@class));

            inherited = inherited.ToHashSet().ToList();
            if (inherited.Count == 0)
                return string.Empty;
            return ": " + string.Join(", ", inherited);
        }
        private IEnumerable<(MemberField field, string @base, uint @offset)> ListAllFields(ObjectDefinition @class)
        {
            var fields = new List<(MemberField field, string @base, uint @offset)>();
            //Add own fields
            foreach (var field in @class.Fields)
            {
                fields.Add((field, @class.Name, field.FieldOffset));
            }
            //Add inherited fields
            foreach (var @base in @class.Bases)
            {
                var baseType = @base.Type.FinalDefinition;
                if (baseType == null)
                    continue;

                var inheritedFields = ListAllFields(baseType);
                foreach (var inheritedField in inheritedFields)
                {
                    if (fields.Any(f => !f.field.IsVTable && f.field.Name == inheritedField.field.Name))
                        throw new InvalidOperationException($"Field {inheritedField.field.Name} exists multiple times");
                    fields.Add((inheritedField.field, inheritedField.@base, @base.TypeOffset + inheritedField.offset));
                }
            }
            return fields;
        }
        private IEnumerable<(MemberFunction function, string @base)> ListAllFunctions(ObjectDefinition @class)
        {
            var functions = new List<(MemberFunction function, string @base)>();
            //Add own fields
            foreach (var function in @class.Functions)
            {
                functions.Add((function, @class.Name));
            }
            //Add inherited functions
            foreach (var @base in @class.Bases)
            {
                var baseType = @base.Type.FinalDefinition;
                if (baseType == null)
                    continue;

                var inheritedFunctions = ListAllFunctions(baseType);
                foreach (var inheritedFunction in inheritedFunctions)
                {
                    //Already exists
                    if (functions.Any(f => f.function.Name == inheritedFunction.function.Name))
                    {
                        var existingFunction = functions.First(f => f.function.Name == inheritedFunction.function.Name);
                        //Check if overridden
                        if (existingFunction.function.Overridden != null)//.IsOverride)
                        {
                            //Replace base method
                            functions[functions.IndexOf(existingFunction)] = inheritedFunction;
                            continue; //overridden
                        }
                        //Check if overloaded
                        if (existingFunction.function.FunctionType.FinalDefinition.Overload != inheritedFunction.function.FunctionType.FinalDefinition.Overload)
                        {
                            //Add base method
                            functions.Add(inheritedFunction);
                            continue; //overloaded
                        }
                        throw new InvalidOperationException($"Function {inheritedFunction.function.Name} exists multiple times without override");
                    }
                    functions.Add(inheritedFunction);
                }
            }
            return functions;
        }
        private bool SearchVTableFunction(ObjectDefinition @class, MemberFunction function, out MemberField? vTableField, out VTableFunction? vTableFunction)
        {
            //Seach function in vtables
            foreach (var _vTableField in @class.Fields.Where(f => f.IsVTable))
            {
                //Get vTable
                var vTablePtr = _vTableField.FieldType.FinalDefinition as PointerDefinition;
                if (vTablePtr == null)
                    continue;
                var vTable = vTablePtr.Type.FinalDefinition as VTableDefinition;
                if (vTable == null)
                    continue;

                //Get vFunc
                foreach (var vFunc in vTable.Functions)
                {
                    //Found
                    if (vFunc.Name == function.Name)
                    {
                        vTableField = _vTableField;
                        vTableFunction = vFunc;
                        return true;
                    }
                }
            }
            //Nothing found
            vTableField = null;
            vTableFunction = null;
            return false;
        }
        private IEnumerable<(string @base, uint offset)> ListBaseOffsets(ObjectDefinition @class)
        {
            var bases = new HashSet<(string @base, uint offset)>()
            {
                //Self
                (@class.Name, 0)
            };
            //Inherited
            foreach (var @base in @class.Bases)
            {
                if (@base.AccessSpecifier != CppSharp.AST.AccessSpecifier.Public)
                    continue;
                var baseType = @base.Type.Definition as ObjectDefinition;
                if (baseType == null)
                    continue;
                //Directly inherited type
                bases.Add((baseType.Name, @base.TypeOffset));
                //Indirecly inherited types
                var inherited = ListBaseOffsets(baseType);
                foreach (var inherit in inherited)
                {
                    bases.Add((inherit.@base, @base.TypeOffset + inherit.offset));
                }
            }
            return bases;
        }
        #endregion
    }
}
