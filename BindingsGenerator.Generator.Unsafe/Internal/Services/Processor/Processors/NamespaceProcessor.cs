using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common;
using CppSharp.AST;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    internal class NamespaceProcessor : ProcessorBase
    {
        readonly NamespaceHelper _namespaceHelper;

        public NamespaceProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _namespaceHelper = serviceProvider.GetRequiredService<NamespaceHelper>();
        }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(Namespace);
        }

        protected override IDefinition? ProcessDeclaration(Declaration declaration, Declaration? parent)
        {
            if (declaration is not Namespace @namespace)
                return null;

            //Check if currently processing
            var processingDefinition = TryGetProcessingDefinition(@namespace.USR);
            if (processingDefinition != null)
                return processingDefinition;

            //Check if already existing
            var definition = TryGetDefinition<NamespaceDefinition>(@namespace.USR);
            if (definition != null) //Namespace can be reuses in different translation unit
            {
                using (var processing = BeginProcess(definition))
                {
                    //Process namespace declarations
                    foreach (var decl in @namespace.Declarations)
                    {
                        TryGetDefinition(decl, @namespace);
                    }
                    return definition;
                }
            }
            else //Create new namespacer
            {
                using (var processing =
                    BeginProcess(new NamespaceDefinition()
                    {
                        ID = @namespace.USR,
                        Name = @namespace.Name,
                        IsRoot = false,
                        Namespace = _namespaceHelper.GetNamespace(@namespace, parent),
                    }))
                {
                    //Create/Update definition
                    definition = new NamespaceDefinition()
                    {
                        ID = processing.Definition.ID,
                        Name = processing.Definition.Name,
                        IsRoot = processing.Definition.IsRoot,
                        Namespace = processing.Definition.Namespace,
                        File = @namespace.TranslationUnit.FileName,
                    };

                    //Process namespace declarations
                    foreach (var decl in @namespace.Declarations)
                    {
                        TryGetDefinition(decl, @namespace);
                    }

                    _namespaceHelper.CopyNestedTypes(processing.Definition, definition);
                    _namespaceHelper.AppendToParentNamespace(definition);
                    return definition;
                }
            }
        }
    }
}
