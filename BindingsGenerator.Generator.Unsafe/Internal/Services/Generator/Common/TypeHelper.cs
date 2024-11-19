using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common
{
    internal class TypeHelper
    {
        readonly CodeGenerator _generator;

        public TypeHelper(CodeGenerator generator)
        {
            _generator = generator;
        }

        public string GetFullTypeName(IDefinition definition, bool useMapping = true)
        {
            //Scopes
            var scopes = GetTypeScope(definition);
            var type = scopes.Last();
            scopes.Remove(type);

            //Mapping and name
            var mapping = _generator.GenerateMapping(definition);
            var name = (useMapping ? mapping?.Typename : type?.ScopeName) ?? definition.Name;

            //Build
            var fullName = scopes.Select(s => s.ScopeName)
                .Append(name);
            return string.Join(".", fullName.ToArray());
        }
        public string GetFullTypeName(ITypeToken token, bool useMapping = true)
        {
            return GetFullTypeName(token.Definition!, useMapping);
        }

        /// <summary>
        /// Last is always the definition itself
        /// </summary>
        public List<NameScope> GetTypeScope(IDefinition definition)
        {
            List<NameScope> scopes = new List<NameScope>();
            if (definition is PointerDefinition pointer)
                definition = pointer.Type.Definition; //Special case

            var scope = _generator.GenerateScope(definition);

            scope ??= new NameScope()
            {
                IsNamespace = false,
                ScopeName = definition.Name,
                ScopePrefix = null,
                ParentScope = null
            };

            //First is always the type itself
            scopes.Insert(0, scope);
            scope = scope.ParentScope;

            //Unpack parents
            while (scope != null)
            {
                scopes.Insert(0, scope);
                scope = scope.ParentScope;
            }

            return scopes;
        }

        public string? GetTypeMarshalAs(IDefinition definition, AttributeUsage usage = AttributeUsage.None)
        {
            var mapping = _generator.GenerateMapping(definition);
            if (mapping == null)
                return null;
            if (mapping.MarshalAs == null)
                return null;
            if (!mapping.MarshalAs.Usage.HasFlag(usage))
                return null;
            return mapping.MarshalAs.Attribute;
        }
        public string? GetTypeMarshalAs(ITypeToken token, AttributeUsage usage = AttributeUsage.None)
        {
            return GetTypeMarshalAs(token.Definition!, usage);
        }
    }
}
