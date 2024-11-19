using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    [DebuggerDisplay("Namespace: {Name, nq}")]
    internal sealed record NamespaceDefinition : ScopingEntityDefinitionBase, IFinalDefinition
    {
        public required bool IsRoot { get; init; }
    }
}
