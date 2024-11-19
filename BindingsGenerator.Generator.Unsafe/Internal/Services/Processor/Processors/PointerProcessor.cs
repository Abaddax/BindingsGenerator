using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Processor;
using CppSharp.AST;
using Type = CppSharp.AST.Type;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Processor.Processors
{
    /// <summary>
    /// Process pointer types
    /// </summary>
    /// <example>int*</example>
    internal class PointerProcessor : ProcessorBase
    {
        public PointerProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public override IEnumerable<System.Type> Processable()
        {
            yield return typeof(PointerType);
        }

        protected override IDefinition? ProcessType(Type type, Declaration parent)
        {
            if (type is not PointerType pointerType)
                return null;

            var pointeeToken = GetToken(pointerType.Pointee, parent);

            return new PointerDefinition()
            {
                ID = GetPointerID(pointeeToken),
                Name = null,
                Type = pointeeToken,
            };
        }
        public static string GetPointerID(ITypeToken token)
        {
            return $"{token.Token}@p";
        }
    }
}
