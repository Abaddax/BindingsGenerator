using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class VTableGenerator : GeneratorBase<VTableDefinition>
    {
        readonly TypeHelper _typeHelper;

        protected override string FileName => "VTables.g.cs";

        public VTableGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }

        public override IEnumerable<string> Usings()
        {
            yield return "System";
            yield return "System.Runtime.InteropServices";
        }

        protected override NameScope? GenerateTypeScope(VTableDefinition vtable)
        {
            return new NameScope()
            {
                ScopeName = vtable.Name,
                IsNamespace = false,
                ScopePrefix = "unknown",
                ParentScope = TryGetScope(vtable.Object?.Definition)
            };
        }

        protected override void GenerateDefinition(VTableDefinition vtable)
        {
            using (BeginNamespace(vtable))
            {
                WriteSummary(vtable);
                WriteGeneratedCodeAttribute();
                WriteObsoletion(vtable);
                WriteLine("[StructLayout(LayoutKind.Explicit)]");
                WriteLine($"internal unsafe partial struct {vtable.Name}");

                using (BeginBlock())
                {
                    foreach (var function in vtable.Functions)
                    {
                        WriteSummary(function);
                        WriteLine($"[FieldOffset({function.FunctionOffset})]");

                        var functionPtrType = function.FunctionType.FinalDefinition;
                        if (functionPtrType == null)
                            continue;
                        var typeName = _typeHelper.GetFullTypeName(functionPtrType.Type, useMapping: true);

                        WriteLine($"{function.AccessSpecifier.Get()} {typeName}* @{function.GetFunctionName()};");
                    }
                }
            }
        }
    }
}
