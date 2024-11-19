using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    internal abstract record ScopedEntityDefinitionBase : NamedEntityDefinitionBase, IFinalDefinition
    {
        /// <summary>
        /// Scope of this entity (Namespace, Class, ...)
        /// </summary>
        public required ITypeToken<ScopingEntityDefinitionBase> Namespace { get; init; }
    }
}
