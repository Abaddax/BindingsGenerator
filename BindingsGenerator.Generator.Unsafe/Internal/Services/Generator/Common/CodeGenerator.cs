using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Generator.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Models;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using Microsoft.Extensions.DependencyInjection;
using GeneratorBase = BindingsGenerator.Generator.Unsafe.Internal.Models.Generator.GeneratorBase;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common
{
    internal sealed class CodeGenerator
    {
        readonly IServiceProvider _serviceProvider;
        readonly ITokenStore _tokenStore;
        readonly IGenerationLogCollector _logger;

        readonly GeneratorContext _context;

        readonly object _generatorsLock = new();
        GeneratorBase[] _generators = Array.Empty<GeneratorBase>();

        public CodeGenerator(
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

        public void Generate()
        {
            lock (_generatorsLock)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    _generators = scope.ServiceProvider.GetServices<GeneratorBase>().ToArray();
                    //Generate
                    foreach (var generator in _generators)
                    {
                        generator.Generate();
                    }

                }
                _generators = Array.Empty<GeneratorBase>();
            }
        }

        public TypeMapping? GenerateMapping(IDefinition? definition, Usage usage)
        {
            if (definition == null)
                return null;
            foreach (var generator in _generators)
            {
                TypeMapping? mapping = generator.GenerateMapping(definition, usage);
                if (mapping != null && mapping.TypeUsage.HasFlag(usage))
                {
                    return mapping;
                }
            }
            return null;
        }
        public NameScope? GenerateScope(IDefinition? definition, Usage usage)
        {
            if (definition == null)
                return null;
            foreach (var generator in _generators)
            {
                NameScope? scope = generator.GenerateScope(definition, usage);
                if (scope != null)
                {
                    return scope;
                }
            }
            return null;
        }

    }
}
