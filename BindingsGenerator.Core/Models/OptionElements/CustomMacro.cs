using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models.OptionElements
{
    public class CustomMacro
    {
        [XmlElement("Regex")]
        public string Regex { get; set; } = string.Empty;
        [XmlElement("Replace")]
        public string Replace { get; set; } = string.Empty;
    }
}
