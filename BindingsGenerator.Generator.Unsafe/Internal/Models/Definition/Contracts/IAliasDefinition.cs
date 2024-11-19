namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts
{
    internal interface IAliasDefinition : IFinalDefinition
    {
        /// <summary>
        /// Alias for type
        /// </summary>
        ITypeToken Type { get; }

        ITypeToken<TFinalDefinition>? TryCast<TFinalDefinition>() where TFinalDefinition : class, IFinalDefinition;
    }
}
