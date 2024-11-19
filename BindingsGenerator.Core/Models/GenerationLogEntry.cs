using System.Diagnostics;

namespace BindingsGenerator.Core.Models
{
    public enum GenerationLogLevel
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
    [DebuggerDisplay("{Message}")]
    public struct GenerationLogEntry
    {
        public GenerationLogLevel Level { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// ErrorCode of this Warning/Error
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// File-Name this Warning/Error got raised from
        /// </summary>
        public string File { get; set; }
        /// <summary>
        /// File-Line this Warning/Error got raised from
        /// </summary>
        public int Line { get; set; }
    }
}
