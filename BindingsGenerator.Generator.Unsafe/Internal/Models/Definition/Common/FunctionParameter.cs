using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    [DebuggerDisplay("{IsConstant?\"const\":\"\",nq} {IsReference?\"ref\":\"\",nq} {Type,nq} {Name,nq}")]
    internal sealed record FunctionParameter : IDocumentable
    {
        /// <summary>
        /// Name of the function parameter
        /// </summary>
        public required string Name { get; init; }
        /// <summary>
        /// Type of the function parameter
        /// </summary>
        public required ITypeToken Type { get; init; }
        public string? Documentation { get; init; }
        public bool IsConstant { get; init; }
        /// <summary>
        /// Function parameter is passed by reference. <br/>
        /// Type is still Type*!!
        /// </summary>
        public bool IsReference { get; init; }
    }
}
