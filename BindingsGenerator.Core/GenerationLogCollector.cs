using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BindingsGenerator.Core
{
    public static class GenerationLogCollectorExtensions
    {
        public static void LogInfo(this IGenerationLogCollector logger, string message)
        {
            logger.AppendLogEntry(new GenerationLogEntry()
            {
                Level = GenerationLogLevel.Info,
                Message = message,
            });
        }
        public static void LogWarning(this IGenerationLogCollector logger, string code, string message, [CallerMemberName] string file = "", [CallerLineNumber] int line = 0)
        {
            logger.AppendLogEntry(new GenerationLogEntry()
            {
                Level = GenerationLogLevel.Warning,
                Code = code,
                Message = message,
                File = file,
                Line = line
            });
        }
        public static void LogError(this IGenerationLogCollector logger, string code, string message, [CallerMemberName] string file = "", [CallerLineNumber] int line = 0)
        {
            logger.AppendLogEntry(new GenerationLogEntry()
            {
                Level = GenerationLogLevel.Error,
                Code = code,
                Message = message,
                File = file,
                Line = line
            });
        }
    }
    public class ConsoleGenerationLogCollector : IGenerationLogCollector
    {
        private readonly List<GenerationLogEntry> generationLogEntries = new List<GenerationLogEntry>();
        public IEnumerable<GenerationLogEntry> GenerationLog => generationLogEntries;

        public void AppendLogEntry(GenerationLogEntry entry)
        {
            switch (entry.Level)
            {
                case GenerationLogLevel.Warning:
                case GenerationLogLevel.Error:
                    {
                        Console.WriteLine($"[{entry.Level}] {entry.Message} (Code: {entry.Code}, Location: {entry.File}@{entry.Line})");
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"[{entry.Level}] {entry.Message}");
                        break;
                    }
            }

            lock (generationLogEntries)
            {
                generationLogEntries.Add(entry);
            }
        }
    }
}
