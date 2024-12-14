namespace AutoGenBindings.Generator.Unsafe.Internal.Models.Generator
{
    [Flags]
    internal enum Usage
    {
        Unknown = 0,
        ReturnValue = 1 << 0,
        Parameter = 1 << 1,
        Field = 1 << 2,
        COM = 1 << 3,
    }
}
