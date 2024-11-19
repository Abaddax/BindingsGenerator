using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Models;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;

namespace BindingsGenerator.Generator.Unsafe.Internal.Processor.Common
{
    internal sealed class TokenStore : ITokenStore
    {
        readonly IDictionary<string, IDefinition> _definitions;

        public TokenStore(GeneratorContext context)
        {
            _definitions = context.Definitions;
        }

        #region ITokenStore
        public ITypeToken Store(IDefinition definition)
        {
            //Add
            if (!_definitions.TryGetValue(definition.ID, out var existingDefinition))
            {
                _definitions.Add(definition.ID, definition);
                return GetToken(definition);
            }
            //Override non final
            if (existingDefinition is not IFinalDefinition finalDefinition)
            {
                _definitions[definition.ID] = definition;
                return GetToken(definition);
            }
            //Update final
            if (finalDefinition.GetType() == definition.GetType())
            {
                _definitions[definition.ID] = definition;
                return GetToken(definition);
            }
            throw new InvalidOperationException("Final types can not change.");
        }
        public ITypeToken Store(IFinalDefinition definition)
            => Store((IDefinition)definition);
        ITypeToken<TFinalDefinition> ITokenStore.Store<TFinalDefinition>(TFinalDefinition definition) where TFinalDefinition : class
            => (ITypeToken<TFinalDefinition>)Store((IFinalDefinition)definition);

        public IDefinition? TryGetDefinition(ITypeToken token)
        {
            if (!_definitions.TryGetValue(token.Token, out var definition))
                return null;
            return definition;
        }
        public IFinalDefinition? TryGetFinalDefinition(ITypeToken token)
            => TryGetDefinition(token) as IFinalDefinition;
        TFinalDefinition? ITokenStore.TryGetFinalDefinition<TFinalDefinition>(ITypeToken<TFinalDefinition> token) where TFinalDefinition : class
            => TryGetFinalDefinition((ITypeToken)token) as TFinalDefinition;
        #endregion

        private ITypeToken GetToken(IDefinition definition)
        {
            if (definition is not IFinalDefinition finalDefinition)
                return new TypeToken() { Token = definition.ID, Store = this };

            //Return TypeToken<TFinalDefinition>
            var genericFinalDefinitionType = typeof(TypeToken<>).MakeGenericType(finalDefinition.GetType());
            TypeToken token = Activator.CreateInstance(genericFinalDefinitionType) as TypeToken;
            //Set Init-only properties
            genericFinalDefinitionType.GetProperty(nameof(TypeToken.Token))?.SetValue(token, finalDefinition.ID);
            genericFinalDefinitionType.GetProperty(nameof(TypeToken.Store))?.SetValue(token, this);
            return token;
        }
    }
}
