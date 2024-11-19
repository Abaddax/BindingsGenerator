using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Core.Models;
using System;

namespace BindingsGenerator.Core
{
    public abstract class GeneratorBase : IGenerator
    {
        public GeneratorOptions Options { get; set; } = new GeneratorOptions();

        public void PrintInfo(IGenerationLogCollector logger)
        {
            logger.LogInfo($"Working path: {Environment.CurrentDirectory}");

            foreach (var includeDir in Options.IncludeDirs)
            {
                logger.LogInfo($"Header path: {includeDir.DirectoryName}");
            }
            foreach (var translationUnit in Options.TranslationUnits)
            {
                foreach (var include in translationUnit.Includes)
                {
                    logger.LogInfo($"Header: {include.FileName}");
                }
            }

            foreach (var binaryDir in Options.BinaryDirs)
            {
                logger.LogInfo($"Binary path: {binaryDir.DirectoryName}");
            }
            foreach (var binary in Options.Binaries)
            {
                logger.LogInfo($"Binary: {binary.FileName}");
            }

            foreach (var define in Options.Defines)
            {
                logger.LogInfo($"Define: {define.Value}");
            }

            logger.LogInfo($"RootNamespace: {Options.RootNamespace}");
            logger.LogInfo($"StaticTypename: {Options.StaticTypename}");
        }

        public abstract void GenerateBindings(IGeneratedFileCollector collector, IGenerationLogCollector logger);
    }
}
