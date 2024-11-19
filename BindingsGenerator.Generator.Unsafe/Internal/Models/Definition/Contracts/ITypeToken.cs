namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts
{
    internal interface ITypeToken
    {
        /// <summary>
        /// Store that contains the corresponding <see cref="IDefinition"/>
        /// </summary>
        ITokenStore Store { get; }
        /// <summary>
        /// Entry to the corresponding <see cref="IDefinition"/>
        /// </summary>
        string Token { get; }

        /// <summary>
        /// Load corresponding <see cref="IDefinition"/> from store
        /// </summary>
        public IDefinition? Definition { get; }
        /// <summary>
        /// Load corresponding <see cref="IFinalDefinition"/> from store
        /// </summary>
        public IFinalDefinition? FinalDefinition { get; }
    }
    internal interface ITypeToken<out TFinalDefinition> : ITypeToken where TFinalDefinition : class, IFinalDefinition
    {
        /// <summary>
        /// Required <see cref="Type"/> for the corresponding <see cref="IDefinition"/>
        /// </summary>
        Type RequiredType { get; }

        /// <summary>
        /// Load corresponding <see cref="TFinalDefinition"/> from store
        /// </summary>
        new TFinalDefinition? FinalDefinition { get; }
    }
}
