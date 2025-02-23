using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
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
            var id = $"{typeToken.Token}@T@{typedefDecl.Name}";

            var typeDefinition = new TypeDefinition()
            {
                ID = id,
                Name = typedefDecl.Name,
                File = typedefDecl.TranslationUnit.FileName,
                Type = typeToken,
                Namespace = _namespaceHelper.GetNamespace(declaration, parent),
                Documentation = declaration.GetDocumentation(),
                Obsoletion = declaration.GetObsoletion(),
            };

            //Do not typedef pointers -> global using will fail!
            if (typeToken.Definition is PointerDefinition)
                return ProcessTypeDefPointer(typeDefinition);

            return typeDefinition;
        }
        protected override IDefinition? ProcessType(Type type, Declaration parent)
        {
            if (type is not TypedefType typedefType)
                return null;

            return ProcessDeclaration(typedefType.Declaration, parent);
        }

        //INTPTR = int* -> INTPTR = int -> INTPTR* -> int*
        //TODO:
        //INTPTR2 = INTPTR* -> INTPTR2 = INTPTR -> INTPTR2* -> INTPTR**
        private IDefinition ProcessTypeDefPointer(TypeDefinition defaultTypedefinition)
        {
            var pointer = defaultTypedefinition.Type.Definition as PointerDefinition ?? throw new ArgumentException(nameof(defaultTypedefinition));
            var pointerDepth = pointer.GetPointerDepth();
            var pointedType = pointer.GetPointedType();

            IDefinition typeDefinition = new TypeDefinition()
            {
                ID = defaultTypedefinition.ID,
                Name = defaultTypedefinition.Name,
                File = defaultTypedefinition.File,
                Type = TryStoreDefinition(pointedType),
                Namespace = defaultTypedefinition.Namespace,
                Documentation = defaultTypedefinition.Documentation,
                Obsoletion = defaultTypedefinition.Obsoletion,
            };

            for (int i = 0; i < pointerDepth; i++)
            {
                typeDefinition = new PointerDefinition()
                {
                    ID = $"{typeDefinition.ID}@tdp",
                    Name = typeDefinition.Name,
                    Type = TryStoreDefinition(typeDefinition),
                    File = typeDefinition.File,
                };
            }
            return typeDefinition;
        }
    }
}
