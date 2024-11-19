using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for everything
    /// </summary>
    [DebuggerDisplay("{Name, nq}")]
    internal record EntityDefinition : IDefinition
    {
        public required string ID { get; init; }
        public virtual required string Name { get; init; }
        public string File { get; init; } = string.Empty;
    }
}
