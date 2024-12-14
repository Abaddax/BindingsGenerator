﻿using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Core.Contracts;
using BindingsGenerator.Core.Models;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Generator.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using CMacroParser.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.CodeDom.Compiler;
using System.Security;

namespace BindingsGenerator.Generator.Unsafe.Internal.Models.Generator
{
    internal abstract class GeneratorBase<TDefinition> : GeneratorBase where TDefinition : IDefinition
    {
        protected GeneratorBase(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        protected override void GenerateDefinitions(IEnumerable<IDefinition> definitions)
        {
            var _definitions = definitions.OfType<TDefinition>();
            GenerateDefinitions(_definitions);
        }
        protected virtual void GenerateDefinitions(IEnumerable<TDefinition> definitions)
        {
            base.GenerateDefinitions((IEnumerable<IDefinition>)definitions);
        }

        protected override void GenerateDefinition(IDefinition definition)
        {
            if (definition is not TDefinition _definition)
                return;
            GenerateDefinition(_definition);
        }
        protected virtual void GenerateDefinition(TDefinition definition)
        {
            return;
        }


        protected override TypeMapping? GenerateTypeMapping(IDefinition definition, Usage usage)
        {
            if (definition is not TDefinition _definition)
                return null;
            return GenerateTypeMapping(_definition, usage);
        }
        protected virtual TypeMapping? GenerateTypeMapping(TDefinition definition, Usage usage)
        {
            return null;
        }

        protected override NameScope? GenerateTypeScope(IDefinition definition, Usage usage)
        {
            if (definition is not TDefinition _definition)
                return null;
            return GenerateTypeScope(_definition, usage);
        }
        protected virtual NameScope? GenerateTypeScope(TDefinition definition, Usage usage)
        {
            return null;
        }
    }
    internal abstract class GeneratorBase
    {
        readonly CodeGenerator _baseGenerator;
        readonly IGenerationLogCollector _logger;
        readonly IGeneratedFileCollector _fileCollector;
        readonly GeneratorContext _context;
        readonly TypeHelper _typeHelper;

        readonly MemoryStream _memoryStream;
        readonly IndentedTextWriter _indentedTextWriter;
        readonly StreamWriter _streamWriter;

        protected IGenerationLogCollector? Logger => _logger;
        protected GeneratorContext Context => _context;

        protected virtual string FileName { get => "Generated.g.cs"; }
        protected virtual string? CustomNamespace { get => null; }
        protected virtual string? CustomTypeName { get => null; }
        protected virtual bool UseNestedTypeGeneration { get => false; }
        protected virtual bool UseEmptyFile { get => false; }

        protected GeneratorBase(IServiceProvider serviceProvider)
        {
            _baseGenerator = serviceProvider.GetRequiredService<CodeGenerator>();
            _logger = serviceProvider.GetRequiredService<IGenerationLogCollector>();
            _fileCollector = serviceProvider.GetRequiredService<IGeneratedFileCollector>();
            _context = serviceProvider.GetRequiredService<GeneratorContext>();
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();

            _memoryStream = new MemoryStream();
            _streamWriter = new StreamWriter(_memoryStream);
            _indentedTextWriter = new IndentedTextWriter(_streamWriter);
        }

        public virtual IEnumerable<string> Usings() => Array.Empty<string>();

        public void Generate()
        {
            /*var definitions = Context.Definitions.Values
                .Where(x => !Context.Options.KnownTypes
                    .Any(y =>
                        string.IsNullOrEmpty(y.TypeId)
                        ? (x.Name == y.Name)
                        : (x.ID == y.TypeId)));*/
            var definitions = Context.Definitions.Values.Where(x => !Context.Options.KnownTypes.Contains(x.ID)).ToArray();

            //Dislaimer
            WriteLine($"""
                #region Disclaimer
                // This file is autogenerated by "{this.GetType().FullName}" ({this.GetType().Assembly.GetName().Version}).
                // DO NOT EDIT THIS FILE! All changes will be lost after re-generation.
                // ---------------------------------------------------------------------
                // THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND. 
                // SINCE THIS FILE WAS AUTOGENERATED, ERRORS MIGHT OCCUR.
                // USE THIS FILE AT YOUR OWN RISK!!!
                #endregion

                """);

            if (!UseEmptyFile)
            {
                //Usings
                WriteLine($"using {Context.Options.RootNamespace};");
                foreach (var @namespace in Usings())
                {
                    WriteLine($"using {@namespace};");
                }
                foreach (var customUsing in Context.Options.CustomUsings)
                {
                    WriteLine($"using {customUsing};");
                }
                WriteLine();

                //Generate
                WriteLine($"namespace {CustomNamespace ?? Context.Options.RootNamespace}");
                using (BeginBlock())
                {
                    //Generate
                    if (UseNestedTypeGeneration)
                    {
                        WriteLine($"public static unsafe partial class {CustomTypeName ?? Context.Options.StaticTypename}");

                        using (BeginBlock())
                        {
                            GenerateDefinitions(definitions);
                        }
                    }
                    else
                    {
                        GenerateDefinitions(definitions);
                    }
                }
            }
            else
            {
                GenerateDefinitions(definitions);
            }


            //Write
            _streamWriter?.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);
            using (var streamReader = new StreamReader(_memoryStream, leaveOpen: true))
            {
                GeneratedSourceFile generatedFile = new()
                {
                    Filename = FileName,
                    Content = streamReader?.ReadToEnd() ?? string.Empty
                };
                _fileCollector.AppendGeneratedFile(generatedFile);
            }

            //Renew
            _memoryStream.SetLength(0);
            _memoryStream.Seek(0, SeekOrigin.Begin);
        }

        public TypeMapping? GenerateMapping(IDefinition definition, Usage usage)
        {
            return GenerateTypeMapping(definition, usage);
        }
        public NameScope? GenerateScope(IDefinition definition, Usage usage)
        {
            return GenerateTypeScope(definition, usage);
        }

        protected virtual void GenerateDefinitions(IEnumerable<IDefinition> definitions)
        {
            foreach (var definition in definitions)
            {
                GenerateDefinition(definition);
            }
        }
        protected virtual void GenerateDefinition(IDefinition definition)
        {
            return;
        }

        protected virtual TypeMapping? GenerateTypeMapping(IDefinition definition, Usage usage)
        {
            return null;
        }
        protected virtual NameScope? GenerateTypeScope(IDefinition definition, Usage usage)
        {
            return null;
        }

        #region Helper
        protected TypeMapping? TryGetMapping(IDefinition? definition, Usage usage)
        {
            return _baseGenerator.GenerateMapping(definition, usage);
        }
        protected NameScope? TryGetScope(IDefinition? definition, Usage usage)
        {
            return _baseGenerator.GenerateScope(definition, usage);
        }
        #endregion

        #region Text Helper
        protected void Write(string value) => _indentedTextWriter.Write(value);
        protected void WriteLine() => _indentedTextWriter.WriteLine();
        protected void WriteLine(string line) => _indentedTextWriter.WriteLine(line);
        protected void WriteLineWithoutIntent(string line) => _indentedTextWriter.WriteLineNoTabs(line);
        protected IDisposable BeginBlock()
        {
            WriteLine("{");
            _indentedTextWriter.Indent++;
            return new Disposable(() =>
            {
                _indentedTextWriter.Indent--;
                WriteLine("}");
            });
        }
        private sealed class Disposable : IDisposable
        {
            private readonly Action _action;

            public Disposable(Action action) => _action = action;

            public void Dispose() => _action();
        }
        #endregion

        #region Declaration Helper
        protected void WriteParam(IDocumentable value, string name)
        {
            var content = value.Documentation?.Trim();

            if (!string.IsNullOrWhiteSpace(content))
                WriteLine($"/// <param name=\"{name}\">{EscapeXmlString(content)}</param>");
        }
        protected void WriteSummary(IDocumentable xmlDoc)
        {
            var content = xmlDoc.Documentation?.Trim();
            if (!string.IsNullOrWhiteSpace(content))
                WriteLine($"/// <summary>{EscapeXmlString(content)}</summary>");
        }
        protected void WriteSummary(IExpression expression)
        {
            var content = expression?.Serialize();
            if (!string.IsNullOrWhiteSpace(content))
                WriteLine($"/// <summary>{EscapeXmlString(content)}</summary>");
        }

        protected void WriteReturnComment(FunctionDefinitionBase function)
        {
            var content = function.ReturnComment?.Trim();
            if (!string.IsNullOrWhiteSpace(content))
                WriteLine($"/// <returns>{EscapeXmlString(content)}</returns>");
        }
        protected void WriteObsoletion(IObsoletable obsoleteable)
        {
            var obsoletion = obsoleteable.Obsoletion;
            if (obsoletion.IsObsolete)
                WriteLine(string.IsNullOrWhiteSpace(obsoletion.Message) ? "[System.Obsolete()]" : $"[System.Obsolete(\"{EscapeQuotes(obsoletion.Message)}\")]");
        }
        protected void WriteGeneratedCodeAttribute()
        {
            WriteLine($"[System.CodeDom.Compiler.GeneratedCode(\"{this.GetType().FullName}\", \"{this.GetType().Assembly.GetName().Version}\")]");
        }
        private static string EscapeXmlString(string content) => SecurityElement.Escape(content);
        private static string EscapeQuotes(string s) => s.Replace("\"", "\\\"");
        #endregion

        #region Namespace Helper
        public IDisposable BeginNamespace(IDefinition definition)
        {
            var scopes = _typeHelper.GetTypeScope(definition);
            var type = scopes.Last();
            scopes.Remove(type);

            int braces = 0;
            int index = 0;

            WriteLine($"#region {_typeHelper.GetFullTypeName(definition, useMapping: false)}");
            bool indented = false;

            //Namespace
            {
                while (index < scopes.Count && scopes[index].IsNamespace)
                {
                    index++;
                }
                if (index > 0)
                {
                    var namespaces = scopes.Select(s => s.ScopeName).Take(index);
                    Write($"namespace {string.Join(".", namespaces.ToArray())} {{ ");
                    braces++;
                    indented = true;
                }
            }

            //Nested types
            {
                while (index < scopes.Count)
                {
                    var scope = scopes[index];
                    Write($"{scope.ScopePrefix} {scope.ScopeName} {{ ");
                    braces++;
                    index++;
                    indented = true;
                }
            }

            WriteLine();

            if (indented)
                _indentedTextWriter.Indent++;
            return new Disposable(() =>
            {
                if (indented)
                    _indentedTextWriter.Indent--;
                for (int i = 0; i < braces; i++)
                {
                    Write("}");
                }
                WriteLine();
                WriteLine($"#endregion");
                WriteLine();
            });
        }
        #endregion
    }
}