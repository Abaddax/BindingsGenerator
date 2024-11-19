using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions
{
    /// <summary>
    /// Definition for all structs/classes
    /// </summary>
    [DebuggerDisplay("Object: {Name, nq}")]
    internal sealed record ObjectDefinition : ScopingEntityDefinitionBase, IDefinition, IFinalDefinition
    {
        /// <summary>
        /// Object fields
        /// </summary>
        public MemberField[] Fields { get; set; } = Array.Empty<MemberField>();
        /// <summary>
        /// Object function (also static)
        /// </summary>
        public MemberFunction[] Functions { get; set; } = Array.Empty<MemberFunction>();
        /// <summary>
        /// Inherited types
        /// </summary>
        public InheritedType[] Bases { get; set; } = Array.Empty<InheritedType>();
        /// <summary>
        /// This type is inherited by some type
        /// </summary>
        public bool IsInherited { get; set; }
        /// <summary>
        /// Type declaration is completed
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// Type is a Win32-COM Object with this GUID
        /// </summary>
        public Guid? IsComObject { get; set; }
    }
}
