using BindingsGenerator.Core.Models;

namespace BindingsGenerator.Core.Contracts
{
    public interface IGenerator
    {
        GeneratorOptions Options { get; set; }

        void GenerateBindings(IGeneratedFileCollector collector, IGenerationLogCollector logger);
    }

}
