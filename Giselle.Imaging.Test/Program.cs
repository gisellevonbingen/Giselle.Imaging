using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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

            var inputs = Directory.GetFiles(inputDir, "*", SearchOption.AllDirectories);

            foreach (var input in inputs)
            {
                var relatedPath = input.Substring(inputDir.Length);
                var output = Path.ChangeExtension(outputDir + relatedPath, ".png");
                new FileInfo(output).Directory.Create();

                using (var fs = new FileStream(input, FileMode.Open))
                {
                    try
                    {
                        var codec = new BmpCodec();
                        var scanData = codec.Read(fs);

                        var image = new Image32Argb(scanData.CreateProcessor());
                        Console.WriteLine(relatedPath + " unique colors = " + image.Colors.Distinct().Count());

                        using (var bitmap = image.ToBitmap())
                        //using (var bitmap = scanData.ToBitmap())
                        {
                            bitmap.Save(output, ImageFormat.Png);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(input);
                        Console.WriteLine(ex);
                    }

                }

            }

        }

    }

}
