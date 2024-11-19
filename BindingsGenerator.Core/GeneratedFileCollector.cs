using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Core.Models;
using System.Collections.Generic;
using System.IO;

namespace BindingsGenerator.Core
{
    public class InMemoryGeneratedFileCollector : IGeneratedFileCollector
    {
        private readonly List<GeneratedSourceFile> generatedSourceFiles = new List<GeneratedSourceFile>();
        private readonly HashSet<string> parsedTypes = new HashSet<string>();

        public IEnumerable<GeneratedSourceFile> GeneratedSourceFiles => generatedSourceFiles;
        public IEnumerable<string> ParsedTypes => parsedTypes;

        public void AppendGeneratedFile(GeneratedSourceFile file)
        {
            lock (generatedSourceFiles)
            {
                generatedSourceFiles.Add(file);
            }
        }
        public void RegisterParsedType(string type)
        {
            lock (parsedTypes)
            {
                parsedTypes.Add(type);
            }
        }
    }
    public class ToFileGeneratedFileCollector : InMemoryGeneratedFileCollector
    {
        public void SaveFiles(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            foreach (var sourceFile in GeneratedSourceFiles)
            {
                File.WriteAllText($"{directory}/{sourceFile.Filename}", sourceFile.Content);
            }
        }
    }
}
