using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using CppSharp.AST;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    [DebuggerDisplay("{FunctionType,nq} {Name,nq}")]
    internal sealed class VTableFunction : IDocumentable
    {
        /// <summary>
        /// Name of the function
        /// </summary>
        public required string Name { get; init; }
        /// <summary>
        /// Function pointer type
        /// </summary>
        public required ITypeToken<PointerDefinition> FunctionType { get; init; }
        /// <summary>
        /// Memory offset of the function pointer
        /// </summary>
        public required uint FunctionOffset { get; init; }
        /// <summary>
        /// Access specifier (public)
        /// </summary>
        public required AccessSpecifier AccessSpecifier { get; init; }
        public string? Documentation { get; init; }
    }
}
