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
#pragma warning disable CA1416 // 플랫폼 호환성 유효성 검사
                    return new Bitmap(frame.Width, frame.Height, frame.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, (IntPtr)scan0);
#pragma warning restore CA1416 // 플랫폼 호환성 유효성 검사
                }

            }

        }

    }

}
