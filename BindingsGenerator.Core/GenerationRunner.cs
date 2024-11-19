using BindingsGenerator.Core.Helper;
using BindingsGenerator.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BindingsGenerator.Core
{
    public static class GenerationRunner
    {
        public static void Run(GeneratorBase generator,
            string settingsFile = "settings.xml",
            string outDir = "generated",
            string logDir = "logs",
            params string[] ignoredTypesFiles)
        {
            ToFileGeneratedFileCollector collector = new ToFileGeneratedFileCollector();
            ConsoleGenerationLogCollector logger = new ConsoleGenerationLogCollector();

            Console.WriteLine("Reading settings...");
            //Ensure settings exist
            if (!File.Exists(settingsFile))
            {
                var dir = Path.GetDirectoryName(settingsFile);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (var fs = File.Create(settingsFile))
                {
                    var defaultOptions = new GeneratorOptions();
                    defaultOptions.Serialize(fs);
                }
            }

            //Read settings
            using (var fs = new FileStream(settingsFile, FileMode.Open))
            {
                generator.Options = fs.Deserialize<GeneratorOptions>();
            }
            foreach (var ignoredTypes in ignoredTypesFiles)
            {
                using (var fs = new FileStream(ignoredTypes, FileMode.Open))
                {
                    List<string> types = fs.Deserialize<List<string>>();
                    generator.Options.KnownTypes.AddRange(types);
                }
            }
            generator.Options.KnownTypes = new HashSet<string>(generator.Options.KnownTypes).ToList();

            Console.WriteLine("Starting generator...");

            try
            {
                generator.GenerateBindings(collector, logger);
            }
            catch (Exception ex)
            {
                logger.LogError("", ex.ToString());
                return;
            }

            Console.WriteLine("Writing output...");

            //Write output
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);
            collector.SaveFiles(outDir);

            //Write logs
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            var log = logger.GenerationLog.Select(x =>
            {
                switch (x.Level)
                {
                    case GenerationLogLevel.Warning:
                    case GenerationLogLevel.Error:
                        return $"[{x.Level}] {x.Message} (Code: {x.Code}, Location: {x.File}@{x.Line})";
                    case GenerationLogLevel.Info:
                        return $"[{x.Level}] {x.Message}";
                    default:
                        return string.Empty;
                }
            });
            File.WriteAllLines($"{logDir}/generation.log", log);

            //Write parsed headers
            using (var fs = new FileStream($"{logDir}/parsedTypes.xml", FileMode.Create))
            {
                collector.ParsedTypes.ToList().Serialize(fs);
            }

            Console.WriteLine("Finished.");
        }

        public static void Run(GeneratorBase generator, string[] args)
        {
            if (args?.Length < 1)
            {
                Console.WriteLine("Invalid arguments.");
                Console.WriteLine("Usage:");
                Console.WriteLine("-s=settings.xml [REQUIRED]");
                Console.WriteLine("-o=out/dir [OPTIONAL, Default = generated]");
                Console.WriteLine("-l=log/dir [OPTIONAL, Default = logs]");
                Console.WriteLine("-i=dir/to/parsedTypes.xml [OPTIONAL, multiple allowed]");
                return;
            }

            var settingsFile = args.First(x => x.StartsWith("-s=")).Substring(3);
            var outDir = args.FirstOrDefault(x => x.StartsWith("-o="))?.Substring(3) ?? "generated";
            var logDir = args.FirstOrDefault(x => x.StartsWith("-l="))?.Substring(3) ?? "logs";
            var ignoredTypes = args.Where(x => x.StartsWith("-i=")).Select(x => x.Substring(3)).ToArray();

            Run(generator, settingsFile, outDir, logDir, ignoredTypes);
        }
    }
}
