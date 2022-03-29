using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Test
{
    public static class ImageArgb32Extensions
    {
        public static Bitmap ToBitmap(this ImageArgb32 image)
        {
            unsafe
            {
                fixed (byte* scan0 = image.Scan)
                {
                    return new Bitmap(image.Width, image.Height, image.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, (IntPtr)scan0);
                }

            }

        }

    }

}
