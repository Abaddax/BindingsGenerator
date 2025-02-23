using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class FunctionFacadeGenerator : GeneratorBase<FunctionDefinition>
    {
        readonly ParameterHelper _paramHelper;
        readonly TypeHelper _typeHelper;

        protected override string FileName => "Facade.g.cs";
        protected override bool UseNestedTypeGeneration => true;

        public FunctionFacadeGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _paramHelper = serviceProvider.GetRequiredService<ParameterHelper>();
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }


        public override IEnumerable<string> Usings()
        {
            yield return "System";
            yield return "System.Runtime.InteropServices";
            yield return $"static {Context.Options.RootNamespace}.vectors";
        }

        protected override void GenerateDefinition(FunctionDefinition function)
        {
            //Generate delegate
            WriteSummary(function);
            foreach (var param in function.Parameters)
            {
                WriteParam(param, param.Name);
            }
            WriteReturnComment(function);
            WriteGeneratedCodeAttribute();
            WriteObsoletion(function);
            WriteSupportedOsPlatformAttribute();

            var returnType = _paramHelper.GetReturnType(function, out _);

            var parameters = _paramHelper.GetParameters(function.Parameters, withAttributes: false);
            var parameterNames = _paramHelper.GetParamterNames(function.Parameters);

            //Static function
            if (function.Caller == null)
            {
                var functionName = _typeHelper.GetFullTypeName(function, useMapping: false).ToFullName();
                WriteLine($"public static {returnType} {functionName}({parameters}) => vectors.{functionName}.Invoke({parameterNames});");
            }
            //Class function
            else
            {
                var callerPtrType = function.Caller.FinalDefinition;
                var callerType = callerPtrType?.Type.FinalDefinition as ObjectDefinition;
                if (callerType == null)
                    return;
                var memberFunction = callerType.Functions.FirstOrDefault(f => f.FunctionType.FinalDefinition == function);
                if (memberFunction == null)
                    return;

                WriteLine($"public static {returnType} {memberFunction.Name}(this {parameters}) => @this.{memberFunction.GetFunctionName()}_Func.Invoke({parameterNames});");
            }
            WriteLine();
        }
    }
}
