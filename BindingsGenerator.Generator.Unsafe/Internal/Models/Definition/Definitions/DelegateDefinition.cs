using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for function pointer (non imported)
    /// </summary>
    [DebuggerDisplay("Delegate: {Name, nq}")]
    internal sealed record DelegateDefinition : FunctionDefinitionBase, IDefinition, IFinalDefinition
    {

    }
}
