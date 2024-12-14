using BindingsGenerator.Core;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Type = CppSharp.AST.Type;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    internal class FunctionProcessor : ProcessorBase
    {
        readonly NamespaceHelper _namespaceHelper;

        public FunctionProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _namespaceHelper = serviceProvider.GetRequiredService<NamespaceHelper>();
        }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(Method);
            yield return typeof(Function);
            yield return typeof(FunctionType);
        }

        protected override IDefinition? ProcessDeclaration(Declaration declaration, Declaration? parent)
        {
            if (declaration is not Function function)
                return null;

            if (HasTemplatedParameters(function))
                return null;
            if (HasFunctionBody(function))
                return null;

            //Check if currently processing
            var processingDefinition = TryGetProcessingDefinition(function.USR);
            if (processingDefinition != null)
                return processingDefinition;

            //Check if already existing
            var definition = TryGetDefinition<FunctionDefinition>(function.USR);
            if (definition != null)
                return definition; //Already processed

            if (function.OperatorKind != CXXOperatorKind.None)
            {
                Logger?.LogWarning("", $"Skipped operator of type {function.OperatorKind} for {function.Signature}", function.TranslationUnit.FileName, function.LineNumberStart);
                return null; //Not supported
            }
            using (var processing =
                BeginProcess(new FunctionDefinition()
                {
                    ID = function.USR,
                    Name = function.GetName(parent),
                    Namespace = _namespaceHelper.GetNamespace(function, parent),
                    Caller = null,
                    CallingConvention = CallingConvention.Default,
                    LibraryName = null,
                    FunctionSignature = null
                }))
            {
                //Methods are always thiscall?
                bool isMemberFunction = false;
                if (declaration is Method method && !method.IsStatic)
                    isMemberFunction = true;

                //Create definition
                definition = new FunctionDefinition()
                {
                    ID = processing.Definition.ID,
                    Name = processing.Definition.Name,
                    Namespace = processing.Definition.Namespace,
                    Caller = TryGetCaller(function),
                    LibraryName = TryGetFunctionExport(function.Mangled)?.LibraryName ?? string.Empty,
                    FunctionSignature = function.Mangled,
                    CallingConvention = isMemberFunction ? CallingConvention.ThisCall : function.CallingConvention,
                    ReturnType = GetReturnTypeToken(function.ReturnType.Type, function),
                    ReturnComment = function.GetReturnDocumentation(),
                    Parameters = GetParameters(function.Parameters, function).ToArray(),
                    Documentation = function.GetDocumentation(),
                    Obsoletion = function.GetObsoletion(),
                    File = function.TranslationUnit.FileName,
                };
                definition.Overload = GetOverloadCount(definition);

                _namespaceHelper.CopyNestedTypes(processing.Definition, definition);
                _namespaceHelper.AppendToParentNamespace(definition);
                return definition;
            }
        }
        protected override IDefinition? ProcessType(Type type, Declaration parent)
        {
            if (type is not FunctionType functionType)
                return null;

            var definition = new DelegateDefinition
            {
                ID = $"{parent.USR}@F@func",
                Name = $"{parent.Name}_func",
                Namespace = _namespaceHelper.GetNamespace(parent, parent),
                ReturnType = GetReturnTypeToken(functionType.ReturnType.Type, parent),
                Parameters = GetParameters(functionType.Parameters, null).ToArray(),
                CallingConvention = functionType.CallingConvention,
                File = parent.TranslationUnit.FileName,
            };

            definition.Overload = GetOverloadCount(definition);

            _namespaceHelper.AppendToParentNamespace(definition);
            return definition;
        }


        private IEnumerable<FunctionParameter> GetParameters(List<Parameter> parameters, Function function = null)
        {
            var @params = new List<FunctionParameter>();

            var caller = TryGetCaller(function);

            //Append instance for instance methods
            if (caller != null)
            {
                var param = new FunctionParameter
                {
                    Name = "this",
                    Type = caller,
                    Documentation = "Caller instance",
                };
                @params.Add(param);
            }

            for (int position = 0; position < parameters.Count; position++)
            {
                var parameter = parameters[position];
                var name = string.IsNullOrEmpty(parameter.Name) ? $"p{position}" : parameter.Name;
                var parameterToken = GetToken(parameter.Type, parameter);
                var param = new FunctionParameter
                {
                    Name = name,
                    Type = parameterToken,
                    Documentation = function.GetParamDocumentation(parameter.Name),
                    IsConstant = parameter.IsConst || parameter.Type is ArrayType { SizeType: ArrayType.ArraySize.Constant },
                    //IsIndirect = parameter.IsIndirect,
                    IsReference = parameter.Type.IsReference()
                };
                @params.Add(param);
            }

            return @params;
        }
        private ITypeToken GetReturnTypeToken(Type type, Declaration parent)
        {
            var typeToken = GetToken(type, parent);

            return typeToken;
        }
        private ITypeToken<PointerDefinition>? TryGetCaller(Function function)
        {
            var method = function as Method;
            bool isInstanceMethod = !method?.IsStatic ?? false;

            if (!isInstanceMethod)
                return null;

            var caller = GetToken(method.Namespace, null);
            var callerPtr = new PointerDefinition()
            {
                ID = PointerProcessor.GetPointerID(caller),
                Name = null,
                Type = caller
            };
            return TryStoreDefinition(callerPtr);
        }
        private bool HasTemplatedParameters(Function function)
        {
            foreach (var parameter in function.Parameters.Select(p => p.Type).Append(function.ReturnType.Type))
            {
                var type = parameter.GetFinalPointee() ?? parameter;
                if (type.GetType().Name.ToLowerInvariant().Contains("template"))
                    return true;
            }
            return false;
        }
        private bool HasFunctionBody(Function function)
        {
            if (!string.IsNullOrEmpty(function.Body))
                return true;
            return false;
        }
        private int GetOverloadCount(FunctionDefinitionBase function)
        {
            var @namespace = function?.Namespace?.FinalDefinition;
            if (@namespace == null)
                throw new ArgumentException(nameof(function));

            var nestedTypes = @namespace.NestedTypes ?? Array.Empty<ITypeToken<ScopedEntityDefinitionBase>>();
            return nestedTypes.Count(t =>
                t.Definition.Name == function.Name &&
                t.Definition is FunctionDefinitionBase);
        }
    }
}
