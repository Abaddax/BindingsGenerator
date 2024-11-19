using BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts;
using BindingsGenerator.Generator.Unsafe.Internal.Models.Generator;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Generator.Generators
{
    internal class FrameworkGenerator : GeneratorBase
    {
        protected override string FileName => "Framework.g.cs";
        protected override string? CustomNamespace
        {
            get
            {
                if (Context.Options.GenerateFramework)
                    return $"{Context.Options.RootNamespace}.Framework";
                else
                    return "BindingsGenerator.Framework";
            }
        }

        public FrameworkGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public override IEnumerable<string> Usings()
        {
            yield return "System.Diagnostics";
            yield return "System.Runtime.InteropServices";
            yield return "System.Runtime.InteropServices.Marshalling";
            yield return "System.Runtime.Versioning";
        }
        protected override void GenerateDefinitions(IEnumerable<IDefinition> definitions)
        {
            if (!Context.Options.GenerateFramework)
                return;

            //INativeInstance
            WriteGeneratedCodeAttribute();
            WriteLine("public unsafe interface INativeInstance");
            using (BeginBlock())
            {
                WriteLine("protected internal void* Instance { init; }");
                WriteLine("protected internal void* GetInstance<T>() where T : unmanaged;");
            }
            WriteLine();

            //NativeObjectMarshaler
            WriteGeneratedCodeAttribute();
            WriteLine("public unsafe class NativeInstanceMarshaler<T>: ICustomMarshaler where T: unmanaged, INativeInstance");
            using (BeginBlock())
            {
                WriteLine("private static readonly NativeInstanceMarshaler<T> Instance = new();");
                WriteLine("public static ICustomMarshaler GetInstance(string cookie) => Instance;");
                WriteLine("public object MarshalNativeToManaged(IntPtr pNativeData) => new T() { Instance = (void*)pNativeData };");
                WriteLine("public IntPtr MarshalManagedToNative(object managedObj) => (IntPtr)((INativeInstance)managedObj).GetInstance<T>();");
                WriteLine("public void CleanUpNativeData(IntPtr pNativeData) { }");
                WriteLine("public void CleanUpManagedData(object managedObj) { }");
                WriteLine("public int GetNativeDataSize() => IntPtr.Size;");
            }
            WriteLine();

            //IFixedSizeArray
            WriteGeneratedCodeAttribute();
            WriteLine("public interface IFixedSizeArray");
            using (BeginBlock())
            {
                WriteLine("int Length { get; }");
            }
            WriteLine();

            //IFixedSizeArray<T>
            WriteGeneratedCodeAttribute();
            WriteLine("public interface IFixedSizeArray<T> : IFixedSizeArray");
            using (BeginBlock())
            {
                WriteLine("T this[uint index] { get; set; }");
                WriteLine("T[] ToArray();");
                WriteLine("void UpdateFrom(T[] array);");
            }
            WriteLine();

            /*
            //IFixedSizeArray_DebugView<T>
            WriteGeneratedCodeAttribute();
            WriteLine("public sealed class IFixedSizeArray_DebugView<T>");
            using (BeginBlock())
            {
                WriteLine("private readonly IFixedSizeArray<T> _array;");
                WriteLine("public IFixedSizeArray_DebugView(IFixedSizeArray<T> array) => _array = array ?? throw new ArgumentNullException(nameof(array));");
                WriteLine("[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]");
                WriteLine("public T[] Items => _array.ToArray();");
            }
            WriteLine();
            */

            //IFixedArray string Extensions
            WriteGeneratedCodeAttribute();
            WriteLine("public unsafe static class FixedSizeArrayExtensions");
            using (BeginBlock())
            {
                WriteLine($"public static string AsString(this IFixedSizeArray<char> array) {{ fixed (char* ptr = array.ToArray()) return new string(ptr); }}");
                WriteLine($"public static string AsString(this IFixedSizeArray<sbyte> array) {{ fixed (sbyte* ptr = array.ToArray()) return new string(ptr); }}");
            }
            WriteLine();
        }
    }
}
