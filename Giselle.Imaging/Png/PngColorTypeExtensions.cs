using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Png
{
    public static class PngColorTypeExtensions
    {
        public static int GetBitsPerPixel(PngColorType type, int bitDepth)
        {
            if (type == PngColorType.IndexedColor)
            {
                return bitDepth;
            }
            else if (type == PngColorType.Truecolor)
            {
                return bitDepth * 3;
            }
            else if (type == PngColorType.TruecolorWithAlpha)
            {
                return bitDepth * 4;
            }
            else
            {
                throw new ArgumentException($"Unknown values : type={type}, bitDepth={bitDepth}");
            }

        }

    }

}
