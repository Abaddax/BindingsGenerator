using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using CppSharp.AST;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    /// <summary>
    /// Base for all functions
    /// </summary>
    internal abstract record class FunctionDefinitionBase : ScopingEntityDefinitionBase, IDocumentable, IObsoletable
    {
        /// <summary>
        /// Type of the return value
        /// </summary>
        public ITypeToken ReturnType { get; init; }
        /// <summary>
        /// Function parameters
        /// </summary>
        public FunctionParameter[] Parameters { get; init; } = Array.Empty<FunctionParameter>();
        /// <summary>
        /// Calling convention for the function
        /// </summary>
        public required CallingConvention CallingConvention { get; init; }
        /// <summary>
        /// Comment for the return value
        /// </summary>
        public string? ReturnComment { get; init; }
        /// <summary>
        /// Occurance of this function as an overload
        /// </summary>
        public int Overload { get; set; }
    }
}
