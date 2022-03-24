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
            var bitmap = new Bitmap(image.Width, image.Height);
            var data = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.Format);
            data.Stride = image.Stride;

            unsafe
            {
                fixed (byte* scan0 = image.Scan)
                {
                    data.Scan0 = (IntPtr)scan0;
                }

            }

            bitmap.UnlockBits(data);
            return bitmap;
        }

    }

}
