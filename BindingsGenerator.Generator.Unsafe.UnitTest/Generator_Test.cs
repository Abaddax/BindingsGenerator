using BindingsGenerator.Core;
using BindingsGenerator.Core.Models;
using System.Text;
using System.Xml.Serialization;

namespace BindingsGenerator.Generator.Unsafe.UnitTest
{
    [TestClass]
    public class Generator_Test
    {
        GeneratorOptions options = new GeneratorOptions()
        {
            RootNamespace = "DXGI.net",
            StaticTypename = "dxgi",

            Defines = new()
            {

            },

            IncludeDirs = new()
            {
                @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0"
            },
            Includes = new()
            {
                // dxgi
                "shared/dxgi.h",
                "shared/dxgi1_2.h",
                "um/d3d11.h",
                "um/d2d1.h",
            },

            BinaryDirs = new()
            {
                @"C:\Windows\System32"
            },
            Binaries = new()
            {
                "dxgi.dll",
                "d3d11.dll",
                "d2d1.dll"
            },
        };
        GeneratorOptions options2 = new GeneratorOptions()
        {
            RootNamespace = "FFmpeg.net",
            StaticTypename = "ffmpeg",

            Defines = new()
                    {
                        "__STDC_CONSTANT_MACROS"
                    },

            BinaryDirs = new()
                    {
                        @"C:\Users\Max\source\repos\remote-desktop-library\remote-desktop-library\extern\redist\win-x64\bin"
                    },
            Binaries = new()
                    {
                        "ffmpeg.dll"
                    },
            IncludeDirs = new()
                    {
                        @"C:\Users\Max\source\repos\remote-desktop-library\remote-desktop-library\extern\redist\win-x64\include"
                    },
            Includes = new()
                    {
                        // libavutil
                        "libavutil/avutil.h",
                        "libavutil/audio_fifo.h",
                        "libavutil/channel_layout.h",
                        "libavutil/cpu.h",
                        "libavutil/file.h",
                        "libavutil/frame.h",
                        "libavutil/opt.h",
                        "libavutil/imgutils.h",
                        "libavutil/time.h",
                        "libavutil/timecode.h",
                        "libavutil/tree.h",
                        "libavutil/hwcontext.h",
                        "libavutil/hwcontext_dxva2.h",
                        "libavutil/hwcontext_d3d11va.h",
                        "libavutil/hdr_dynamic_metadata.h",
                        "libavutil/mastering_display_metadata.h",

                        // libswresample
                        "libswresample/swresample.h",

                        // libpostproc
                        "libpostproc/postprocess.h",

                        // libswscale
                        "libswscale/swscale.h",

                        // libavcodec
                        "libavcodec/avcodec.h",
                        "libavcodec/bsf.h",
                        "libavcodec/dxva2.h",
                        "libavcodec/d3d11va.h",

                        // libavformat
                        "libavformat/avformat.h",

                        // libavfilter
                        "libavfilter/avfilter.h",
                        "libavfilter/buffersrc.h",
                        "libavfilter/buffersink.h",

                        // libavdevice
                        "libavdevice/avdevice.h"
                    }
        };




        [TestMethod]
        public void T1_GenerateBindings()
        {
            Generator generator = new Generator()
            {
                Options = options2
            };

            var fileCollector = new InMemoryGeneratedFileCollector();
            var logCollector = new ConsoleGenerationLogCollector();
            generator.GenerateBindings(fileCollector, logCollector);

            Console.WriteLine(fileCollector.GeneratedSourceFiles.Last().Content);
        }
    }

    [TestClass]
    public class Generator_Test2
    {
        GeneratorOptions options = new GeneratorOptions()
        {
            RootNamespace = "DXGI.net",
            StaticTypename = "dxgi",

            Defines = new()
            {

            },

            IncludeDirs = new()
            {
                @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0"
            },
            Includes = new()
            {
                // dxgi
                "shared/dxgi.h",
                "shared/dxgi1_2.h",
                "um/d3d11.h",
                "um/d2d1.h",
            },

            BinaryDirs = new()
            {
                @"C:\Windows\System32"
            },
            Binaries = new()
            {
                "dxgi.dll",
                "d3d11.dll",
                "d2d1.dll"
            },
        };
        GeneratorOptions options2 = new GeneratorOptions()
        {
            RootNamespace = "FFmpeg.net",
            StaticTypename = "ffmpeg",

            Defines = new()
                    {
                        "__STDC_CONSTANT_MACROS"
                    },

            BinaryDirs = new()
                    {
                        @"C:\Users\Max\source\repos\remote-desktop-library\remote-desktop-library\extern\redist\win-x64\bin"
                    },
            Binaries = new()
                    {
                        "ffmpeg.dll"
                    },
            IncludeDirs = new()
                    {
                        @"C:\Users\Max\source\repos\remote-desktop-library\remote-desktop-library\extern\redist\win-x64\include"
                    },
            Includes = new()
                    {
                        // libavutil
                        "libavutil/avutil.h",
                        "libavutil/audio_fifo.h",
                        "libavutil/channel_layout.h",
                        "libavutil/cpu.h",
                        "libavutil/file.h",
                        "libavutil/frame.h",
                        "libavutil/opt.h",
                        "libavutil/imgutils.h",
                        "libavutil/time.h",
                        "libavutil/timecode.h",
                        "libavutil/tree.h",
                        "libavutil/hwcontext.h",
                        "libavutil/hwcontext_dxva2.h",
                        "libavutil/hwcontext_d3d11va.h",
                        "libavutil/hdr_dynamic_metadata.h",
                        "libavutil/mastering_display_metadata.h",

                        // libswresample
                        "libswresample/swresample.h",

                        // libpostproc
                        "libpostproc/postprocess.h",

                        // libswscale
                        "libswscale/swscale.h",

                        // libavcodec
                        "libavcodec/avcodec.h",
                        "libavcodec/bsf.h",
                        "libavcodec/dxva2.h",
                        "libavcodec/d3d11va.h",

                        // libavformat
                        "libavformat/avformat.h",

                        // libavfilter
                        "libavfilter/avfilter.h",
                        "libavfilter/buffersrc.h",
                        "libavfilter/buffersink.h",

                        // libavdevice
                        "libavdevice/avdevice.h"
                    }
        };
        GeneratorOptions options3 = new GeneratorOptions()
        {
            RootNamespace = "WASAPI.net",
            StaticTypename = "wasapi",

            Defines = new()
            {
                "WIN32_LEAN_AND_MEAN"
            },

            IncludeDirs = new()
            {
                @"F:\Windows Kits\10\Include\10.0.19041.0\um"
            },
            Includes = new()
            {
                //wasapi
                ("windows.h", false),
                "combaseapi.h",
                "Audioclient.h",
                "mmdeviceapi.h",
                "Functiondiscoverykeys_devpkey.h",
            },

            BinaryDirs = new()
            {
                @"C:\Windows\System32"
            },
            Binaries = new()
            {
                "Ole32.dll"
            },
        };

        GeneratorOptions options4 = new GeneratorOptions()
        {
            RootNamespace = "WASAPI.net",
            StaticTypename = "wasapi",

            Defines = new()
            {
                "WIN32_LEAN_AND_MEAN"
            },

            IncludeDirs = new()
            {
                @"F:\Windows Kits\10\Include\10.0.19041.0\shared",
                @"F:\Windows Kits\10\Include\10.0.19041.0\um"
            },
            Includes = new()
            {
                                
                //wasapi
                "winerror.h",
                ("windows.h", false),
                /*"combaseapi.h",
                "Audioclient.h",
                "mmdeviceapi.h",
                "Functiondiscoverykeys_devpkey.h",*/
            },

            BinaryDirs = new()
            {
                @"C:\Windows\System32"
            },
            Binaries = new()
            {
                "Ole32.dll"
            },
        };




        [TestMethod]
        public void T1_GenerateBindings()
        {
            using (var ms = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(typeof(GeneratorOptions));
                ser.Serialize(ms, options3);

                var bytes = ms.ToArray();
                var str = Encoding.UTF8.GetString(bytes);
            }


            Generator generator = new Generator()
            {
                Options = options4
            };

            var fileCollector = new InMemoryGeneratedFileCollector();
            var logCollector = new ConsoleGenerationLogCollector();
            generator.GenerateBindings(fileCollector, logCollector);

            Console.WriteLine(fileCollector.GeneratedSourceFiles.Last().Content);
        }
    }
}