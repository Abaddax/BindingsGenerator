using System;
using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models.OptionElements
{
    public class IncludeFile
    {
        [XmlAttribute("Name")]
        public string FileName { get; set; } = string.Empty;
        [XmlAttribute("ForceGeneration")]
        public bool ForceGeneration { get; set; } = true;

        #region Helper
        public void Deconstruct(out string fileName, out bool forceGeneration)
        {
            fileName = FileName;
            forceGeneration = ForceGeneration;
        }
        public static implicit operator IncludeFile(ValueTuple<string, bool> tuple)
        {
            return new IncludeFile() { FileName = tuple.Item1, ForceGeneration = tuple.Item2 };
        }
        public static implicit operator IncludeFile(string includeFile)
        {
            return new IncludeFile() { FileName = includeFile };
        }
        public static implicit operator string(IncludeFile includeFile)
        {
            return includeFile.FileName;
        }
        public bool ShouldSerializeForceGeneration()
        {
            return ForceGeneration != true;
        }
        #endregion

    }
}
