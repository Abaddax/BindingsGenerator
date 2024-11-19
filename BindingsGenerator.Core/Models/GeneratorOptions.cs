using BindingsGenerator.Core.Models.OptionElements;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BindingsGenerator.Core.Models
{
    public class GeneratorOptions
    {
        public GeneratorPlatform Platform { get; set; } = GeneratorPlatform.WindowsMscv;
        public GeneratorLanguageVersion LanguageVersion { get; set; } = GeneratorLanguageVersion.CPP20;

        /// <summary>
        /// Directories for <see cref="Includes"/>
        /// </summary>
        public List<IncludeDir> IncludeDirs { get; set; } = new List<IncludeDir>();
        /// <summary>
        /// Headers that will be parsed, each
        /// </summary>
        public List<TranslationUnit> TranslationUnits { get; set; } = new List<TranslationUnit>();

        /// <summary>
        /// Directories for <see cref="Binaries"/>
        /// </summary>
        public List<BinaryDir> BinaryDirs { get; set; } = new List<BinaryDir>();
        /// <summary>
        /// Binary-Files to import functions from (DllImport)
        /// </summary>
        public List<BinaryFile> Binaries { get; set; } = new List<BinaryFile>();

        /// <summary>
        /// Custrom defines when parsing <see cref="Includes"/>
        /// </summary>
        public List<Define> Defines { get; set; } = new List<Define>();

        /// <summary>
        /// RootNamespace of the generator output
        /// </summary>
        public string RootNamespace { get; set; } = "BindingsGenerator.Generated";
        /// <summary>
        /// All static functions will be generated in this static class
        /// </summary>
        public string StaticTypename { get; set; } = "Static";

        /// <summary>
        /// Add SuppressUnmanagedCodeSecurityAttribute to generated functions/delegates
        /// </summary>
        public bool SuppressUnmanagedCodeSecurity { get; set; } = true;
        /// <summary>
        /// Generate the framework as part of the generator output <br/>
        /// If false, the framework needs to be imported with a package
        /// </summary>
        public bool GenerateFramework { get; set; } = false;
        /// <summary>
        /// Generate the COM-Bindings
        /// </summary>
        public bool GenerateCOMBindings { get; set; } = false;
        /// <summary>
        /// This project will not be used in another generated project (final project)
        /// </summary>
        public bool IsFinal { get; set; } = true;

        /// <summary>
        /// Ignore these types when generating
        /// </summary>
        /// <remarks>Types might be missing and need to imported!</remarks>
        [XmlArrayItem("TypeID")]
        public List<string> KnownTypes { get; set; } = new List<string>();

        /// <summary>
        /// Macros that are known and will not be parsed
        /// </summary>
        [XmlArrayItem("Macro")]
        public List<string> KnownMacros { get; set; } = new List<string>();
        /// <summary>
        /// Macros will be ignored my default! <br/>
        /// To parse macros anyways this will be used to generate the needed code.
        /// </summary>
        /// <remarks>
        /// Raw injection of <see cref="CustomMacros.Replace"/> in generated output!
        /// </remarks>
        public List<CustomMacro> CustomMacros { get; set; } = new List<CustomMacro>();

        /// <summary>
        /// Add these using to every generated file
        /// </summary>
        [XmlArrayItem("Using")]
        public List<string> CustomUsings { get; set; } = new List<string>();

        /// <summary>
        /// Regex to find COM-Classes
        /// </summary>
        /// <remarks>
        /// First capture-group must be GUID
        /// </remarks>
        [XmlArrayItem("MarkerRegex")]
        public List<string> ComMarkers { get; set; } = new List<string>();
    }
}
