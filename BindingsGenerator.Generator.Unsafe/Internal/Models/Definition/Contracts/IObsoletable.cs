using System.Diagnostics;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts
{
    /// <summary>
    /// Marker for obsoleted code
    /// </summary>
    /// <example>[Obsolete(message)]</example>
    [DebuggerDisplay("{IsObsolete?\"Obsolete\":\"Not Obsolete\"}")]
    internal struct Obsoletion
    {
        public bool IsObsolete { get; init; }
        public string Message { get; init; }
    }
    internal interface IObsoletable
    {
        /// <summary>
        /// Obsoletion info
        /// </summary>
        Obsoletion Obsoletion { get; }
    }
}
