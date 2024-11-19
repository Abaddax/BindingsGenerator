using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for typedefs
    /// </summary>
    [DebuggerDisplay("Typedef: {Name, nq}")]
    internal sealed record TypeDefinition : ScopedEntityDefinitionBase, IAliasDefinition
    {
        /// <summary>
        /// Type that the pointer points to
        /// </summary>
        public required ITypeToken Type { get; init; }

        public ITypeToken<TFinalDefinition>? TryCast<TFinalDefinition>() where TFinalDefinition : class, IFinalDefinition
        {
            if (Type.Store == null)
                return null;
            if (Type is ITypeToken<TFinalDefinition> definition)
                return definition;
            if (Type.Definition is IAliasDefinition alias)
                return alias.TryCast<TFinalDefinition>();
            return null;
        }
    }
}
