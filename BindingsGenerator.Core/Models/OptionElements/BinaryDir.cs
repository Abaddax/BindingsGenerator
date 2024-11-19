using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models.OptionElements
{
    public class BinaryDir
    {
        [XmlAttribute("Path")]
        public string DirectoryName { get; set; } = string.Empty;

        #region Helper
        public void Deconstruct(out string fileName)
        {
            fileName = DirectoryName;
        }
        public static implicit operator BinaryDir(string binaryDir)
        {
            return new BinaryDir() { DirectoryName = binaryDir };
        }
        public static implicit operator string(BinaryDir binaryDir)
        {
            return binaryDir.DirectoryName;
        }
        #endregion
    }
}
