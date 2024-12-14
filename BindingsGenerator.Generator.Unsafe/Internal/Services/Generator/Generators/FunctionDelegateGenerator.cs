﻿using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Generator.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using CppCallingConvention = global::CppSharp.AST.CallingConvention;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class FunctionDelegateGenerator : GeneratorBase<FunctionDefinitionBase>
    {
        readonly ParameterHelper _paramHelper;

        protected override string FileName => "Delegates.g.cs";

        public FunctionDelegateGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _paramHelper = serviceProvider.GetRequiredService<ParameterHelper>();
        }

        public override IEnumerable<string> Usings()
        {
            yield return "System";
            yield return "System.Security";
            yield return "System.Runtime.InteropServices";
            if (Context.Options.GenerateFramework)
                yield return $"{Context.Options.RootNamespace}.Framework";
            else
                yield return "BindingsGenerator.Unsafe.Framework";
        }

        protected override TypeMapping? GenerateTypeMapping(IDefinition definition, Usage usage)
        {
            //Function
            if (definition is FunctionDefinition function1)
            {
                return new TypeMapping()
                {
                    Typename = $"{GenerateNestedName(function1, out _, out _)}_delegate",
                    Usage = usage,
                };
            }
            //Delegate
            if (definition is DelegateDefinition function2)
            {
                return new TypeMapping()
                {
                    Typename = $"{GenerateNestedName(function2, out _, out _)}_delegate",
                    Usage = usage,
                };
            }
            //Function pointer
            if (definition is PointerDefinition pointer1 &&
               pointer1.Type.Definition is FunctionDefinition function3)
            {
                return new TypeMapping()
                {
                    Typename = $"{GenerateNestedName(function3, out _, out _)}_delegate*",
                    Usage = usage,
                    MarshalAs =
                    [
                        new TypeAttribute()
                        {
                            Usage = Usage.Parameter | Usage.ReturnValue | Usage.Field,
                            Attribute = "MarshalAs(UnmanagedType.FunctionPtr)"
                        }
                    ]
                };
            }
            //Delegate pointer
            if (definition is PointerDefinition pointer2 &&
               pointer2.Type.Definition is DelegateDefinition function4)
            {
                return new TypeMapping()
                {
                    Typename = $"{GenerateNestedName(function4, out _, out _)}_delegate*",
                    Usage = usage,
                    MarshalAs =
                    [
                        new TypeAttribute()
                        {
                            Usage = Usage.Parameter | Usage.ReturnValue | Usage.Field,
                            Attribute = "MarshalAs(UnmanagedType.FunctionPtr)"
                        }
                    ]
                };
            }
            return null;
        }
        protected override NameScope? GenerateTypeScope(FunctionDefinitionBase function, Usage usage)
        {
            return new NameScope()
            {
                ScopeName = GenerateNestedName(function, out _, out var parent),
                IsNamespace = false,
                ScopePrefix = null, //Functions do not support nesting
                ParentScope = TryGetScope(parent, usage)
            };
        }

        protected override void GenerateDefinition(FunctionDefinitionBase function)
        {
            var functionName = GenerateNestedName(function, out var scope, out _);
            using (BeginNamespace(scope))
            {
                //Generate delegate
                WriteGeneratedCodeAttribute();
                if (Context.Options.SuppressUnmanagedCodeSecurity)
                    WriteLine("[SuppressUnmanagedCodeSecurity]");
                WriteLine($"[UnmanagedFunctionPointer(CallingConvention.{Convert(function.CallingConvention)})]");

                var returnType = _paramHelper.GetReturnType(function, out var returnAttribute);
                if (!string.IsNullOrEmpty(returnAttribute))
                    WriteLine(returnAttribute);
                var parameters = _paramHelper.GetParameters(function.Parameters, withAttributes: true);

                WriteLine($"public unsafe delegate {returnType} {functionName}_delegate({parameters});");
            }
        }

        private string GenerateNestedName(FunctionDefinitionBase definition, out IDefinition scope, out IDefinition? parent)
        {
            List<string> names = new List<string>();
            scope = definition;
            parent = definition.Namespace?.Definition;
            while (parent is FunctionDefinitionBase delegateParent)
            {
                names.Add(delegateParent.Name);
                scope = parent;
                parent = delegateParent.Namespace?.Definition;
            }
            names.Add(definition.Name);
            if (definition.Overload > 0)
                names.Add($"overload{definition.Overload}");
            return string.Join("_", names.ToArray());
        }
        private static CallingConvention Convert(CppCallingConvention callingConvention)
        {
            switch (callingConvention)
            {
                case CppCallingConvention.Default:
                    return CallingConvention.Winapi;
                case CppCallingConvention.C:
                    return CallingConvention.Cdecl;
                case CppCallingConvention.StdCall:
                    return CallingConvention.StdCall;
                case CppCallingConvention.ThisCall:
                    return CallingConvention.ThisCall;
                case CppCallingConvention.FastCall:
                    return CallingConvention.FastCall;
                default:
                    throw new NotSupportedException($"Unknown calling convention: {callingConvention}");
            }
        }
    }
}
