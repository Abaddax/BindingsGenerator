using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using CppSharp.AST;
using Type = CppSharp.AST.Type;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    /// <summary>
    /// Resolves: <br/>
    /// Builtin types <br/>
    /// Typedefs <br/>
    /// Nested types <br/>
    /// </summary>
    internal class DefaultTypeProcessor : ProcessorBase
    {
        public DefaultTypeProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(BuiltinType);
            yield return typeof(TagType);
            yield return typeof(AttributedType);
        }
        protected override IDefinition? ProcessType(Type type, Declaration parent)
        {
            return type switch
            {
                BuiltinType builtinType => GetBuiltinTypeDefinition(builtinType.Type),
                TagType tagType => GetDefinition(tagType.Declaration, parent),
                AttributedType attributedType => GetBuiltinTypeDefinition(PrimitiveType.Void),
                _ => null,
            };
        }

        private IDefinition GetBuiltinTypeDefinition(PrimitiveType builtinPrimitiveType)
        {
            //C++-Type -> C#-Type
            var typeName = builtinPrimitiveType switch
            {
                //8-Bit
                PrimitiveType.Bool => "bool",
                PrimitiveType.Char => "sbyte",
                PrimitiveType.SChar => "sbyte",
                PrimitiveType.UChar => "byte",
                //16-Bit
                PrimitiveType.Short => "short",
                PrimitiveType.UShort => "short",
                PrimitiveType.WideChar => "char",
                //32-Bit
                PrimitiveType.Int => "int",
                PrimitiveType.UInt => "uint",
                PrimitiveType.Long => "int",
                PrimitiveType.ULong => "uint",
                //64-Bit
                PrimitiveType.LongLong => "long",
                PrimitiveType.ULongLong => "ulong",
                //32-Bit IEEE-754 binary32 
                PrimitiveType.Float => "float",
                //64-Bit IEEE-754 binary64 
                PrimitiveType.Double => "double",
                //128-Bit IEEE-754 binary128 
                PrimitiveType.LongDouble => "decimal",
                PrimitiveType.Decimal => "decimal",
                //Rest
                PrimitiveType.Void => "void",
                PrimitiveType.IntPtr => "IntPtr",
                PrimitiveType.UIntPtr => "UIntPtr",
                PrimitiveType.Null => "null",
                _ => throw new ArgumentOutOfRangeException(nameof(builtinPrimitiveType))
            };
            return new EntityDefinition() { ID = typeName, Name = typeName };
        }
    }
}
