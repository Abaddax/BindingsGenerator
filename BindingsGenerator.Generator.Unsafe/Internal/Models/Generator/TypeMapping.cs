using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;

namespace BindingsGenerator.Generator.Unsafe.Internal.Generator.Common
{
    internal record TypeMapping
    {
        public required string Typename { get; init; }
        public TypeAttribute? MarshalAs { get; init; }
        public TypeAttribute[] CustomAttributes { get; init; } = Array.Empty<TypeAttribute>();
    }
}
