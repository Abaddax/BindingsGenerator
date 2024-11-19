using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common;
using CppSharp.AST;
using Microsoft.Extensions.DependencyInjection;
using Type = CppSharp.AST.Type;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    /// <summary>
    /// Resolves: <br/>
    /// Builtin types <br/>
    /// Typedefs <br/>
    /// Nested types <br/>
    /// </summary>
    internal class TypedefProcessor : ProcessorBase
    {
        readonly NamespaceHelper _namespaceHelper;

        public TypedefProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _namespaceHelper = serviceProvider.GetRequiredService<NamespaceHelper>();
        }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(TypedefDecl);
            yield return typeof(TypedefType);
        }
        protected override IDefinition? ProcessDeclaration(Declaration declaration, Declaration? parent)
        {
            if (declaration is not TypedefDecl typedefDecl)
                return null;

            var typeToken = GetToken(typedefDecl.Type, typedefDecl);

            //Do not typedef the same name
            if (typeToken.Definition?.Name == typedefDecl.Name)
                return typeToken.Definition;
            //Do not typedef pointers
            if (typeToken.Definition is PointerDefinition pointer)
                return pointer;

            var id = $"{typeToken.Token}@T@{typedefDecl.Name}";
            return new TypeDefinition()
            {
                ID = id,
                Name = typedefDecl.Name,
                File = typedefDecl.TranslationUnit.FileName,
                Type = typeToken,
                Namespace = _namespaceHelper.GetNamespace(declaration, parent),
                Documentation = declaration.GetDocumentation(),
                Obsoletion = declaration.GetObsoletion(),
            };
        }
        protected override IDefinition? ProcessType(Type type, Declaration parent)
        {
            if (type is not TypedefType typedefType)
                return null;

            return ProcessDeclaration(typedefType.Declaration, parent);
        }
    }
}
