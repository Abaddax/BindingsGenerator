using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using CppSharp.AST;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    internal sealed record MemberFunction : IDocumentable, IObsoletable
    {
        /// <summary>
        /// Name of the function
        /// </summary>
        public required string Name { get; init; }
        /// <summary>
        /// Memory offset of the function (delegate)
        /// </summary>
        public required ITypeToken<FunctionDefinition> FunctionType { get; init; }
        /// <summary>
        /// Access specifier (public)
        /// </summary>
        public required AccessSpecifier AccessSpecifier { get; init; }
        /// <summary>
        /// Function kind (Normal, Constructor, Operator, ...)
        /// </summary>
        public required CXXMethodKind FunctionKind { get; init; }
        /// <summary>
        /// This function is virtual
        /// </summary>
        public bool IsVirtual { get; init; }
        /// <summary>
        /// This function overrides this base function
        /// </summary>
        public ITypeToken<FunctionDefinition>? Overridden { get; init; }
        //public bool IsOverride { get; init; }
        /// <summary>
        /// This function is static
        /// </summary>
        public bool IsStatic { get; init; }

        public string? Documentation { get; init; }
        public Obsoletion Obsoletion { get; init; }
    }
}
