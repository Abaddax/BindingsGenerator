﻿using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using CppSharp.AST;

namespace BindingsGenerator.Generator.Unsafe.Internal.Models.Generator
{
    internal static class TypeExtensions
    {
        public static string ToFullName(this string name)
        {
            name ??= string.Empty;
            return name
                .Replace("::", "_")
                .Replace(".", "_");
        }

        public static string Get(this AccessSpecifier accessSpecifier)
        {
            return accessSpecifier switch
            {
                AccessSpecifier.Public => "public",
                AccessSpecifier.Internal => "internal",
                AccessSpecifier.Private => "private",
                AccessSpecifier.Protected => "protected internal",
                _ => "public"
            };
        }

        public static bool IsPOD(this ObjectDefinition @class)
        {
            if (@class.IsInherited)
                return false;
            if (@class.Functions.Length != 0)
                return false;
            if (@class.Bases.Length != 0)
                return false;
            return true;
        }
        public static int GetPointerDepth(this PointerDefinition pointer)
        {
            var typeDefintion = pointer.Type.Definition;
            if (typeDefintion is not PointerDefinition pointerDefinition)
                return 1;
            return pointerDefinition.GetPointerDepth() + 1;
        }

        public static string GetFunctionName(this FunctionDefinitionBase function)
        {
            var overload = function.Overload;
            if (overload == 0)
                return function.Name;
            else
                return $"{function.Name}_overload{overload}";
        }
        public static string GetFunctionName(this MemberFunction function)
        {
            var definition = function.Overridden?.FinalDefinition ?? function.FunctionType.FinalDefinition;
            return definition.GetFunctionName();
        }
        public static string GetFunctionName(this VTableFunction function)
        {
            var definition = function.FunctionType.FinalDefinition.Type.Definition as FunctionDefinition;
            return definition.GetFunctionName();
        }
    }
}