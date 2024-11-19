using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;



namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class TypedefGenerator : GeneratorBase<TypeDefinition>
    {
        readonly TypeHelper _typeHelper;

        protected override string FileName => "Typedefs.g.cs";
        protected override bool UseEmptyFile => true;

        public TypedefGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }

        protected override void GenerateDefinition(TypeDefinition type)
        {
            var nestedType = GetNestedType(type);
            if (nestedType == null)
                return;

            var typeName = _typeHelper.GetFullTypeName(type, useMapping: false);
            var nestedTypeName = _typeHelper.GetFullTypeName(nestedType, useMapping: false);
            if (nestedType is IFinalDefinition)
                nestedTypeName = $"{Context.Options.RootNamespace}.{nestedTypeName}";

            WriteLine($"global using {typeName} = {nestedTypeName};");
        }

        private IDefinition GetNestedType(TypeDefinition type)
        {
            var nestedType = type.Type.Definition;
            if (nestedType is TypeDefinition typeDef)
                return GetNestedType(typeDef);
            return nestedType!;
        }
    }
}
