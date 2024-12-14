using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class NamespaceGenerator : GeneratorBase<NamespaceDefinition>
    {
        protected override string FileName => "Namespace.g.cs";

        public NamespaceGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        protected override NameScope? GenerateTypeScope(NamespaceDefinition @namespace, Usage usage)
        {
            if (@namespace.IsRoot)
                return null;
            return new NameScope()
            {
                ScopeName = @namespace.Name,
                IsNamespace = true,
                ScopePrefix = "namespace",
                ParentScope = TryGetScope(@namespace.Namespace?.Definition, usage)
            };
        }
    }
}
