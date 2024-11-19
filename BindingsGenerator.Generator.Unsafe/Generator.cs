using BindingsGenerator.Core;
using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Models;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using BindingsGenerator.Generator.Unsafe.Internal.Processor.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Services;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors;
using DllExportScanner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GeneratorBase = BindingsGenerator.Generator.Unsafe.Internal.Models.Generator.GeneratorBase;



namespace BindingsGenerator.Generator.Unsafe
{
    public class Generator : BindingsGenerator.Core.GeneratorBase
    {
        private readonly IServiceCollection _serviceCollection;

        public Generator()
        {
            _serviceCollection = new ServiceCollection();

            //Processing
            _serviceCollection.AddSingleton<ASTParser>();
            _serviceCollection.AddSingleton<ITokenStore, TokenStore>();
            _serviceCollection.AddSingleton<ASTProcessor>();
            _serviceCollection.AddTransient<NamespaceHelper>();

            //Processors
            _serviceCollection.AddTransient<ProcessorBase, ArrayProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, DefaultTypeProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, EnumerationProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, FunctionProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, MacroProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, NamespaceProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, ObjectProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, PointerProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, TypedefProcessor>();
            _serviceCollection.AddTransient<ProcessorBase, UnionProcessor>();

            //Generation
            _serviceCollection.AddSingleton<CodeGenerator>();
            _serviceCollection.AddTransient<TypeHelper>();
            _serviceCollection.AddTransient<ParameterHelper>();

            //Generators
            _serviceCollection.AddTransient<GeneratorBase, ComObjectGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, EnumerationGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, FixedSizeArrayGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, FrameworkGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, FunctionDelegateGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, FunctionFacadeGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, FunctionVectorGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, ImportFunctionGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, LibraryGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, MacroGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, NamespaceGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, ObjectGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, TypedefGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, UnionGenerator>();
            _serviceCollection.AddTransient<GeneratorBase, VTableGenerator>();
        }

        public override void GenerateBindings(IGeneratedFileCollector collector, IGenerationLogCollector logger)
        {
            var context = new GeneratorContext() { Options = Options };
            //Clear serice-collection
            _serviceCollection.RemoveAll<IGeneratedFileCollector>();
            _serviceCollection.RemoveAll<IGenerationLogCollector>();
            _serviceCollection.RemoveAll<GeneratorContext>();

            //Init service-collection
            _serviceCollection.AddSingleton(collector);
            _serviceCollection.AddSingleton(logger);
            _serviceCollection.AddSingleton(context);

            PrintInfo(logger);

            using (var serviceProvider = _serviceCollection.BuildServiceProvider())
            {
                // parse headers
                var astParser = serviceProvider.GetRequiredService<ASTParser>();
                var astContext = astParser.Parse(Options.TranslationUnits.ToArray());

                // process
                List<FunctionExport> functionExports = new();
                var exportScanner = DllExportScannerFactory.GetScanner();
                foreach (var binary in Options.Binaries)
                {
                    var exports = exportScanner.ListExports(binary, Options.BinaryDirs.Select(x => Path.GetFullPath(x.DirectoryName)).ToArray());
                    functionExports.AddRange(exports);
                }
                foreach (var export in functionExports
                        .GroupBy(x => x.FunctionSignature)
                        // Eliminate duplicated names
                        .Select(x => x.First()))
                {
                    context.ExportMap.TryAdd(export.FunctionSignature, export);
                }

                var processor = serviceProvider.GetRequiredService<ASTProcessor>();

                try
                {
                    processor.Process(astContext);
                }
                catch (Exception ex)
                {
                    logger.LogError("", ex.ToString());
                    throw;
                }

                // generate files
                var generator = serviceProvider.GetRequiredService<CodeGenerator>();

                try
                {
                    generator.Generate();
                }
                catch (Exception ex)
                {
                    logger.LogError("", ex.ToString());
                    throw;
                }
            }

            var typeIDs = context.Definitions.Values.Select(x => x.ID).Where(x => !string.IsNullOrEmpty(x)).ToHashSet();
            foreach (var id in typeIDs)
            {
                collector.RegisterParsedType(id);
            }

            return;
        }
    }
}
