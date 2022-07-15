using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public static class PngPixelFormatExtensions
    {
        public static (PngColorType ColorType, byte BitDepth) ToPngColorType(this PngPixelFormat format)
        {
            if (format == PngPixelFormat.Bpp1Indexed) return (PngColorType.IndexedColor, 1);
            else if (format == PngPixelFormat.Bpp2Indexed) return (PngColorType.IndexedColor, 2);
            else if (format == PngPixelFormat.Bpp4Indexed) return (PngColorType.IndexedColor, 4);
            else if (format == PngPixelFormat.Bpp8Indexed) return (PngColorType.IndexedColor, 8);
            else if (format == PngPixelFormat.Bpp24Truecolor) return (PngColorType.Truecolor, 8);
            else if (format == PngPixelFormat.Bpp32TruecolorWithAlpha) return (PngColorType.TruecolorWithAlpha, 8);
            else return (PngColorType.TruecolorWithAlpha, 8);
        }

        public static PngPixelFormat ToPngPixelFormat(PngColorType type, byte bitDepth)
        {
            if (type == PngColorType.IndexedColor)
            {
                if (bitDepth == 1) return PngPixelFormat.Bpp1Indexed;
                else if (bitDepth == 2) return PngPixelFormat.Bpp2Indexed;
                else if (bitDepth == 4) return PngPixelFormat.Bpp4Indexed;
                else if (bitDepth == 8) return PngPixelFormat.Bpp8Indexed;
            }
            else if (type == PngColorType.Truecolor)
            {
                if (bitDepth == 8) return PngPixelFormat.Bpp24Truecolor;
            }
            else if (type == PngColorType.TruecolorWithAlpha)
            {
                if (bitDepth == 8) return PngPixelFormat.Bpp32TruecolorWithAlpha;
            }

            throw new ArgumentException($"Unknown Values : type={type}, bitDepth={bitDepth}");
        }

        public static PngPixelFormat ToPngPixelFormat(this PixelFormat value)
        {
            if (value == PixelFormat.Format1bppIndexed)
            {
                return PngPixelFormat.Bpp1Indexed;
            }
            else if (value == PixelFormat.Format2bppIndexed)
            {
                return PngPixelFormat.Bpp2Indexed;
            }
            else if (value == PixelFormat.Format4bppIndexed)
            {
                return PngPixelFormat.Bpp4Indexed;
            }
            else if (value == PixelFormat.Format8bppIndexed)
            {
                return PngPixelFormat.Bpp8Indexed;
            }
            else if (value == PixelFormat.Format24bppRgb888)
            {
                return PngPixelFormat.Bpp24Truecolor;
            }
            else if (value == PixelFormat.Format32bppArgb8888)
            {
                return PngPixelFormat.Bpp32TruecolorWithAlpha;
            }
            else
            {
                throw new ArgumentException($"Unknown PixelFormat : {value}");
            }

        }

        public static PixelFormat ToPixelFormat(this PngPixelFormat value)
        {
            if (value == PngPixelFormat.Bpp1Indexed)
            {
                return PixelFormat.Format1bppIndexed;
            }
            if (value == PngPixelFormat.Bpp2Indexed)
            {
                return PixelFormat.Format2bppIndexed;
            }
            else if (value == PngPixelFormat.Bpp4Indexed)
            {
                return PixelFormat.Format4bppIndexed;
            }
            else if (value == PngPixelFormat.Bpp8Indexed)
            {
                return PixelFormat.Format8bppIndexed;
            }
            else if (value == PngPixelFormat.Bpp4Indexed)
            {
                return PixelFormat.Format4bppIndexed;
            }
            else if (value == PngPixelFormat.Bpp24Truecolor)
            {
                return PixelFormat.Format24bppRgb888;
            }
            else if (value == PngPixelFormat.Bpp32TruecolorWithAlpha)
            {
                return PixelFormat.Format32bppArgb8888;
            }
            else
            {
                throw new ArgumentException($"Unknown PngPixelFormat : {value}");
            }

        }

    }

}
