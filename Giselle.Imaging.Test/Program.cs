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
                var inputBytes = File.ReadAllBytes(inputPath);

                try
                {
                    var codec = ImageCodec.FindCodec(inputBytes);
                    var image = ImageCodec.FromBytes(inputBytes);
                    Console.WriteLine(relatedPath + " unique colors = " + image.Colors.Distinct().Count());

                    using (var outputStream = new FileStream(output, FileMode.Create))
                    {
                        codec.Write(outputStream, image, new BmpEncodeOptions() { BitsPerPixel = image.Format.ToBmpBitsPerPixel() });
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

}
