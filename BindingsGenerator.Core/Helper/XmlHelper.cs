using System.IO;
using System.Xml.Serialization;

namespace BindingsGenerator.Core.Helper
{
    internal static class XmlHelper
    {
        public static void Serialize<T>(this T value, Stream stream)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            ser.Serialize(stream, value);
        }
        public static T Deserialize<T>(this Stream stream)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            return (T)ser.Deserialize(stream);
        }
    }
}
