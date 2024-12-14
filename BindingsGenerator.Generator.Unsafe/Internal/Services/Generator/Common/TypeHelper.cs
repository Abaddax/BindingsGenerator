using AutoGenBindings.Generator.Unsafe.Internal.Models.Generator;
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

        public string GetFullTypeName(IDefinition definition, bool useMapping = true, Usage usage = Usage.Unknown)
        {
            //Scopes
            var scopes = GetTypeScope(definition, usage);
            var type = scopes.Last();
            scopes.Remove(type);

            //Mapping and name
            var mapping = _generator.GenerateMapping(definition, usage);
            var name = type?.ScopeName;
            if (definition is PointerDefinition pointerDefinition)
            {
                if (pointerDefinition.GetPointedType() is TypeDefinition typeDef && typeDef.GetNestedType().Name == "void")
                {
                    name = "System.IntPtr" + new string('*', pointerDefinition.GetPointerDepth() - 1);
                }
                else
                {
                    name = name + new string('*', pointerDefinition.GetPointerDepth());
                }
            }
            if (useMapping && usage == Usage.Unknown)
                name = mapping?.Typename;
            else if (useMapping && mapping != null && mapping.Usage.HasFlag(usage))
                name = mapping.Typename;
            name ??= definition.Name;

            //Build
            var fullName = scopes.Select(s => s.ScopeName)
                .Append(name);
            return string.Join(".", fullName.ToArray());
        }
        public string GetFullTypeName(ITypeToken token, bool useMapping = true, Usage usage = Usage.Unknown)
        {
            return GetFullTypeName(token.Definition!, useMapping: useMapping, usage: usage);
        }

        /// <summary>
        /// Last is always the definition itself
        /// </summary>
        public List<NameScope> GetTypeScope(IDefinition definition, Usage usage = Usage.Unknown)
        {
            List<NameScope> scopes = new List<NameScope>();
            if (definition is PointerDefinition pointer)
                definition = pointer.GetPointedType(); //Special case

            var scope = _generator.GenerateScope(definition, usage);

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

        public string? GetTypeMarshalAs(IDefinition definition, Usage usage = Usage.Unknown)
        {
            var mapping = _generator.GenerateMapping(definition, usage);
            if (mapping == null)
                return null;
            if (mapping.MarshalAs.Length == 0)
                return null;
            var marshalAs = mapping.MarshalAs.FirstOrDefault(x => x.Usage.HasFlag(usage));
            if (marshalAs == null)
                return null;
            return marshalAs.Attribute;
        }
        public string? GetTypeMarshalAs(ITypeToken token, Usage usage = Usage.Unknown)
        {
            return GetTypeMarshalAs(token.Definition!, usage: usage);
        }
    }
}
