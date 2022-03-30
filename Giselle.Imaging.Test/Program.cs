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
using System.Threading;
using System.Threading.Tasks;
using Giselle.Imaging.Bmp;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Test
{
    public class Program
    {
        public static void Main()
        {
            //TestPipe();
            TestCodec();
        }

        public static void TestPipe()
        {
            var pipe = new Pipe();
            new Thread(() =>
            {
                while (true)
                {
                    var buffer = new byte[16];
                    var len = pipe.Reader.Read(buffer, 0, buffer.Length);
                    Console.WriteLine($"Read : {len}");
                }
            }).Start();
            new Thread(() =>
            {
                while (true)
                {
                    var data = new byte[Console.ReadLine().Length];
                    pipe.Writer.Write(data, 0, data.Length);
                }
            }).Start();
        }

        public static void TestCodec()
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
                        //SaveImageAsReadCodec(outputStream, image, codec, new BmpEncodeOptions() { BitsPerPixel = image.PixelFormat.ToBmpBitsPerPixel() });
                        SaveImageAsBitmap(outputStream, image);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(inputPath);
                    Console.WriteLine(ex);
                }

            }

        }

        public static void SaveImageAsBitmap(FileStream outputStream, ImageArgb32 image)
        {
            image.ToBitmap().Save(outputStream, ImageFormat.Png);
        }

        public static void SaveImageAsReadCodec(Stream output, ImageArgb32 image, IImageCodec codec, EncodeOptions options)
        {
            codec.Write(output, image, options);
        }

    }

}
