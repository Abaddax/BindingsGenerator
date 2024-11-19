using BindingsGenerator.Core.Models.OptionElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BindingsGenerator.Core.Helper
{
    public static class IncludeHelper
    {
        public static string CreateHeader(IEnumerable<IncludeFile> includes, IEnumerable<string> includeDirs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var include in includes)
            {
                if (!include.ForceGeneration)
                {
                    sb.AppendLine($"#include <{include.FileName}>");
                    continue;
                }
                var filePath = FindFilePath(include.FileName, includeDirs);
                if (filePath == null)
                    throw new FileNotFoundException($"{include.FileName} not found in {string.Join("; ", includeDirs)}");
                sb.AppendLine(File.ReadAllText(filePath));
            }
            return sb.ToString();
        }
        public static string CreateHeaderFile(IEnumerable<IncludeFile> includes, IEnumerable<string> includeDirs)
        {
            var content = CreateHeader(includes, includeDirs);

            var fileName = includes.FirstOrDefault(x => x.ForceGeneration == true)?.FileName;
            if (fileName != null)
                fileName = Path.GetFileNameWithoutExtension(fileName);
            fileName = Path.GetTempPath() + fileName + ".h";
            if (File.Exists(fileName))
                File.Delete(fileName);
            using (var headerFile = File.Create(fileName))
            {
                using (var writer = new StreamWriter(headerFile))
                {
                    writer.Write(content);
                }
            }
            return fileName;
        }
        public static string PreprocessIncludes(string headerFile, HashSet<string> includeDirs)
        {
            includeDirs = new HashSet<string>(includeDirs);

            var filePath = FindFilePath(headerFile, includeDirs);
            if (filePath == null)
                return $"#include \"{headerFile}\"";
            //throw new FileNotFoundException($"{headerFile} not found in {string.Join("; ", includeDirs)}");

            var folder = Path.GetDirectoryName(filePath);
            includeDirs.Add(folder);

            List<string> lines = new List<string>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                var lineToAdd = line;

                try
                {
                    var match1 = Regex.Match(line, "#include <([^\"<>]*)>");
                    if (match1.Success)
                    {
                        var includeFileName = match1.Groups[1].Value;
                        var includeFileContent = PreprocessIncludes(includeFileName, includeDirs);

                        lineToAdd = Regex.Replace(line, "#include <([^\"<>]*)>", includeFileContent);
                        continue;
                    }
                    var match2 = Regex.Match(line, "#include \"([^\"<>]*)\"");
                    if (match2.Success)
                    {
                        var includeFileName = match2.Groups[1].Value;
                        var includeFileContent = PreprocessIncludes(includeFileName, includeDirs);

                        lineToAdd = Regex.Replace(line, "#include \"([^\"<>]*)\"", includeFileContent);
                        continue;
                    }
                    continue;
                }
                finally
                {
                    lines.Add(lineToAdd);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string? FindFilePath(string file, IEnumerable<string> includeDirs)
        {
            foreach (var includeDir in includeDirs)
            {
                var path = Path.Combine(includeDir, file);
                if (File.Exists(path))
                    return Path.GetFullPath(path);
            }
            return null;
        }

    }
}
