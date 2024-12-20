using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common;
using CppSharp.AST;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    internal class UnionProcessor : ProcessorBase
    {
        readonly NamespaceHelper _namespaceHelper;

        public UnionProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _namespaceHelper = serviceProvider.GetRequiredService<NamespaceHelper>();
        }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(Class);
        }

        protected override IDefinition? ProcessDeclaration(Declaration declaration, Declaration? parent)
        {
            if (declaration is not Class union)
                return null;

            if (!union.IsUnion)
                return null; //Not a union

            //Check if currently processing
            var processingDefinition = TryGetProcessingDefinition(union.USR);
            if (processingDefinition != null)
                return processingDefinition;

            //Check if already existing
            var definition = TryGetDefinition<UnionDefinition>(union.USR);
            if (definition?.IsCompleted == true)
                return definition; //Already processed

            using (var processing =
                BeginProcess(new UnionDefinition()
                {
                    ID = union.USR,
                    Name = union.GetName(parent),
                    Namespace = _namespaceHelper.GetNamespace(union, parent),
                }))
            {
                //Create/Update definition
                definition = new UnionDefinition
                {
                    ID = processing.Definition.ID,
                    Name = processing.Definition.Name,
                    Namespace = processing.Definition.Namespace,
                    Documentation = union.GetDocumentation(),
                    Obsoletion = union.GetObsoletion(),
                    IsCompleted = !union.IsIncomplete,
                    Fields = ListFields(union, definition)
                        .ToArray(),
                    File = union.TranslationUnit.FileName
                };

                _namespaceHelper.CopyNestedTypes(processing.Definition, definition);
                _namespaceHelper.AppendToParentNamespace(definition);
                return definition;
            }
        }

        private IEnumerable<MemberField> ListFields(Class union, UnionDefinition? existingDefinition = null)
        {
            var fields = new List<MemberField>(existingDefinition?.Fields ?? Array.Empty<MemberField>());
            for (int i = 0; i < union.Fields.Count; i++)
            {
                var field = union.Fields[i];

                if (string.IsNullOrEmpty(field.Name))
                    field.Name = $"anonymousField_{i}"; //no field name -> anonymous
                if (fields.Any(f => f.Name == field.Name))
                    continue; //Already present

                //Create new
                var fieldToken = GetToken(field.Type, field);

                var memberField = new MemberField()
                {
                    Name = field.Name,
                    FieldType = fieldToken,
                    FieldOffset = union.Layout.Fields.FirstOrDefault(f => f.Name == field.Name)?.Offset ?? union.Layout.Fields[i].Offset,
                    IsStatic = field.IsStatic,
                    AccessSpecifier = field.Access,
                    Documentation = field.GetDocumentation(),
                    Obsoletion = field.GetObsoletion()
                };
                fields.Add(memberField);
            }
            return fields;
        }
    }
}
