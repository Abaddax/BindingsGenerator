using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common
{
    internal class ParameterHelper
    {
        readonly TypeHelper _typeHelper;

        public ParameterHelper(TypeHelper typeHelper)
        {
            _typeHelper = typeHelper;
        }

        /// <summary>
        /// Uncovers parameters type for arguments
        /// </summary>
        /// <param name="qualifier">in, out, ref</param>
        public ITypeToken GetParamterType(FunctionParameter parameter, out string qualifier)
        {
            var parameterType = parameter.Type.Definition;

            //By value or const value
            if (parameterType is not PointerDefinition pointerDefinition)
            {
                if (parameter.IsConstant)
                    qualifier = "in";
                else
                    qualifier = string.Empty;
                return parameter.Type;
            }
            var pointerDepth = pointerDefinition.GetPointerDepth();

            //By reference
            if (pointerDepth >= 1 && parameter.IsReference)
            {
                qualifier = "ref";
                return pointerDefinition.Type;
            }
            //Out param -> wont work all the time
            if (pointerDepth == 2 && !parameter.IsReference)
            {
                qualifier = "ref";
                //qualifier = "out";
                return pointerDefinition.Type;
            }
            //Default
            qualifier = string.Empty;
            return parameter.Type;
        }
        /// <summary>
        /// Build the function parameter list
        /// </summary>
        /// <returns>a, in b, ref c, out d</returns>
        public string GetParamterNames(IEnumerable<FunctionParameter> parameters)
        {
            List<string> names = new();
            foreach (var parameter in parameters)
            {
                _ = GetParamterType(parameter, out var qualifier);
                var name = $"@{parameter.Name}";
                if (!string.IsNullOrEmpty(qualifier))
                    name = $"{qualifier} {name}";
                names.Add(name);
            }
            return string.Join(", ", names);
        }
        /// <summary>
        /// Build function parameter list
        /// </summary>
        /// <returns>int a, out int b, [MarshalAs(UnmanagedType.LPWStr)] ref string c</returns>
        public string GetParameters(IEnumerable<FunctionParameter> parameters, bool withAttributes = true, bool useMapping = true, Usage customUsage = Usage.Unknown)
        {
            List<string> names = new();
            var usage = Usage.Parameter | customUsage;
            foreach (var parameter in parameters)
            {
                var type = GetParamterType(parameter, out var qualifier);
                var typeName = _typeHelper.GetFullTypeName(type, useMapping: useMapping, usage: usage);
                var marshalAs = _typeHelper.GetTypeMarshalAs(type, usage: usage);

                var name = $"{typeName} @{parameter.Name}";
                if (!string.IsNullOrEmpty(qualifier))
                    name = $"{qualifier} {name}";
                if (withAttributes && !string.IsNullOrEmpty(marshalAs))
                    name = $"[{marshalAs}] {name}";
                names.Add(name);
            }
            return string.Join(", ", names);
        }

        public string GetReturnType(FunctionDefinitionBase function, out string? returnAttribute, bool useMapping = true, Usage customUsage = Usage.Unknown)
        {
            var usage = Usage.ReturnValue | customUsage;
            var typeName = _typeHelper.GetFullTypeName(function.ReturnType, useMapping: useMapping, usage: usage);
            var marshalAs = _typeHelper.GetTypeMarshalAs(function.ReturnType, usage: usage);
            if (string.IsNullOrEmpty(marshalAs))
                returnAttribute = null;
            else
                returnAttribute = $"[return: {marshalAs}]";
            return typeName;
        }
    }
}
