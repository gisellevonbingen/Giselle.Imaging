using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.BMP;

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
                        var codec = new BMPCodec();
                        var image = codec.Read(fs);

                        using (var bitmap = image.ToBitmap())
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
