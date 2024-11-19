using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using CMacroParser.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for all macros
    /// </summary>
    [DebuggerDisplay("Macro: {Name, nq}")]
    internal sealed record MacroDefinition : EntityDefinition, IFinalDefinition
    {
        public required IMacroDefinition? Definition { get; init; }
        public required string RawExpression { get; init; }
    }
}
