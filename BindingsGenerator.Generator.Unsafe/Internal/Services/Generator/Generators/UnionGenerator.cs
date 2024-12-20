using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class UnionGenerator : GeneratorBase<UnionDefinition>
    {
        readonly TypeHelper _typeHelper;

        protected override string FileName => "Unions.g.cs";

        public UnionGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }

        public override IEnumerable<string> Usings()
        {
            yield return "System";
            yield return "System.Runtime.InteropServices";
        }

        protected override NameScope? GenerateTypeScope(UnionDefinition union, Usage usage)
        {
            return new NameScope()
            {
                ScopeName = union.Name,
                IsNamespace = false,
                ScopePrefix = "public unsafe partial struct",
                ParentScope = TryGetScope(@union.Namespace?.Definition, usage)
            };
        }

        protected override void GenerateDefinition(UnionDefinition union)
        {
            using (BeginNamespace(union))
            {
                WriteSummary(union);
                if (!union.IsCompleted)
                    WriteLine("/// <remarks>This union is incomplete.</remarks>");
                WriteGeneratedCodeAttribute();
                WriteObsoletion(union);
                WriteLine("[StructLayout(LayoutKind.Explicit)]");
                WriteLine($"public unsafe partial struct {union.Name}");

                using (BeginBlock())
                {
                    foreach (var field in union.Fields)
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
        }
    }
}
