using BindingsGenerator.Core.Models;
using System.Collections.Generic;

namespace BindingsGenerator.Core.Contracts
{
    public interface IGenerationLogCollector
    {
        IEnumerable<GenerationLogEntry> GenerationLog { get; }
        void AppendLogEntry(GenerationLogEntry entry);
    }
}
