using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class LibraryGenerator : GeneratorBase
    {
        protected override string FileName => "Library.g.cs";
        protected override bool UseNestedTypeGeneration => true;

        public LibraryGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public override IEnumerable<string> Usings()
        {
            yield return "System.Collections.Generic";
            yield return "System.Collections.Immutable";
        }

        protected override void GenerateDefinitions(IEnumerable<IDefinition> definitions)
        {
            WriteGeneratedCodeAttribute();
            WriteLine("public static IReadOnlyDictionary<string, string> LibraryVersionMap = new Dictionary<string, string>");

            using (BeginBlock())
            {
                foreach (var pair in Context.ExportMap.Values
                    .Select(x => new { x.LibraryName, x.LibraryVersion }).Distinct()
                    .ToDictionary(x => x.LibraryName, x => x.LibraryVersion ?? "Unknown")
                    .OrderBy(x => x.Key))
                {
                    WriteLine($"{{\"{pair.Key}\", \"{pair.Value}\"}},");
                }
            }
            WriteLine(".ToImmutableDictionary();");
        }
    }
}
