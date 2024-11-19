using BindingsGenerator.Core;
using BindingsGenerator.Core.Models;
using System.Text;
using System.Xml.Serialization;

namespace BindingsGenerator.Generator.Unsafe
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GeneratorOptions options = new()
            {
                TranslationUnits = new List<Core.Models.OptionElements.TranslationUnit>()
                {
                    new Core.Models.OptionElements.TranslationUnit()
                    {
                        Includes = new List<Core.Models.OptionElements.IncludeFile>()
                        {
                            new Core.Models.OptionElements.IncludeFile()
                            {
                                FileName = "test"
                            },
                            new Core.Models.OptionElements.IncludeFile()
                            {
                                FileName = "test2",
                                ForceGeneration = false
                            }
                        }
                    }
                },
                ComMarkers = new List<string>()
                {
                    "1234"
                },
                BinaryDirs = new List<Core.Models.OptionElements.BinaryDir>()
                {
                    new Core.Models.OptionElements.BinaryDir(){ DirectoryName = "Dir"}
                },
                Binaries = new List<Core.Models.OptionElements.BinaryFile>()
                {
                    new Core.Models.OptionElements.BinaryFile(){FileName = "123"}
                },
                Defines = new List<Core.Models.OptionElements.Define>()
                {
                    new Core.Models.OptionElements.Define(){ Value = "123"}
                }
            };

            using (var ms = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(typeof(GeneratorOptions));
                ser.Serialize(ms, options);
                var str = Encoding.UTF8.GetString(ms.ToArray());
            }

            Generator generator = new Generator();

            GenerationRunner.Run(generator, args);
        }
    }
}
