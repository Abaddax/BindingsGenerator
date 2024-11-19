using BindingsGenerator.Core;
using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Core.Helper;
using BindingsGenerator.Core.Models;
using BindingsGenerator.Generator.Unsafe.Internal.Models;
using CppSharp;
using CppSharp.AST;
using CppSharp.Parser;
using ClangParser = CppSharp.ClangParser;
using ParserASTContext = CppSharp.Parser.AST.ASTContext;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services
{
    internal sealed class ASTParser
    {
        readonly string[] _includeDirs = Array.Empty<string>();
        readonly string[] _defines = Array.Empty<string>();

        readonly IGenerationLogCollector _logger;
        readonly GeneratorContext _context;

        bool _hasParsingErrors;

        public ASTParser(
            GeneratorContext context,
            IGenerationLogCollector logger)
        {
            _context = context;
            _logger = logger;

            _includeDirs = _context.Options.IncludeDirs.Select(x => x.DirectoryName).ToArray();
            _defines = _context.Options.Defines.Select(x => x.Value).ToArray();
        }

        public ASTContext Parse(params Core.Models.OptionElements.TranslationUnit[] translationUnits)
        {
            var sourceFiles = new List<string>();
            foreach (var unit in translationUnits)
            {
                var file = IncludeHelper.CreateHeaderFile(unit.Includes, _includeDirs);
                sourceFiles.Add(file);
            }

            try
            {
                _hasParsingErrors = false;
                var context = ParseInternal(sourceFiles);
                if (_hasParsingErrors)
                    throw new InvalidOperationException();
                return context;
            }
            finally
            {
                foreach (var file in sourceFiles)
                {
                    File.Delete(file);
                }
            }
        }

        private ASTContext ParseInternal(IEnumerable<string> sourceFiles)
        {
            var parserOptions = new ParserOptions
            {
                Verbose = true,
                ASTContext = new ParserASTContext(),
                LanguageVersion = Enum.Parse<LanguageVersion>(_context.Options.LanguageVersion.ToString()),
                SkipFunctionBodies = true,
                SkipPrivateDeclarations = true,
            };

            switch (_context.Options.Platform)
            {
                case GeneratorPlatform.WindowsMscv:
                    {
                        parserOptions.Setup(TargetPlatform.Windows);
                        parserOptions.SetupMSVC(VisualStudioVersion.Latest);
                        break;
                    }
                case GeneratorPlatform.Windows:
                    {
                        parserOptions.Setup(TargetPlatform.Windows);
                        break;
                    }
                case GeneratorPlatform.Linux:
                    {
                        parserOptions.Setup(TargetPlatform.Linux);
                        parserOptions.SetupLinux();
                        break;
                    }
            }

            foreach (var includeDir in _includeDirs)
            {
                parserOptions.AddIncludeDirs(Path.GetFullPath(includeDir));
            }

            foreach (var define in _defines)
            {
                parserOptions.AddDefines(define);
            }
            var result = ClangParser.ParseSourceFiles(sourceFiles, parserOptions);
            OnSourceFileParsed(sourceFiles, result);
            return ClangParser.ConvertASTContext(parserOptions.ASTContext);
        }
        private void OnSourceFileParsed(IEnumerable<string> files, ParserResult result)
        {
            if (result == null)
                return;
            switch (result.Kind)
            {
                case ParserResultKind.Success:
                    _logger.LogInfo($"Parsed '{string.Join(", ", files)}'");
                    break;
                case ParserResultKind.Error:
                    _logger.LogError("", $"Error parsing '{string.Join(", ", files)}'");
                    _hasParsingErrors = true;
                    break;
                case ParserResultKind.FileNotFound:
                    _logger.LogError("", $"A file from '{string.Join(",", files)}' was not found");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (uint i = 0; i < result.DiagnosticsCount; ++i)
            {
                var diagnostics = result.GetDiagnostics(i);

                var message =
                    $"{diagnostics.FileName}({diagnostics.LineNumber},{diagnostics.ColumnNumber}): {diagnostics.Level.ToString().ToLower()}: {diagnostics.Message}";
                _logger.LogInfo(message);
            }
        }
    }
}
