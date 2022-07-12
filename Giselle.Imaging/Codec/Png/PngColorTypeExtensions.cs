using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public static class PngColorTypeExtensions
    {
        public static (PngColorType ColorType, byte BitDepth) ToPngPixelFormat(this PixelFormat format)
        {
            if (format == PixelFormat.Format1bppIndexed) return (PngColorType.IndexedColor, 1);
            else if (format == PixelFormat.Format2bppIndexed) return (PngColorType.IndexedColor, 2);
            else if (format == PixelFormat.Format4bppIndexed) return (PngColorType.IndexedColor, 4);
            else if (format.IsColorTableLE(PixelFormat.Format8bppIndexed) == true) return (PngColorType.IndexedColor, 8);
            else if (format == PixelFormat.Format24bppRgb888) return (PngColorType.Truecolor, 8);
            else return (PngColorType.TruecolorWithAlpha, 8);

        }

        public static PixelFormat ToPixelFormat(PngColorType type, byte bitDepth)
        {
            if (type == PngColorType.IndexedColor)
            {
                if (bitDepth == 1) return PixelFormat.Format1bppIndexed;
                if (bitDepth == 2) return PixelFormat.Format2bppIndexed;
                if (bitDepth == 4) return PixelFormat.Format4bppIndexed;
                if (bitDepth == 8) return PixelFormat.Format8bppIndexed;
            }
            else if (type == PngColorType.Truecolor)
            {
                if (bitDepth == 8) return PixelFormat.Format24bppRgb888;
            }
            else if (type == PngColorType.TruecolorWithAlpha)
            {
                if (bitDepth == 8) return PixelFormat.Format32bppArgb8888;
            }

            throw new ArgumentException($"Unknown Values : type={type}, bitDepth={bitDepth}");
        }

    }

}
