using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using CMacroParser;
using CMacroParser.Contracts;
using System.Text.RegularExpressions;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class MacroGenerator : GeneratorBase<MacroDefinition>
    {
        private IEnumerable<IDefinition> _allDefinitions;
        private IEnumerable<IMacroDefinition> _macros;

        protected override string FileName => "Macros.g.cs";
        protected override bool UseNestedTypeGeneration => true;

        public MacroGenerator(IServiceProvider serviceProvider)
           : base(serviceProvider) { }

        public override IEnumerable<string> Usings()
        {
            yield return "System";
        }

        protected override void GenerateDefinitions(IEnumerable<IDefinition> definitions)
        {
            _allDefinitions = definitions.ToArray();

            base.GenerateDefinitions(definitions);
        }
        protected override void GenerateDefinitions(IEnumerable<MacroDefinition> macros)
        {
            //Preprocess macros
            var preprocessedMacros = PreprocessMacros(macros).ToArray();

            base.GenerateDefinitions(preprocessedMacros);
        }

        protected override void GenerateDefinition(MacroDefinition macro)
        {
            if (Context.Options.KnownMacros.Contains(macro.Name))
                return; //Already known, do not generate

            var customMacros = Context.Options.CustomMacros
                .Where(custom => Regex.IsMatch(macro.RawExpression, custom.Regex))
                .ToArray();

            //Default
            if (customMacros.Length == 0)
            {
                if (macro.Definition?.Expression == null)
                {
                    WriteGeneratedCodeAttribute();
                    WriteLine($"public static void* {macro.Name} => throw new MissingFieldException(\"Unable to autogenerate expression.\");");
                    WriteLine();
                }
                else
                {
                    var expression = macro.Definition.Expression.Expand(_macros);
                    var type = expression.DeduceType();
                    string typeName;
                    bool custom = false;
                    switch (type.Deduced)
                    {
                        //Not supported
                        case LiteralType.unknown:
                        case LiteralType.@void:
                            return;
                        case LiteralType.custom:
                            {
                                custom = true;
                                //Check if type is known
                                var found = _allDefinitions
                                    .Where(x => x.Name == type.Type)
                                    .Any(x => x.TryGetNestedType() is not IFinalDefinition);
                                if (!found)
                                    return;
                                typeName = type.ToString()!;
                            }
                            break;
                        default:
                            {
                                typeName = type.ToString()!;
                            }
                            break;
                    }

                    WriteSummary(macro.Definition.Expression);
                    WriteGeneratedCodeAttribute();
                    if (expression.IsConst(_macros))
                    {
                        //TODO
                        if (custom)
                            WriteLine($"public const {type} {macro.Name} = unchecked({expression});");
                        else
                            WriteLine($"public const {type} {macro.Name} = {expression};");
                    }
                    else
                    {
                        WriteLine($"public static {type} {macro.Name} = {expression};");
                    }
                    WriteLine();
                }
            }
            //Custom macro
            else if (customMacros.Length == 1)
            {
                var customMacro = customMacros.First();
                var custom = Regex.Replace(macro.RawExpression, customMacro.Regex, customMacro.Replace);

                WriteLine(custom);
                WriteLine();
            }
            else
            {
                throw new InvalidOperationException($"Multiple custom macros for {macro.Name}");
            }
        }

        private IEnumerable<MacroDefinition> PreprocessMacros(IEnumerable<MacroDefinition> macros)
        {
            //Check for custom macros
            var customMacros = macros
                .Where(x => Context.Options.CustomMacros.Any(custom => Regex.IsMatch(x.RawExpression, custom.Regex)))
                .ToArray();

            //Parse macros and check
            var knownMacros = Context.Options.KnownMacros
                .Select(x => MacroParser.ParseMacro(x));
            _macros = macros
                .Where(x => x.Definition != null)
                .Select(x => x.Definition!)
                .Concat(knownMacros)
                .ToArray();


            foreach (var customMacro in customMacros)
            {
                yield return customMacro;
            }
            foreach (var macro in macros.Except(customMacros))
            {
                if (macro.Definition?.Expression == null)
                    continue;
                if (macro.Definition.Expression.ContainsUnknown(_macros))
                    continue;
                var ret = new MacroDefinition()
                {
                    ID = macro.ID,
                    Name = macro.Name,
                    File = macro.File,
                    Definition = macro.Definition,
                    RawExpression = macro.RawExpression
                        .Replace("\r\n", " ")
                        .Replace("\n", " ")
                        .Replace("\r", " ")
                };
                yield return ret;
            }
        }

    }
}
