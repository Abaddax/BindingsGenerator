using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for all unions
    /// </summary>
    [DebuggerDisplay("VTable: {Name}")]
    internal sealed record VTableDefinition : ScopedEntityDefinitionBase, IDefinition, IFinalDefinition
    {
        /// <summary>
        /// Object this vtable belongs to
        /// </summary>
        public required ITypeToken<ObjectDefinition> Object { get; init; }
        /// <summary>
        /// Functions in the VTable
        /// </summary>
        public VTableFunction[] Functions { get; set; } = Array.Empty<VTableFunction>();

    }
}
