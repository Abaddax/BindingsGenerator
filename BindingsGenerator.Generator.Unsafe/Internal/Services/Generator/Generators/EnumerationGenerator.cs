﻿using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;


namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class EnumerationGenerator : GeneratorBase<EnumerationDefinition>
    {
        readonly TypeHelper _typeHelper;

        protected override string FileName => "Enums.g.cs";

        public EnumerationGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }

        protected override NameScope? GenerateTypeScope(EnumerationDefinition @enum)
        {
            return new NameScope()
            {
                ScopeName = @enum.Name,
                IsNamespace = false,
                ScopePrefix = null, //enums do not support nesting
                ParentScope = GenerateScope(@enum.Namespace?.Definition)
            };
        }

        protected override void GenerateDefinition(EnumerationDefinition @enum)
        {
            using (BeginNamespace(@enum))
            {
                WriteSummary(@enum);
                WriteGeneratedCodeAttribute();
                WriteObsoletion(@enum);
                WriteLine($"public enum {@enum.Name} : {_typeHelper.GetFullTypeName(@enum.UnderlyingType)}");

                using (BeginBlock())
                {
                    foreach (var item in @enum.Items)
                    {
                        WriteSummary(item);
                        WriteLine($"@{item.Name} = {item.Value},");
                    }
                }
            }
        }
    }
}