using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models.OptionElements
{
    public class BinaryFile
    {
        [XmlAttribute("Name")]
        public string FileName { get; set; } = string.Empty;

        #region Helper
        public void Deconstruct(out string fileName)
        {
            fileName = FileName;
        }
        public static implicit operator BinaryFile(string binaryFile)
        {
            return new BinaryFile() { FileName = binaryFile };
        }
        public static implicit operator string(BinaryFile binaryFile)
        {
            return binaryFile.FileName;
        }
        #endregion
    }
}
