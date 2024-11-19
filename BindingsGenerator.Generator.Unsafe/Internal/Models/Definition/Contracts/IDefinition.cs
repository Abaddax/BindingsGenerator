using System.ComponentModel.DataAnnotations;

namespace BindingsGenerator.Generator.Unsafe.Internal.Definition.Contracts
{
    /// <summary>
    /// Contract for all declarations/types
    /// </summary>
    internal interface IDefinition
    {
        /// <summary>
        /// Unique ID of the definition
        /// </summary>
        [Key]
        string ID { get; }
        /// <summary>
        /// Name of the definition
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Translation unit file
        /// </summary>
        string File { get; }
    }
}
