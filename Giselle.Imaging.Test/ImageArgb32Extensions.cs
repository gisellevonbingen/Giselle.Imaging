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
        public static Bitmap ToBitmap(this ImageArgb32Frame frame)
        {
            unsafe
            {
                fixed (byte* scan0 = frame.Scan)
                {
                    return new Bitmap(frame.Width, frame.Height, frame.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, (IntPtr)scan0);
                }

            }

        }

    }

}
