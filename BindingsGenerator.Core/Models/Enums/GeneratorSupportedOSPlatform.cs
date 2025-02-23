using System;

namespace BindingsGenerator.Core.Models.Enums
{
    [Flags]
    public enum GeneratorSupportedOSPlatform
    {
        All = 0b0001,
        Current = 0b0010,
        Windows = 0b0100,
        Linux = 0b1000,
    }
}
