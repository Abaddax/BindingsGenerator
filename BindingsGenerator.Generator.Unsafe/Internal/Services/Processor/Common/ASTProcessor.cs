using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Models;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using CppSharp.AST;
using Microsoft.Extensions.DependencyInjection;
using Type = CppSharp.AST.Type;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common
{
    internal sealed class ASTProcessor
    {
        readonly IServiceProvider _serviceProvider;
        readonly ITokenStore _tokenStore;
        readonly IGenerationLogCollector _logger;

        readonly GeneratorContext _context;

        readonly object _processorsLock = new();
        ProcessorBase[] _processors = Array.Empty<ProcessorBase>();

        public ASTProcessor(
            IServiceProvider serviceProvider,
            ITokenStore tokenStore,
            IGenerationLogCollector logger,
            GeneratorContext context)
        {
            _serviceProvider = serviceProvider;
            _tokenStore = tokenStore;
            _logger = logger;

            _context = context;
        }

        public void Process(ASTContext context)
        {
            var headers = _context.Options.TranslationUnits.SelectMany(x => x.Includes).Select(x => Path.GetFileName(x.FileName)).ToHashSet();
            var units = context.TranslationUnits.Where(x => headers.Contains(x.FileName) || !x.IsSystemHeader).ToArray();

            lock (_processorsLock)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    _processors = scope.ServiceProvider.GetServices<ProcessorBase>().ToArray();
                    foreach (var translationUnit in units)
                    {
                        //TODO:
                        //if (_options.IgnoredHeaders.Contains(translationUnit.Name))
                        //    continue;
                        foreach (var processor in _processors)
                        {
                            var processed = processor.Process(translationUnit);
                            foreach (var definition in processed)
                            {
                                _tokenStore.Store(definition);
                            }
                        }
                    }
                }
                _processors = Array.Empty<ProcessorBase>();
            }
        }

        public IDefinition? TryProcessDeclaration(Declaration declaration, Declaration? parent)
        {
            foreach (var processor in _processors)
            {
                IDefinition? definition = processor.Process(declaration, parent);
                if (definition != null)
                {
                    var token = _tokenStore.Store(definition);
                    return _tokenStore.TryGetDefinition(token);
                }
            }
            return null;
        }
        public IDefinition? TryProcessType(Type type, Declaration parent)
        {
            foreach (var processor in _processors)
            {
                IDefinition? definition = processor.Process(type, parent);
                if (definition != null)
                {
                    var token = _tokenStore.Store(definition);
                    return _tokenStore.TryGetDefinition(token);
                }
            }
            return null;
        }

    }
}
