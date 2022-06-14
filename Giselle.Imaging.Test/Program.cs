﻿using System;
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
using Giselle.Imaging.Codec;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.Codec.ICC;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.Codec.Tiff;
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
            //TestPipe();
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
            var rootDir = @"C:\Users\Seil\Desktop\Test\Codec\";
            var inputDir = $@"{rootDir}Input\";
            var outputDir = $@"{rootDir}Output\";

            Directory.CreateDirectory(rootDir);
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputDir);

            var inputPaths = Directory.GetFiles(inputDir, "*", SearchOption.AllDirectories);

            foreach (var inputPath in inputPaths)
            {
                var relatedPath = inputPath.Substring(inputDir.Length);
                var outputPath = outputDir + relatedPath;
                new FileInfo(outputPath).Directory.Create();
                var inputBytes = File.ReadAllBytes(inputPath);

                try
                {
                    Console.WriteLine("===================================");
                    Console.WriteLine(relatedPath);
                    var codec = ImageCodecs.FindCodec(inputBytes);
                    var raw = ImageCodecs.FromBytes(inputBytes);

                    var image = raw.Decode();
                    Console.WriteLine("unique colors = " + image.Colors.Distinct().Count());

                    using (var outputStream = new FileStream(outputPath, FileMode.Create))
                    {
                        //SaveImageAsReadCodec(outputStream, image, codec, new PngEncodeOptions() { Interlace = false });
                        //SaveImageAsReadCodec(outputStream, image, codec, new BmpEncodeOptions() { });
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
            var raw = codec.CreateEmpty();
            raw.Encode(image, options);

            codec.Write(output, raw);
        }

    }

}
