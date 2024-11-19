using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Models.Processor
{
    /// <summary>
    /// Token to a <see cref="IDefinition"/>
    /// </summary>
    [DebuggerDisplay("{Token}")]
    internal record TypeToken : ITypeToken
    {
        /// <summary>
        /// Store that contains the corresponding <see cref="IDefinition"/>
        /// </summary>
        public required ITokenStore Store { get; init; }
        /// <summary>
        /// Entry to the corresponding <see cref="IDefinition"/>
        /// </summary>
        public required string Token { get; init; }

        /// <summary>
        /// Load corresponding <see cref="IDefinition"/> from store
        /// </summary>
        public IDefinition? Definition => Store?.TryGetDefinition(this);
        /// <summary>
        /// Load corresponding <see cref="IFinalDefinition"/> from store
        /// </summary>
        public IFinalDefinition? FinalDefinition => Store?.TryGetFinalDefinition(this);
    }

    /// <summary>
    /// Token to a <see cref="IFinalDefinition"/>
    /// </summary>
    /// <typeparam name="TFinalDefinition"></typeparam>
    [DebuggerDisplay("{Token} ({RequiredType.Name})")]
    internal record TypeToken<TFinalDefinition> : TypeToken, ITypeToken<TFinalDefinition> where TFinalDefinition : class, IFinalDefinition
    {
        /// <summary>
        /// Required <see cref="Type"/> for the corresponding <see cref="IDefinition"/>
        /// </summary>
        public Type RequiredType => typeof(TFinalDefinition);

        /// <summary>
        /// Load corresponding <see cref="TFinalDefinition"/> from store
        /// </summary>
        public new TFinalDefinition? FinalDefinition => Store?.TryGetFinalDefinition(this);
    }
}
