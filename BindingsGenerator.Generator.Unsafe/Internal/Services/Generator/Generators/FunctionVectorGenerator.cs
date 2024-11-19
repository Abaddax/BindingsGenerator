using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class FunctionVectorGenerator : GeneratorBase<FunctionDefinitionBase>
    {
        readonly TypeHelper _typeHelper;

        protected override string FileName => "Vectors.g.cs";
        protected override bool UseNestedTypeGeneration => true;
        protected override string? CustomTypeName => "vectors";

        public FunctionVectorGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }

        public override IEnumerable<string> Usings()
        {
            yield return "System";
            yield return "System.Diagnostics";
            yield return "System.Security";
            yield return "System.Runtime.InteropServices";
            yield return $"static {Context.Options.RootNamespace}.imports";
        }

        protected override void GenerateDefinitions(IEnumerable<FunctionDefinitionBase> functions)
        {
            base.GenerateDefinitions(functions);

            //Inject static constructor
            WriteLine("static vectors()");
            using (BeginBlock())
            {
                WriteLine("imports.Initialize();");
            }
        }
        protected override void GenerateDefinition(FunctionDefinitionBase function)
        {
            var delegateName = _typeHelper.GetFullTypeName(function);
            var functionName = _typeHelper.GetFullTypeName(function, useMapping: false).ToFullName();

            WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            WriteLine($"internal static {delegateName} _{functionName};");
            WriteLine($"public static {delegateName} {functionName} => _{functionName} ?? throw new MissingMethodException(\"{functionName}\");");
            WriteLine();
        }
    }
}
