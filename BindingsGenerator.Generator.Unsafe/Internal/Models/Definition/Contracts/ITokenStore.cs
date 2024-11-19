namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts
{
    /// <summary>
    /// Store for <see cref="IDefinition"/> and corresponding <see cref="ITypeToken"/>
    /// </summary>
    internal interface ITokenStore
    {
        /// <summary>
        /// Store definition in store
        /// </summary>
        /// <returns>Stored token</returns>
        ITypeToken Store(IDefinition definition);
        /// <summary>
        /// Store final-definition in store
        /// </summary>
        /// <returns>Stored token</returns>
        ITypeToken Store(IFinalDefinition definition);
        /// <summary>
        /// Store final-definition in store
        /// </summary>
        /// <returns>Stored token</returns>
        ITypeToken<TFinalDefinition> Store<TFinalDefinition>(TFinalDefinition definition) where TFinalDefinition : class, IFinalDefinition;

        /// <summary>
        /// Reads definition from store
        /// </summary>
        /// <returns>Definition or null</returns>
        IDefinition? TryGetDefinition(ITypeToken token);
        /// <summary>
        /// Reads final-definition from store
        /// </summary>
        /// <returns>Final-definition or null</returns>
        IFinalDefinition? TryGetFinalDefinition(ITypeToken token);
        /// <summary>
        /// Reads final-definition from store
        /// </summary>
        /// <returns>Final-definition or null</returns>
        TFinalDefinition? TryGetFinalDefinition<TFinalDefinition>(ITypeToken<TFinalDefinition> token) where TFinalDefinition : class, IFinalDefinition;
    }
}
