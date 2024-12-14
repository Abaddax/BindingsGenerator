using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using CMacroParser;
using CMacroParser.Contracts;
using CppSharp.AST;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using IDefinition = BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts.IDefinition;
using MacroDefinition = BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions.MacroDefinition;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    internal class MacroProcessor : ProcessorBase
    {
        public MacroProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(CppSharp.AST.MacroDefinition);
            yield return typeof(CppSharp.AST.MacroExpansion);
        }

        protected override IDefinition? ProcessPreprocessedEntity(PreprocessedEntity preprocessedEntity, Declaration parent)
        {
            /*if (preprocessedEntity.MacroLocation == MacroLocation.ClassHead)
                Debugger.Break();*/
            return preprocessedEntity switch
            {
                CppSharp.AST.MacroDefinition macroDefinition => ProcessPreprocessedEntity(macroDefinition, parent),
                CppSharp.AST.MacroExpansion macroExpansion => ProcessPreprocessedEntity(macroExpansion, parent),
                _ => null,
            };
        }


        private IDefinition ProcessPreprocessedEntity(CppSharp.AST.MacroDefinition macro, Declaration parent)
        {
            if (macro.MacroLocation == MacroLocation.ClassHead)
                Debugger.Break();
            string contentHash;
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(macro.Expression));
                contentHash = Convert.ToBase64String(hash);
            }
            var id = $"M@{parent.TranslationUnit.Name}@{macro.Name}@{contentHash}";

            //Check if already existing
            var definition = TryGetDefinition<MacroDefinition>(id);
            if (definition != null)
                return definition; //Already processed

            if (string.IsNullOrEmpty(macro.Name))
            {
                return new MacroDefinition()
                {
                    Name = string.Empty,
                    File = parent.TranslationUnit.FileName,
                    ID = id,
                    Definition = null,
                    RawExpression = macro.Expression
                };
            }

            IMacroDefinition? macroDefinition;
            try
            {
                macroDefinition = macro.Expression switch
                {
                    //CppSharp does not respect function macros
                    string expression when Regex.IsMatch(expression, "^\\([^\\(\\)]*\\) ") => MacroParser.ParseMacro($"{macro.Name}{macro.Expression}"),
                    _ => MacroParser.ParseMacro($"{macro.Name} {macro.Expression}")
                };
            }
            catch (Exception ex)
            {
                macroDefinition = null;
            }

            return new MacroDefinition()
            {
                Name = macro.Name,
                File = parent.TranslationUnit.FileName,
                ID = id,
                Definition = macroDefinition,
                RawExpression = macro.Expression
            };
        }
        private IDefinition ProcessPreprocessedEntity(CppSharp.AST.MacroExpansion macro, Declaration parent)
        {
            if (macro.Definition != null)
                return ProcessPreprocessedEntity(macro.Definition, parent);

            string contentHash;
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(macro.Text));
                contentHash = Convert.ToBase64String(hash);
            }
            var id = $"M@{parent.TranslationUnit.Name}@{macro.Name}@{contentHash}";

            //Check if already existing
            var definition = TryGetDefinition<MacroDefinition>(id);
            if (definition != null)
                return definition; //Already processed

            if (string.IsNullOrEmpty(macro.Name))
            {
                return new MacroDefinition()
                {
                    Name = string.Empty,
                    File = parent.TranslationUnit.FileName,
                    ID = id,
                    Definition = null,
                    RawExpression = macro.Text
                };
            }

            IMacroDefinition? macroDefinition;
            try
            {
                macroDefinition = MacroParser.ParseMacro($"{macro.Name} {macro.Text}");
            }
            catch (Exception ex1)
            {
                //Try again without gap 
                //CppSharp does not respect function macros
                try
                {
                    macroDefinition = MacroParser.ParseMacro($"{macro.Name}{macro.Text}");
                }
                catch (Exception ex2)
                {
                    macroDefinition = null;
                }
            }

            return new MacroDefinition()
            {
                Name = macro.Name,
                File = parent.TranslationUnit.FileName,
                ID = id,
                Definition = macroDefinition,
                RawExpression = macro.Text
            };
        }

    }
}
