using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Bmp
{
    public static class BmpBitsPerPixelExtensions
    {
        public static BmpBitsPerPixel ToBmpBitsPerPixel(this PixelFormat value)
        {
            if (value == PixelFormat.Format1bppIndexed)
            {
                return BmpBitsPerPixel.Bpp1Indexed;
            }
            else if (value == PixelFormat.Format4bppIndexed)
            {
                return BmpBitsPerPixel.Bpp4Indexed;
            }
            else if (value == PixelFormat.Format8bppIndexed)
            {
                return BmpBitsPerPixel.Bpp8Indexed;
            }
            else if (value == PixelFormat.Format16bppRgb555)
            {
                return BmpBitsPerPixel.Bpp16Rgb555;
            }
            else if (value == PixelFormat.Format24bppRgb888)
            {
                return BmpBitsPerPixel.Bpp24Rgb;
            }
            else if (value == PixelFormat.Format32bppArgb8888)
            {
                return BmpBitsPerPixel.Bpp32Argb;
            }
            else
            {
                throw new ArgumentException($"Unknown PixelFormat : {value}");
            }

        }

        public static PixelFormat ToPixelFormat(this BmpBitsPerPixel value)
        {
            if (value == BmpBitsPerPixel.Bpp1Indexed)
            {
                return PixelFormat.Format1bppIndexed;
            }
            else if (value == BmpBitsPerPixel.Bpp4Indexed)
            {
                return PixelFormat.Format4bppIndexed;
            }
            else if (value == BmpBitsPerPixel.Bpp8Indexed)
            {
                return PixelFormat.Format8bppIndexed;
            }
            else if (value == BmpBitsPerPixel.Bpp16Rgb555)
            {
                return PixelFormat.Format16bppRgb555;
            }
            else if (value == BmpBitsPerPixel.Bpp24Rgb)
            {
                return PixelFormat.Format24bppRgb888;
            }
            else if (value == BmpBitsPerPixel.Bpp32Argb)
            {
                return PixelFormat.Format32bppArgb8888;
            }
            else
            {
                throw new ArgumentException($"Unknown BmpBitsPerPixel : {value}");
            }

        }

    }

}
