using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models.OptionElements
{
    public class Define
    {
        [XmlAttribute("Value")]
        public string Value { get; set; } = string.Empty;

        #region Helper
        public static implicit operator Define(string define)
        {
            return new Define() { Value = define };
        }
        public static implicit operator string(Define define)
        {
            return define.Value;
        }
        #endregion
    }
}
