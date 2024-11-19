using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models.OptionElements
{
    public class IncludeDir
    {
        [XmlAttribute("Path")]
        public string DirectoryName { get; set; } = string.Empty;

        #region Helper
        public static implicit operator IncludeDir(string includeFile)
        {
            return new IncludeDir() { DirectoryName = includeFile };
        }
        public static implicit operator string(IncludeDir includeDir)
        {
            return includeDir.DirectoryName;
        }
        #endregion
    }
}
