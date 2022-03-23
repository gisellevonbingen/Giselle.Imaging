using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.BMP;

namespace Giselle.Imaging.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dir = @"C:\Users\Seil\Desktop\";
            var input = $"{dir}Test.bmp";
            var output = $"{dir}Result.png";

            using (var fs = new FileStream(input, FileMode.Open))
            {
                var codec = new BMPCodec();
                var image = codec.Read(fs);
                Console.WriteLine(image.Width + " " + image.Height);

                using (var bitmap = new Bitmap(image.Width, image.Height))
                {
                    var data = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

                    unsafe
                    {
                        fixed (byte* scan0 = image.Scan)
                        {
                            data.Scan0 = (IntPtr)scan0;
                        }

                    }

                    bitmap.UnlockBits(data);
                    bitmap.Save(output, ImageFormat.Png);
                    Console.WriteLine("Complete");
                }

            }

        }

    }

}
