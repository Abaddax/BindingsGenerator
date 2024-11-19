using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    internal class EnumerationProcessor : ProcessorBase
    {
        readonly NamespaceHelper _namespaceHelper;

        public EnumerationProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _namespaceHelper = serviceProvider.GetRequiredService<NamespaceHelper>();
        }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(Enumeration);
        }

        protected override IDefinition? ProcessDeclaration(Declaration declaration, Declaration? parent)
        {
            if (declaration is not Enumeration enumeration)
                return null;

            if (!enumeration.Type.IsPrimitiveType())
                throw new NotSupportedException();

            //Check if currently processing
            var processingDefinition = TryGetProcessingDefinition(enumeration.USR);
            if (processingDefinition != null)
                return processingDefinition;

            //Check if already existing
            var definition = TryGetDefinition<EnumerationDefinition>(enumeration.USR);
            if (definition != null)
                return definition; //Already processed

            using (var processing = BeginProcess(enumeration.USR))
            {
                //Create definition
                definition = new EnumerationDefinition
                {
                    ID = enumeration.USR,
                    Name = enumeration.GetName(parent),
                    Namespace = _namespaceHelper.GetNamespace(enumeration, parent),
                    IsCompleted = true,
                    UnderlyingType = GetToken(enumeration.Type, enumeration),
                    Documentation = enumeration.GetDocumentation(),
                    Obsoletion = enumeration.GetObsoletion(),
                    Items = enumeration.Items
                       .Select(x =>
                           new EnumerationItem
                           {
                               Name = x.Name,
                               Value = GetEnumerationItemValue(x, enumeration.BuiltinType.Type),
                               Documentation = x.GetDocumentation(),
                               Obsoletion = x.GetObsoletion(),
                           })
                       .ToArray(),
                    File = enumeration.TranslationUnit.FileName,
                };
                _namespaceHelper.AppendToParentNamespace(definition);
                return definition;
            }
        }

        private static string GetEnumerationItemValue(Enumeration.Item item, PrimitiveType enumerationType)
        {
            var value = item.Value;
            object _value = enumerationType switch
            {
                PrimitiveType.Int => (int)value,
                PrimitiveType.UInt => (uint)value,
                PrimitiveType.Long => (long)value,
                PrimitiveType.ULong => value,
                _ => value,
            };
            return _value.ToString();
        }
    }
}
