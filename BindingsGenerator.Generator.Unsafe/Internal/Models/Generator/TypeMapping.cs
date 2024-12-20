using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;

namespace BindingsGenerator.Generator.Unsafe.Internal.Generator.Common
{
    internal record TypeMapping
    {
        public required string TypeName { get; init; }
        public required Usage TypeUsage { get; init; }
        public TypeAttribute[] MarshalAs { get; init; } = Array.Empty<TypeAttribute>();
    }
}
