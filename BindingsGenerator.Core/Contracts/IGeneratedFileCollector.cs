using BindingsGenerator.Core.Models;
using System.Collections.Generic;

namespace BindingsGenerator.Core.Contracts
{
    public interface IGeneratedFileCollector
    {
        IEnumerable<GeneratedSourceFile> GeneratedSourceFiles { get; }
        void AppendGeneratedFile(GeneratedSourceFile file);

        IEnumerable<string> ParsedTypes { get; }
        void RegisterParsedType(string type);
    }
}
