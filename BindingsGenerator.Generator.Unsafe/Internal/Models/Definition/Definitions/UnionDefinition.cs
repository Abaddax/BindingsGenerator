using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for all unions
    /// </summary>
    [DebuggerDisplay("Union: {Name, nq}")]
    internal sealed record UnionDefinition : ScopingEntityDefinitionBase, IDefinition, IFinalDefinition
    {
        /// <summary>
        /// Fields in the union
        /// </summary>
        public MemberField[] Fields { get; set; } = Array.Empty<MemberField>();
        /// <summary>
        /// Type declaration is completed
        /// </summary>
        public bool IsCompleted { get; set; }

    }
}
