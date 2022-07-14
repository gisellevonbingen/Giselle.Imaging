using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Imaging.Codec;
using Giselle.Imaging.Codec.Ani;
using Giselle.Imaging.Codec.ICC;
using Giselle.Imaging.Codec.Ico;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.IO;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Test
{
    public class Program
    {
        public static void Main()
        {
            TestPadding();
            TestICCProfile();
            TestCodec();
        }

        public static void TestPadding()
        {
            Action<int, int, int, int> function = (int width, int bitsPerPixel, int padding, int require) =>
            {
                var result = ScanProcessor.GetStride(width, bitsPerPixel, padding);
                Console.WriteLine($"ScanProcessor.GetStride({width}, {bitsPerPixel}, {padding}) = {result}, {result == require}");
            };

            function(17, 1, 1, 3);
            function(17, 1, 2, 4);
            function(17, 4, 4, 12);
            function(335, 1, 4, 44);
            function(335, 2, 4, 84);
            function(335, 4, 4, 168);
            function(335, 8, 4, 336);
        }

        public static void TestICCProfile()
        {
            var rootDir = @"C:\Users\Seil\Desktop\Test\ICCProfile\";
            var inputDir = $@"{rootDir}Input\";
            var outputDir = $@"{rootDir}Output\";

            Directory.CreateDirectory(rootDir);
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputDir);

            var inputPaths = Directory.GetFiles(inputDir, "*", SearchOption.AllDirectories);

            foreach (var inputPath in inputPaths)
            {
                var relatedPath = inputPath.Substring(inputDir.Length);
                var inputBytes = File.ReadAllBytes(inputPath);
                var outputPath = outputDir + relatedPath;
                new FileInfo(outputPath).Directory.Create();

                Console.WriteLine("===================================");
                Console.WriteLine(relatedPath);

                try
                {
                    using (var inputStream = new MemoryStream(inputBytes))
                    {
                        var profile = new ICCProfile(inputStream);
                        Console.WriteLine($"Input Bytes : {inputBytes.Length}");

                        using (var compactOutputStream = new MemoryStream())
                        {
                            profile.Write(compactOutputStream, new ICCProfileWriteOptions { Compact = true });
                            var outputBytes = compactOutputStream.ToArray();
                            Console.WriteLine($"Compact Output Bytes : {outputBytes.Length}");
                            Console.WriteLine($"Compact Eqauls : {inputBytes.SequenceEqual(outputBytes)}");
                        }

                        using (var nonCompactOutputStream = new MemoryStream())
                        {
                            profile.Write(nonCompactOutputStream, new ICCProfileWriteOptions { Compact = false });
                            var outputBytes = nonCompactOutputStream.ToArray();
                            Console.WriteLine($"NonCompact Output Bytes : {outputBytes.Length}");
                            Console.WriteLine($"NonCompact Eqauls : {inputBytes.SequenceEqual(outputBytes)}");
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

        }

        public static void TestCodec()
        {
            var rootDir = @"C:\Users\Seil\Desktop\Test\Codec\";
            var inputDir = $@"{rootDir}Input\";
            var outputBaseDir = $@"{rootDir}Output\";

            Directory.CreateDirectory(rootDir);
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputBaseDir);

            var inputPaths = Directory.GetFiles(inputDir, "*", SearchOption.AllDirectories);

            foreach (var inputPath in inputPaths)
            {
                var relatedPath = inputPath.Substring(inputDir.Length);
                var tempPath = outputBaseDir + relatedPath;
                var inputBytes = File.ReadAllBytes(inputPath);
                var fileName = Path.GetFileNameWithoutExtension(tempPath);
                var outputDir = Path.GetDirectoryName(tempPath);
                new DirectoryInfo(outputDir).Create();

                try
                {
                    Console.WriteLine("===================================");
                    var codec = ImageCodecs.FindCodec(inputBytes);
                    Console.WriteLine($"File Name : {relatedPath}");
                    Console.WriteLine($"Find Codec : {codec}");

                    using (var input = new BitStream(new MemoryStream(inputBytes)))
                    //using (var input = new MemoryStream(inputBytes))
                    {
                        var container = ImageCodecs.FromStream(input);
                        SaveContainerEachFrames(outputDir, fileName, container);
                        SaveContainer(inputBytes, outputDir, fileName, container);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(inputPath);
                    Console.WriteLine(ex);
                }

            }

        }

        public static void SaveContainer(byte[] inputBytes, string outputDir, string fileName, ImageArgb32Container container)
        {
            var outputFileName = $"{outputDir}/{fileName}";

            if (container.PrimaryCodec is IcoCodec icoCodec)
            {
                var ico = new IcoContainer(new MemoryStream(inputBytes));

                using (var fs = new FileStream($"{outputFileName}.{icoCodec.GetExtension(ico.Type)}", FileMode.Create))
                {
                    ico.Write(fs);
                }

            }
            else if (container.PrimaryCodec is AniCodec aniCodec)
            {
                var ani = new AniContainer(new MemoryStream(inputBytes));

                using (var fs = new FileStream($"{outputFileName}.{aniCodec.PrimaryExtension}", FileMode.Create))
                {
                    ani.Write(fs);
                }

            }
            else
            {
                SaveImageAsPrimary(outputFileName, container);
            }

        }

        public static void SaveContainerEachFrames(string outputDir, string fileName, ImageArgb32Container container)
        {
            for (int i = 0; i < container.Count; i++)
            {
                var frame = container[i];
                var logPrefix = container.Count > 1 ? $"[{i}/{container.Count}] " : string.Empty;
                var fileSuffix = container.Count > 1 ? $"_{i}" : string.Empty;
                Console.WriteLine($"{logPrefix}unique colors = " + frame.Colors.Distinct().Count());

                var outputPath = $"{outputDir}/{fileName}{fileSuffix}";
                SaveImageAsPng(outputPath, frame);
                //SaveImageAsPrimary(outputPath, frame);
            }

        }

        public static void SaveImageAsPrimary(string path, ImageArgb32Container container)
        {
            try
            {
                using (var outputStream = new FileStream(Path.ChangeExtension(path, container.PrimaryCodec.PrimaryExtension), FileMode.Create))
                {
                    container.Save(outputStream);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public static void SaveImageAsPrimary(string path, ImageArgb32Frame frame)
        {
            try
            {
                using (var outputStream = new FileStream(Path.ChangeExtension(path, frame.PrimaryCodec.PrimaryExtension), FileMode.Create))
                {
                    frame.Save(outputStream);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public static void SaveImageAsPng(string path, ImageArgb32Frame frame)
        {
            using (var outputStream = new FileStream(path + ".png", FileMode.Create))
            {
                PngCodec.Instance.Write(outputStream, frame, new PngSaveOptions() { });
            }

        }

    }

}
