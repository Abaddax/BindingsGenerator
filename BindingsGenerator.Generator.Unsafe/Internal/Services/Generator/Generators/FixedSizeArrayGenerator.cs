using BindingsGenerator.Generator.Unsafe.Internal.Definition.Definitions;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;
using BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class FixedSizeArrayGenerator : GeneratorBase<FixedSizeArrayDefinition>
    {
        readonly TypeHelper _typeHelper;

        protected override string FileName => "Arrays.g.cs";

        public FixedSizeArrayGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _typeHelper = serviceProvider.GetRequiredService<TypeHelper>();
        }

        public override IEnumerable<string> Usings()
        {
            yield return "System.Diagnostics";
            if (Context.Options.GenerateFramework)
                yield return $"{Context.Options.RootNamespace}.Framework";
            else
                yield return "BindingsGenerator.Unsafe.Framework";
        }

        protected override void GenerateDefinition(FixedSizeArrayDefinition array)
        {
            using (BeginNamespace(array))
            {
                var length = array.Length;
                var elementType = array.ElementType;
                var elementTypeName = _typeHelper.GetFullTypeName(elementType);
                var arrayName = array.Name;

                WriteGeneratedCodeAttribute();
                if (array.IsPointer)
                    WriteLine($"public unsafe struct {arrayName} : IFixedSizeArray");
                else
                    //WriteLine($"[DebuggerTypeProxy(typeof(IFixedSizeArray_DebugView<{elementTypeName}>))]");
                    WriteLine($"public unsafe struct {arrayName} : IFixedSizeArray<{elementTypeName}>");

                using (BeginBlock())
                {
                    var lengthPropertyName = "Size";
                    WriteLine($"public static readonly int {lengthPropertyName} = {length};");
                    WriteLine($"public int Length => {lengthPropertyName};");

                    //Inline arrays causes debugger issues
                    //if (array.IsPrimitive)
                    //    WritePrimitiveFixedArray(elementTypeName, length);
                    //else
                    WriteComplexFixedArray(elementTypeName, length);

                    WriteLine($"public static implicit operator {elementTypeName}[]({arrayName} @struct) => @struct.ToArray();");
                }
            }
        }


        private void WritePrimitiveFixedArray(string elementType, int length)
        {
            WriteLine($"fixed {elementType} _[{length}];");
            WriteLine();

            WriteLine($"public {elementType} this[uint i]");

            using (BeginBlock())
            {
                WriteLine("get => _[i];");
                WriteLine("set => _[i] = value;");
            }

            WriteLine($"public {elementType}[] ToArray()");
            using (BeginBlock())
            {
                WriteLine($"var a = new {elementType}[{length}]; for (uint i = 0; i < {length}; i++) a[i] = _[i]; return a;");
            }

            WriteLine($"public void UpdateFrom({elementType}[] array)");
            using (BeginBlock())
            {
                WriteLine($"uint i = 0; foreach(var value in array) {{ _[i++] = value; if (i >= {length}) return; }}");
            }
        }
        private void WriteComplexFixedArray(string elementType, int length)
        {
            WriteLine(string.Join(" ", Enumerable.Range(0, length).Select(i => $"{elementType} _{i};")));
            WriteLine();

            var @fixed = $"fixed ({elementType}* p0 = &_0)";
            WriteLine($"public {elementType} this[uint i]");

            using (BeginBlock())
            {
                WriteLine($"get {{ if (i >= {length}) throw new ArgumentOutOfRangeException(); {@fixed} {{ return *(p0 + i); }} }}");
                WriteLine($"set {{ if (i >= {length}) throw new ArgumentOutOfRangeException(); {@fixed} {{ *(p0 + i) = value;  }} }}");
            }

            WriteLine($"public {elementType}[] ToArray()");
            using (BeginBlock())
            {
                WriteLine($"{@fixed} {{ var a = new {elementType}[{length}]; for (uint i = 0; i < {length}; i++) a[i] = *(p0 + i); return a; }}");
            }

            WriteLine($"public void UpdateFrom({elementType}[] array)");
            using (BeginBlock())
            {
                WriteLine($"{@fixed} {{ uint i = 0; foreach(var value in array) {{ *(p0 + i++) = value; if (i >= {length}) return; }} }}");
            }
        }
    }
}
