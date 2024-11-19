using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using CppSharp.AST;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common
{
    internal sealed class NamespaceHelper
    {
        static readonly ScopingEntityDefinitionBase _rootNamespace = new NamespaceDefinition() { ID = "RootNamespace", Name = "", IsRoot = true, Namespace = null };

        readonly ASTProcessor _processor;
        readonly ITokenStore _tokenStore;

        public NamespaceHelper(
            ASTProcessor processor,
            ITokenStore tokenStore)
        {
            _processor = processor;
            _tokenStore = tokenStore;
        }

        public ITypeToken<ScopingEntityDefinitionBase> GetNamespace(Declaration declaration, Declaration? parent)
        {
            if (string.IsNullOrEmpty(declaration.Namespace.QualifiedLogicalName))
                goto RETURN_ROOT;

            var @namespace = declaration.Namespace;
            while (@namespace != null)
            {
                var definition = _processor.TryProcessDeclaration(@namespace, parent);
                if (definition == null)
                    goto RETURN_ROOT;
                if (_tokenStore.Store(definition) is ITypeToken<ScopingEntityDefinitionBase> scope)
                    return scope;
                @namespace = @namespace.Namespace;
            }

        RETURN_ROOT:
            return (ITypeToken<ScopingEntityDefinitionBase>)_tokenStore.Store(_rootNamespace);
        }
        public void AppendToParentNamespace(ScopedEntityDefinitionBase definition)
        {
            var @namespace = definition?.Namespace?.FinalDefinition;
            if (@namespace == null)
                throw new ArgumentException(nameof(definition));

            var nestedTypes = @namespace.NestedTypes ?? Array.Empty<ITypeToken<ScopedEntityDefinitionBase>>();
            if (nestedTypes.Any(t => t.Definition.ID == definition.ID))
                return; //Already added
            @namespace.NestedTypes = nestedTypes.Append((ITypeToken<ScopedEntityDefinitionBase>)_tokenStore.Store(definition)).ToArray();
        }
        public void CopyNestedTypes(ScopingEntityDefinitionBase source, ScopingEntityDefinitionBase destination)
        {
            destination.NestedTypes = source.NestedTypes;
        }

    }
}
