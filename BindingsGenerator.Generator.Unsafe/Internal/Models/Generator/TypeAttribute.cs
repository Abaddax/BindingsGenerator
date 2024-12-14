namespace AutoGenBindings.Generator.Unsafe.Internal.Models.Generator
{
    internal sealed record TypeAttribute
    {
        /// <summary>
        /// Where does the attribute apply
        /// </summary>
        public required Usage Usage { get; init; }
        /// <summary>
        /// Attribute string
        /// </summary>
        /// <example>MarshalAs(UnmanagedType.LPStr)</example>
        public required string Attribute { get; init; }
    }
}
