using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for all enums
    /// </summary>
    [DebuggerDisplay("Enum: {Name, nq}")]
    internal sealed record EnumerationDefinition : ScopedEntityDefinitionBase, IDefinition, IFinalDefinition
    {
        /// <summary>
        /// Type of the underlying system type
        /// </summary>
        public required ITypeToken UnderlyingType { get; init; }
        /// <summary>
        /// Enumeration items
        /// </summary>
        public EnumerationItem[] Items { get; set; } = Array.Empty<EnumerationItem>();
        /// <summary>
        /// Type declaration is completed
        /// </summary>
        public bool IsCompleted { get; set; }

    }
}
