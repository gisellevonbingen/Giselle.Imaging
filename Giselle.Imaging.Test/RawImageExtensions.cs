using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Test
{
    public static class RawImageExtensions
    {
        public static Bitmap ToBitmap(this RawImage image)
        {
            unsafe
            {
                fixed (byte* scan0 = image.Scan)
                {
                    return new Bitmap(image.Width, image.Height, image.Stride, image.Format, (IntPtr)scan0);
                }

            }

        }

    }

}
