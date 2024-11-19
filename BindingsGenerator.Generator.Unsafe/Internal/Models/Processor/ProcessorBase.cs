using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common;
using CppSharp.AST;
using DllExportScanner;
using Microsoft.Extensions.DependencyInjection;
using Type = CppSharp.AST.Type;

namespace BindingsGenerator.Generator.Unsafe.Internal.Models.Processor
{
    internal abstract class ProcessorBase
    {
        readonly HashSet<string> _currentlyProcessing = new();

        readonly ITokenStore _tokenStore;
        readonly IGenerationLogCollector _logger;
        readonly ASTProcessor _baseProcessor;
        readonly GeneratorContext _context;

        protected IGenerationLogCollector? Logger => _logger;
        protected GeneratorContext Context => _context;

        protected ProcessorBase(IServiceProvider serviceProvider)
        {
            _tokenStore = serviceProvider.GetRequiredService<ITokenStore>();
            _logger = serviceProvider.GetRequiredService<IGenerationLogCollector>();
            _baseProcessor = serviceProvider.GetRequiredService<ASTProcessor>();
            _context = serviceProvider.GetRequiredService<GeneratorContext>();
        }

        /// <summary>
        /// List of processable Types
        /// </summary>
        public virtual IEnumerable<System.Type> Processable() => Array.Empty<System.Type>();


        private IEnumerable<IDefinition> HandleDeclarations(DeclarationContext declarationContext)
        {
            foreach (var declaration in declarationContext.Declarations)
            {
                foreach (var processableType in Processable())
                {
                    if (declaration.GetType().IsAssignableTo(processableType))
                    {
                        var definiton = Process(declaration, null);
                        if (definiton != null)
                            yield return definiton;
                    }
                }
            }
        }
        private IEnumerable<IDefinition> HandlePreprocessedEntities(Declaration declaration)
        {
            foreach (var preprocessedEntity in declaration.PreprocessedEntities)
            {
                foreach (var processableType in Processable())
                {
                    if (preprocessedEntity.GetType().IsAssignableTo(processableType))
                    {
                        var definiton = Process(preprocessedEntity, declaration);
                        if (definiton != null)
                            yield return definiton;
                    }
                }
            }

            if (declaration is DeclarationContext declarationContext)
            {
                foreach (var childDeclaration in declarationContext.Declarations)
                {
                    var childs = HandlePreprocessedEntities(childDeclaration);
                    foreach (var child in childs)
                    {
                        yield return child;
                    }
                }
            }
        }

        /// <summary>
        /// Process all declarations in the translationUnit
        /// </summary>
        /// <returns>List of processed Definitions</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IEnumerable<IDefinition> Process(TranslationUnit translationUnit)
        {
            //Process declarations
            foreach (var declaration in HandleDeclarations(translationUnit))
            {
                yield return declaration;
            }

            //Process preprocessedEntities (macros)
            foreach (var macro in HandlePreprocessedEntities(translationUnit))
            {
                yield return macro;
            }
        }
        /// <summary>
        /// Process declaration
        /// </summary>
        /// <returns>Definiton or null</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IDefinition? Process(Declaration declaration, Declaration? parent)
        {
            if (!Processable().Contains(declaration.GetType()))
                return null;

            return ProcessDeclaration(declaration, parent);
        }
        /// <summary>
        /// Process type
        /// </summary>
        /// <returns>Definiton or null</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IDefinition? Process(Type type, Declaration parent)
        {
            if (!Processable().Contains(type.GetType()))
                return null;

            return ProcessType(type, parent);
        }
        /// <summary>
        /// Process macro
        /// </summary>
        /// <returns>Definiton or null</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IDefinition? Process(PreprocessedEntity preprocessedEntity, Declaration parent)
        {
            if (!Processable().Contains(preprocessedEntity.GetType()))
                return null;

            return ProcessPreprocessedEntity(preprocessedEntity, parent);
        }

        protected virtual IDefinition? ProcessDeclaration(Declaration declaration, Declaration? parent)
        {
            return null;
        }
        protected virtual IDefinition? ProcessType(Type type, Declaration parent)
        {
            return null;
        }
        protected virtual IDefinition? ProcessPreprocessedEntity(PreprocessedEntity preprocessedEntity, Declaration parent)
        {
            return null;
        }

        #region Helper
        /// <summary>
        /// Try to get an existing definition by token
        /// </summary>
        /// <returns>null on failure</returns>
        protected TDefinition? TryGetDefinition<TDefinition>(ITypeToken token) where TDefinition : class, IDefinition
        {
            return _tokenStore.TryGetDefinition(token) as TDefinition;
        }
        /// <summary>
        /// Try to get an existing definition by token
        /// </summary>
        /// <returns>null on failure</returns>
        protected TDefinition? TryGetDefinition<TDefinition>(string token) where TDefinition : class, IDefinition
        {
            return TryGetDefinition<TDefinition>(new TypeToken() { Token = token, Store = _tokenStore });
        }
        /// <summary>
        /// Try to store a definition
        /// </summary>
        /// <returns>null on failure</returns>
        protected ITypeToken? TryStoreDefinition(IDefinition definition)
        {
            try
            {
                return _tokenStore.Store(definition);
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Try to store a definition
        /// </summary>
        /// <returns>null on failure</returns>
        protected ITypeToken<TDefinition>? TryStoreDefinition<TDefinition>(TDefinition definition) where TDefinition : class, IFinalDefinition
        {
            try
            {
                return _tokenStore.Store(definition) as ITypeToken<TDefinition>;
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// Get definition for declaration
        /// </summary>
        protected IDefinition? TryGetDefinition(Declaration declaration, Declaration? parent)
        {
            return _baseProcessor.TryProcessDeclaration(declaration, parent);
        }
        /// <summary>
        /// Get definition for type
        /// </summary>
        protected IDefinition? TryGetDefinition(Type type, Declaration parent)
        {
            return _baseProcessor.TryProcessType(type, parent);
        }
        /// <summary>
        /// Get definition for declaration
        /// </summary>
        protected IDefinition GetDefinition(Declaration declaration, Declaration? parent)
        {
            var definition = TryGetDefinition(declaration, parent);
            if (definition == null)
                throw new InvalidOperationException($"Declaration is not processable with current settings. Declaration: {declaration?.GetType()}");
            return definition;
        }
        /// <summary>
        /// Get definition for type
        /// </summary>
        protected IDefinition GetDefinition(Type type, Declaration parent)
        {
            var definition = TryGetDefinition(type, parent);
            if (definition == null)
                throw new InvalidOperationException($"Type is not processable with current settings. Type: {type?.GetType()}");
            return definition;
        }

        /// <summary>
        /// Get token for declaration
        /// </summary>
        protected ITypeToken GetToken(Declaration declaration, Declaration? parent)
        {
            return _tokenStore.Store(GetDefinition(declaration, parent));
        }
        /// <summary>
        /// Get token for type
        /// </summary>
        protected ITypeToken GetToken(Type type, Declaration parent)
        {
            return _tokenStore.Store(GetDefinition(type, parent));
        }


        protected FunctionExport? TryGetFunctionExport(string functionName)
        {
            if (Context.ExportMap.TryGetValue(functionName, out var export))
                return export;
            return null;
        }
        #endregion

        #region Processing helper
        protected sealed class ProcessDisposable<TDefinition> : IDisposable where TDefinition : class, IDefinition
        {
            private readonly Action _action;
            private readonly ITypeToken _token;
            public TDefinition Definition => _token.Definition as TDefinition;
            public ProcessDisposable(ITypeToken token, Action action)
            {
                _action = action;
                _token = token;
            }
            public void Dispose() => _action();
        }
        /// <summary>
        /// Notify that the current type is beeing processed. <br/>
        /// Break recursiv loops
        /// </summary>      
        protected ProcessDisposable<EntityDefinition> BeginProcess(string id)
        {
            //Preload definitions if needed
            if (TryGetDefinition<IDefinition>(id) == null)
                TryStoreDefinition(new EntityDefinition() { ID = id, Name = id });

            if (!_currentlyProcessing.Add(id))
                throw new InvalidOperationException($"Type {id} is already beeing processed.");

            return new ProcessDisposable<EntityDefinition>(
                new TypeToken() { Token = id, Store = _tokenStore },
                () =>
                {
                    _currentlyProcessing.Remove(id);
                });
        }
        protected ProcessDisposable<TDefinition> BeginProcess<TDefinition>(TDefinition processingDefinition) where TDefinition : class, IDefinition
        {
            //Preload definitions if needed
            if (TryGetDefinition<IDefinition>(processingDefinition.ID) == null)
                TryStoreDefinition(processingDefinition);

            if (!_currentlyProcessing.Add(processingDefinition.ID))
                throw new InvalidOperationException($"Type {processingDefinition.ID} is already beeing processed.");

            return new ProcessDisposable<TDefinition>(
                new TypeToken() { Token = processingDefinition.ID, Store = _tokenStore },
                () =>
                {
                    _currentlyProcessing.Remove(processingDefinition.ID);
                });
        }
        protected IDefinition? TryGetProcessingDefinition(string id)
        {
            if (_currentlyProcessing.Contains(id))
                return TryGetDefinition<IDefinition>(id);
            return null;
        }
        protected bool IsProcessing(string id)
        {
            return TryGetProcessingDefinition(id) != null;
        }
        #endregion

    }
}
