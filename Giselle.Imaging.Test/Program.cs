using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Bmp;

namespace Giselle.Imaging.Test
{
    public class Program
    {
        public static void Main()
        {
            var rootDir = @"C:\Users\Seil\Desktop\Test\";
            var inputDir = $@"{rootDir}Input\";
            var outputDir = $@"{rootDir}Output\";

            Directory.CreateDirectory(rootDir);
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputDir);

            var inputPaths = Directory.GetFiles(inputDir, "*", SearchOption.AllDirectories);

            foreach (var inputPath in inputPaths)
            {
                var relatedPath = inputPath.Substring(inputDir.Length);
                var output = outputDir + relatedPath;
                new FileInfo(output).Directory.Create();

                using (var inputStream = new FileStream(inputPath, FileMode.Open))
                {
                    try
                    {
                        var codec = new BmpCodec();
                        var scanData = codec.Read(inputStream);

                        var image = new Image32Argb(scanData);
                        Console.WriteLine(relatedPath + " unique colors = " + image.Colors.Distinct().Count());

                        using (var outputStream = new FileStream(output, FileMode.Create))
                        {
                            //SaveScanDataAsBitmap(outputStream, scanData);
                            //SaveScanDataAsCodec(outputStream, scanData, codec);
                            SaveImageAsCodec(outputStream, image, codec, new BmpEncodeOptions() { BitsPerPixel = scanData.Format.ToBmpBitsPerPixel() });
                            //SaveImageAsCodec(outputStream, image, codec, new BmpEncodeOptions() {  });
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(inputPath);
                        Console.WriteLine(ex);
                    }

                }

            }

        }

        public static void SaveScanDataAsBitmap(Stream output, ScanData scanData)
        {
            using (var bitmap = scanData.ToBitmap())
            {
                bitmap.Save(output, ImageFormat.Png);
            }

        }

        public static void SaveScanDataAsCodec(Stream output, ScanData scanData, IImageCodec codec)
        {
            codec.Write(output, scanData);
        }

        public static void SaveImageAsCodec<T>(Stream output, Image32Argb image, IImageCodec<T> codec, T options) where T : EncodeOptions, new()
        {
            var scanData = codec.Encode(image, options);
            SaveScanDataAsCodec(output, scanData, codec);
        }

    }


}
