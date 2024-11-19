using BindingsGenerator.Core.Models;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using DllExportScanner;
using System.Collections.Concurrent;

namespace BindingsGenerator.Generator.Unsafe.Internal.Models
{
    internal record GeneratorContext
    {
        public required GeneratorOptions Options { get; init; }
        public IDictionary<string, IDefinition> Definitions { get; } = new ConcurrentDictionary<string, IDefinition>();
        public IDictionary<string, FunctionExport> ExportMap { get; } = new ConcurrentDictionary<string, FunctionExport>();
    }
}
