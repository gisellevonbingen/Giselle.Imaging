using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Png
{
    public static class PngColorTypeExtensions
    {
        public static PixelFormat ToPixelFormat(PngColorType type, int bitDepth)
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
                return PixelFormat.Format24bppRgb888;
            }
            else if (type == PngColorType.TruecolorWithAlpha)
            {
                return PixelFormat.Format32bppArgb8888;
            }

            throw new ArgumentException($"Unknown values : type={type}, bitDepth={bitDepth}");
        }

    }

}
