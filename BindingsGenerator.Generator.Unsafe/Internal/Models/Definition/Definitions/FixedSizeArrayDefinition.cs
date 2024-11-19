using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for all fixed size arrays
    /// </summary>
    /// <example>char[10]</example>
    [DebuggerDisplay("FixedSizeArray: {Name, nq}")]
    internal sealed record FixedSizeArrayDefinition : EntityDefinition, IDefinition, IFinalDefinition
    {
        /// <summary>
        /// Element type
        /// </summary>
        public required ITypeToken ElementType { get; init; }
        /// <summary>
        /// Fixed size
        /// </summary>
        public required int Length { get; init; }
        public bool IsPrimitive { get; init; }
        public bool IsPointer { get; init; }
    }
}
