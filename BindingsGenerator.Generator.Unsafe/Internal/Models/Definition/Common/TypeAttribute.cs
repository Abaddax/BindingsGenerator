namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    [Flags]
    internal enum AttributeUsage
    {
        None = 0,
        ReturnValue = 1 << 0,
        Parameter = 1 << 1,
        Field = 1 << 2,
        COM = 1 << 3,
    }
    internal sealed record TypeAttribute
    {
        /// <summary>
        /// Where does the attribute apply
        /// </summary>
        public required AttributeUsage Usage { get; init; }
        /// <summary>
        /// Attribute string
        /// </summary>
        /// <example>MarshalAs(UnmanagedType.LPStr)</example>
        public required string Attribute { get; init; }
    }
}
