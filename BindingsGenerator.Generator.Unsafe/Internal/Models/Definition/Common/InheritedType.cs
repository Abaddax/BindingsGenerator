using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using CppSharp.AST;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    internal sealed record InheritedType
    {
        /// <summary>
        /// Type that is inherited
        /// </summary>
        public required ITypeToken<ObjectDefinition> Type { get; init; }
        /// <summary>
        /// Memory offset of the inherited type
        /// </summary>
        public required uint TypeOffset { get; init; }
        /// <summary>
        /// Access specifier (public)
        /// </summary>
        public required AccessSpecifier AccessSpecifier { get; init; }
    }
}
