using BindingsGenerator.Core;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Common;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Common;
using CppSharp.AST;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    internal class ObjectProcessor : ProcessorBase
    {
        readonly NamespaceHelper _namespaceHelper;

        public ObjectProcessor(IServiceProvider serviceProvider)
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
            if (declaration is not Class @class)
                return null;

            if (@class.IsUnion)
                return null; //Unions are not processed

            //Check if currently processing
            var processingDefinition = TryGetProcessingDefinition(@class.USR);
            if (processingDefinition != null)
                return processingDefinition;

            //Check if already existing
            var definition = TryGetDefinition<ObjectDefinition>(@class.USR);
            if (definition?.IsCompleted == true)
                return definition; //Already processed

            using (var processing = BeginProcess(new ObjectDefinition()
            {
                ID = @class.USR,
                Name = @class.GetName(parent),
                Namespace = _namespaceHelper.GetNamespace(@class, parent),
                IsCompleted = false
            }))
            {
                //Create/Update definition
                definition = new ObjectDefinition
                {
                    ID = processing.Definition.ID,
                    Name = processing.Definition.Name,
                    Namespace = processing.Definition.Namespace,
                    Documentation = @class.GetDocumentation(),
                    Obsoletion = @class.GetObsoletion(),
                    IsCompleted = !@class.IsIncomplete,
                    IsInherited = definition?.IsInherited ?? false,
                    IsComObject = FindComMarker(@class),
                    Fields = ListFields(@class, definition)
                        .Concat(ListVTables(@class))
                        .OrderBy(f => f.FieldOffset)
                        .ToArray(),
                    Functions = ListFunctions(@class, definition)
                        .ToArray(),
                    Bases = ListInheritance(@class, definition)
                        .ToArray(),
                    File = @class.TranslationUnit.FileName
                };

                _namespaceHelper.CopyNestedTypes(processing.Definition, definition);
                _namespaceHelper.AppendToParentNamespace(definition);
                return definition;
            }
        }


        private IEnumerable<MemberField> ListFields(Class @class, ObjectDefinition? existingDefinition = null)
        {
            var fields = new List<MemberField>(existingDefinition?.Fields ?? Array.Empty<MemberField>());
            //Scan fields
            foreach (var field in @class.Fields)
            {
                if (string.IsNullOrEmpty(field.Name))
                    continue; //no field name -> invalid
                if (fields.Any(f => f.Name == field.Name))
                    continue; //Already present

                //Create new
                var fieldToken = GetToken(field.Type, field);

                var memberField = new MemberField()
                {
                    Name = field.Name,
                    FieldType = fieldToken,
                    FieldOffset = @class.Layout.Fields.First(f => f.Name == field.Name).Offset,
                    IsStatic = field.IsStatic,
                    AccessSpecifier = field.Access,
                    Documentation = field.GetDocumentation(),
                    Obsoletion = field.GetObsoletion(),
                };
                fields.Add(memberField);
            }

            return fields;
        }
        private IEnumerable<MemberFunction> ListFunctions(Class @class, ObjectDefinition? existingDefinition = null)
        {
            var functions = new List<MemberFunction>(existingDefinition?.Functions ?? Array.Empty<MemberFunction>());
            //Scan functions
            foreach (var function in @class.Methods)
            {
                if (function.Kind != CXXMethodKind.Normal)
                {
                    Logger?.LogWarning("", $"Skipped function of type {function.Kind} for {@class.Name}", function.TranslationUnit.FileName, function.LineNumberStart);
                    continue;
                }

                //Create new
                var token = GetToken(function, null);
                if (token.Definition is IAliasDefinition alias)
                    token = alias.TryCast<FunctionDefinition>();
                if (token is not ITypeToken<FunctionDefinition> functionToken)
                    throw new InvalidOperationException($"Class function can not be processed. {function.Type?.GetType()}");

                if (functions.Any(member => member.FunctionType.FinalDefinition.ID == functionToken.FinalDefinition.ID))
                    continue; //Already present

                ITypeToken<FunctionDefinition> overridden = null;
                if (function.BaseMethod != null)
                    overridden = GetToken(function.BaseMethod, null) as ITypeToken<FunctionDefinition>;

                var memberFunction = new MemberFunction()
                {
                    Name = function.Name,
                    FunctionType = functionToken,
                    AccessSpecifier = function.Access,
                    FunctionKind = function.Kind,
                    IsStatic = function.IsStatic,
                    IsVirtual = function.IsVirtual,
                    Overridden = overridden,
                    //IsOverride = function.IsOverride,
                    Documentation = function.GetDocumentation(),
                    Obsoletion = function.GetObsoletion(),
                };
                functions.Add(memberFunction);
            }
            return functions;
        }
        private IEnumerable<InheritedType> ListInheritance(Class @class, ObjectDefinition? existingDefinition = null)
        {
            var inheritedTypes = new List<InheritedType>(existingDefinition?.Bases ?? Array.Empty<InheritedType>());
            foreach (var inherited in @class.Bases)
            {
                var token = GetToken(inherited.Type, @class);
                if (token.Definition is IAliasDefinition alias)
                    token = alias.TryCast<ObjectDefinition>();
                if (token is not ITypeToken<ObjectDefinition> objectToken)
                    throw new InvalidOperationException($"Class inheritance can not be processed. {inherited.Type?.GetType()}");

                var exisingObject = TryGetDefinition<ObjectDefinition>(objectToken);
                exisingObject.IsInherited = true;

                if (inheritedTypes.Any(f => f.Type.Token == objectToken.Token))
                    continue; //Already present

                var inheritedType = new InheritedType()
                {
                    Type = objectToken,
                    TypeOffset = (uint)inherited.Offset,
                    AccessSpecifier = inherited.Access,
                };
                inheritedTypes.Add(inheritedType);
            }
            return inheritedTypes;
        }
        private IEnumerable<MemberField> ListVTables(Class @class)
        {
            var vTables = new List<MemberField>();
            int index = 1;

            //Add vtables
            foreach (var vTable in @class.Layout.VFTables)
            {
                var vTableID = $"{@class.USR}@VT@VTable{index}";
                var vTableName = $"VTable{index++}";

                var classDefinition = TryGetDefinition<ObjectDefinition>(@class.USR);
                if (classDefinition == null)
                    throw new InvalidOperationException($"VTable can not be processed. Unknown class. {@class}");

                //Check if already existing and build if needed
                var vTableDefinition = TryGetDefinition<VTableDefinition>(vTableID);
                //Build vtable
                vTableDefinition ??= new VTableDefinition()
                {
                    ID = vTableID,
                    Name = vTableName,
                    Namespace = TryStoreDefinition(classDefinition),
                    Object = TryStoreDefinition(classDefinition),
                    Documentation = "Generated VTable",
                    File = @class.TranslationUnit.FileName
                };

                //List functions
                var vFuncs = new List<VTableFunction>();
                for (int offset = 0; offset < vTable.Layout.Components.Count; offset++)
                {
                    var funcPtr = vTable.Layout.Components[offset];

                    if (funcPtr.Method.Kind != CXXMethodKind.Normal)
                    {
                        Logger.LogWarning("", $"Skipped vtable function of type {funcPtr.Method.Kind} for {@class.Name}", @class.TranslationUnit.FileName, @class.LineNumberStart);
                        continue;
                    }

                    //Get virtual function
                    var funcToken = GetToken(funcPtr.Method, null);

                    //VTable contains function pointer
                    var funcPtrDefinition = new PointerDefinition()
                    {
                        ID = PointerProcessor.GetPointerID(funcToken),
                        Name = null,
                        Type = funcToken,
                    };
                    var funcPtrToken = TryStoreDefinition(funcPtrDefinition);

                    //Build vtable function
                    var vFunc = new VTableFunction()
                    {
                        Name = funcPtr.Method.Name,
                        FunctionType = funcPtrToken,
                        FunctionOffset = (uint)(offset * @class.Layout.Alignment),
                        AccessSpecifier = AccessSpecifier.Public,
                        Documentation = "Virtual function pointer",
                    };
                    vFuncs.Add(vFunc);
                }
                vTableDefinition.Functions = vFuncs.ToArray();

                ITypeToken vTableToken = TryStoreDefinition(vTableDefinition);
                if (vTableToken == null)
                    throw new InvalidOperationException("This should never happen");//continue;

                //Class stores only pointer to vTable
                var vTablePtr = new PointerDefinition()
                {
                    ID = PointerProcessor.GetPointerID(vTableToken),
                    Name = null,
                    Type = vTableToken,
                };
                vTableToken = TryStoreDefinition(vTablePtr);

                //Add VTable to fields
                var memberField = new MemberField()
                {
                    Name = $"{vTableName}Ptr",
                    FieldType = vTableToken,
                    FieldOffset = (uint)vTable.VFPtrOffset,
                    IsVTable = true,
                    AccessSpecifier = AccessSpecifier.Public,
                    Documentation = "VTable-Pointer",
                };
                vTables.Add(memberField);
            }

            return vTables;
        }

        private Guid? FindComMarker(Class @class)
        {
            List<Regex> regexs = new List<Regex>()
            {
                new Regex("MIDL_INTERFACE\\(\\\"([a-fA-F0-9\\-]*)\"\\)"),
                new Regex("DECLSPEC_UUID\\(\\\"([a-fA-F0-9\\-]*)\"\\)"),
                new Regex("DX_DECLARE_INTERFACE\\(\\\"([a-fA-F0-9\\-]*)\"\\)")
            };

            foreach (var macro in @class.PreprocessedEntities.Where(x => x.MacroLocation == MacroLocation.ClassHead))
            {
                var text = macro switch
                {
                    CppSharp.AST.MacroDefinition macroDefinition => macroDefinition.Expression,
                    CppSharp.AST.MacroExpansion macroExpansion => macroExpansion.Text,
                    _ => null
                };
                if (string.IsNullOrEmpty(text))
                    continue;

                foreach (var regex in regexs)
                {
                    var match = regex.Match(text);
                    if (!match.Success)
                        continue;
                    if (!Guid.TryParse(match.Groups[1].Value, out var guid))
                        continue;
                    return guid;
                }
            }

            return null;
        }
    }
}
