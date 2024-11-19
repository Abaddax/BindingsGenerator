using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    [DebuggerDisplay("{Name} = {Value}")]
    internal record EnumerationItem : IDocumentable, IObsoletable
    {
        /// <summary>
        /// Enum item name
        /// </summary>
        public required string Name { get; init; }
        /// <summary>
        /// Enum item value
        /// </summary>
        public required string Value { get; init; }
        public string? Documentation { get; init; }
        public Obsoletion Obsoletion { get; init; }
    }
}
