using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    internal abstract record ScopingEntityDefinitionBase : ScopedEntityDefinitionBase
    {
        /// <summary>
        /// Entity (Namespace, Class, ...) inside this scope
        /// </summary>
        public ITypeToken<ScopedEntityDefinitionBase>[] NestedTypes { get; set; } = Array.Empty<ITypeToken<ScopedEntityDefinitionBase>>();
    }
}
