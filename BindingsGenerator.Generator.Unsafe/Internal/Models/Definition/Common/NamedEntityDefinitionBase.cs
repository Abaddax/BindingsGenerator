using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;

///<summary>
///lol
///</summary>
namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    /// <summary>
    /// Base for user-defined complex types (union, struct, class)
    /// </summary>
    internal abstract record NamedEntityDefinitionBase : EntityDefinition, IDocumentable, IObsoletable
    {
        public string? Documentation { get; init; }
        public Obsoletion Obsoletion { get; init; }
    }
}
