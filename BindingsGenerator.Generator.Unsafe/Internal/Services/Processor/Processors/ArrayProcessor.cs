using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using Type = CppSharp.AST.Type;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    internal class ArrayProcessor : ProcessorBase
    {
        public ArrayProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(ArrayType);
        }

        protected override IDefinition? ProcessType(Type type, Declaration parent)
        {
            const string defaultArrayName = "array";
            const string ptrArrayName = "ptrArray";

            if (type is not ArrayType arrayType)
                return null;

            var elementType = arrayType.Type;
            var elementTypeToken = GetToken(arrayType.Type, parent);

            //Unknown size -> char[] -> char*
            if (arrayType.SizeType != ArrayType.ArraySize.Constant)
            {
                return new PointerDefinition()
                {
                    ID = PointerProcessor.GetPointerID(elementTypeToken),
                    Name = null,
                    Type = elementTypeToken
                };
            }

            var fixedSize = (int)arrayType.Size;
            var isPointer = elementType.IsPointer();

            var arrayName = isPointer ? ptrArrayName : defaultArrayName;
            var elementTypeName = isPointer ? GetDefinition(elementType.GetPointee(), parent).Name : elementTypeToken.Definition.Name;

            //Single dimenstion array
            if (elementType is not ArrayType elementArrayType)
            {
                var id = $"{elementTypeToken.Token}@{(isPointer ? "pA" : "A")}@{fixedSize}";
                var name = $"{elementTypeName}_{arrayName}{fixedSize}";
                //Check if already existing
                var definition = TryGetDefinition<FixedSizeArrayDefinition>(id);
                if (definition != null)
                    return definition; //Already processed

                return new FixedSizeArrayDefinition
                {
                    ID = id,
                    Name = name,
                    Length = fixedSize,
                    ElementType = elementTypeToken,
                    IsPrimitive = elementType.IsPrimitiveType(),
                    IsPointer = isPointer,
                    File = parent.TranslationUnit.FileName
                };
            }

            //Multidimentional
            if (elementArrayType.SizeType == ArrayType.ArraySize.Constant)
            {
                fixedSize = fixedSize / (int)elementArrayType.Size;
                var id = $"{elementTypeToken.Token}x{fixedSize}";
                var name = $"{elementTypeToken.Definition.Name}x{fixedSize}";

                //Check if already existing
                var definition = TryGetDefinition<FixedSizeArrayDefinition>(id);
                if (definition != null)
                    return definition; //Already processed

                return new FixedSizeArrayDefinition
                {
                    ID = id,
                    Name = name,
                    Length = fixedSize,
                    ElementType = elementTypeToken,
                    IsPrimitive = elementType.IsPrimitiveType(),
                    IsPointer = isPointer,
                    File = parent.TranslationUnit.FileName
                };
            }

            throw new NotImplementedException("TODO");
        }
    }
}
