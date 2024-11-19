using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models.OptionElements
{
    public class KnownType
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = string.Empty;
        [XmlAttribute("ID")]
        public string? TypeId { get; set; } = null;
        [XmlAttribute("Alias")]
        public string? Alias { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not KnownType type)
                return false;
            if (type.TypeId != null && TypeId != null)
                return type.TypeId == TypeId;
            return type.Name == Name;
        }
    }

}
