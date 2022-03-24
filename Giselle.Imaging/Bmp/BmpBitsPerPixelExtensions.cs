using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Bmp
{
    public static class BmpBitsPerPixelExtensions
    {
        public static bool IsUseColorTable(this BmpBitsPerPixel value) => value <= BmpBitsPerPixel.Indexed8;

        public static PixelFormat ToPixelFormat(this BmpBitsPerPixel value)
        {
            if (value == BmpBitsPerPixel.Indexed1)
            {
                return PixelFormat.Format1bppIndexed;
            }
            else if (value == BmpBitsPerPixel.Indexed4)
            {
                return PixelFormat.Format4bppIndexed;
            }
            else if (value == BmpBitsPerPixel.Indexed8)
            {
                return PixelFormat.Format8bppIndexed;
            }
            else if (value == BmpBitsPerPixel.Rgb24)
            {
                return PixelFormat.Format24bppRgb;
            }
            else if (value == BmpBitsPerPixel.Argb32)
            {
                return PixelFormat.Format32bppArgb;
            }
            else
            {
                return PixelFormat.Undefined;
            }

        }

    }

}
