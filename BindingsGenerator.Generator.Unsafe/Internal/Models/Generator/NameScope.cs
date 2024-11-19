namespace BindingsGenerator.Generator.Unsafe.Internal.Models.Generator
{
    internal record NameScope
    {
        /// <summary>
        /// Name of this scope
        /// </summary>
        public required string ScopeName { get; init; }
        public NameScope? ParentScope { get; init; }
        /// <summary>
        /// True -> namespace, false -> nested type
        /// </summary>
        public required bool IsNamespace { get; init; }
        /// <summary>
        /// Prefix of the namespace (namespace, public partial class, ...)
        /// </summary>
        public required string? ScopePrefix { get; init; }
    }
}
