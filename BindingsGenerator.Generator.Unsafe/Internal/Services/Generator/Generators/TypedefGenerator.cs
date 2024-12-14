using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;



namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class TypedefGenerator : GeneratorBase<TypeDefinition>
    {
        readonly ConcurrentBag<string> _typeDefs = new();
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
            var nestedType = type.GetNestedType();
            if (nestedType == null)
                return;

            var typeName = _typeHelper.GetFullTypeName(type, useMapping: false);
            if (_typeDefs.Contains(typeName))
                return; //Already typedefed
            var nestedTypeName = _typeHelper.GetFullTypeName(nestedType, useMapping: false);
            if (nestedTypeName == "void")
                return; //cannot typedef void (edge-case)
            if (nestedType is IFinalDefinition)
                nestedTypeName = $"{Context.Options.RootNamespace}.{nestedTypeName}";

            WriteLine($"global using {typeName} = {nestedTypeName};");
            _typeDefs.Add(typeName);
        }
    }
}
