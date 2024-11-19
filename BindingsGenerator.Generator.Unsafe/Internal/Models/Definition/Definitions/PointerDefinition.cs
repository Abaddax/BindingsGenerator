using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for all pointers
    /// </summary>
    [DebuggerDisplay("{Name, nq}")]
    internal sealed record PointerDefinition : EntityDefinition, IFinalDefinition
    {
        /// <summary>
        /// Autodeterminated, init with null
        /// </summary>
        public override required string Name
        {
            get => $"{Type.Definition!.Name}*";
            init {/*Noop*/}
        }
        /// <summary>
        /// Type that the pointer points to
        /// </summary>
        public required ITypeToken Type { get; init; }
    }
}
