namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Common
{
    /// <summary>
    /// Base for exported/importable functions from a DLL
    /// </summary>
    internal abstract record ImportFunctionDefinitionBase : FunctionDefinitionBase
    {
        /// <summary>
        /// Library where is function is exported
        /// </summary>
        public required string LibraryName { get; init; }
        /// <summary>
        /// Entrypoint for the function import
        /// </summary>
        public required string FunctionSignature { get; init; }
    }
}
