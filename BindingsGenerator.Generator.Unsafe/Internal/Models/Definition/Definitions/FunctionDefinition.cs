using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for standalone function
    /// </summary>
    [DebuggerDisplay("Function: {Name, nq}")]
    internal sealed record FunctionDefinition : ImportFunctionDefinitionBase, IDefinition, IFinalDefinition
    {
        /// <summary>
        /// Type of the function caller (null if static)
        /// </summary>
        public required ITypeToken<PointerDefinition>? Caller { get; init; }
    }
}
