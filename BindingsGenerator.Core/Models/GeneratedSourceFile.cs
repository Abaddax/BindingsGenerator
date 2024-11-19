using System.Diagnostics;

namespace BindingsGenerator.Core.Models
{
    [DebuggerDisplay("{Filename}")]
    public struct GeneratedSourceFile
    {
        public string Filename { get; set; }
        public string Content { get; set; }
    }
}
