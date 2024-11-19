using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using CppSharp.AST;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    [DebuggerDisplay("{IsStatic?\"static \":\"\", nq}{AccessSpecifier} {FieldType,nq} {Name,nq}")]
    internal sealed record MemberField : IDocumentable, IObsoletable
    {
        /// <summary>
        /// Name of the field
        /// </summary>
        public required string Name { get; init; }
        /// <summary>
        /// Type of the field
        /// </summary>
        public required ITypeToken FieldType { get; init; }
        /// <summary>
        /// Memory offset of the field
        /// </summary>
        public required uint FieldOffset { get; init; }
        /// <summary>
        /// Access specifier (public)
        /// </summary>
        public required AccessSpecifier AccessSpecifier { get; init; }
        /// <summary>
        /// This field is static
        /// </summary>
        public bool IsStatic { get; init; }
        /// <summary>
        /// This field is a VTable
        /// </summary>
        public bool IsVTable { get; init; }
        public string? Documentation { get; init; }
        public Obsoletion Obsoletion { get; init; }
    }
}
