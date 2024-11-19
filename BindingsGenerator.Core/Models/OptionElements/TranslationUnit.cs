using System.Collections.Generic;
using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models.OptionElements
{
    public class TranslationUnit
    {
        [XmlElement("Include")]
        public List<IncludeFile> Includes { get; set; } = new List<IncludeFile>();
    }
}
