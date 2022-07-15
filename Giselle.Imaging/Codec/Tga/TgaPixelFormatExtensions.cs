using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public static class TgaPixelFormatExtensions
    {
        public static (TgaImageType Type, byte PixelDepth, byte AlphaBits) ToTgaImageType(this TgaPixelFormat value)
        {
            if (value == TgaPixelFormat.NoImage)
            {
                return (TgaImageType.NoImage, 0, 0);
            }
            else if (value == TgaPixelFormat.Bpp8Indexed)
            {
                return (TgaImageType.ColorMapped, 8, 0);
            }
            else if (value == TgaPixelFormat.Bpp8Grayscale)
            {
                return (TgaImageType.Grayscale, 8, 0);
            }
            else if (value == TgaPixelFormat.Bpp16AGrayscale)
            {
                return (TgaImageType.Grayscale, 16, 8);
            }
            else if (value == TgaPixelFormat.Bpp24Rgb)
            {
                return (TgaImageType.TrueColor, 24, 0);
            }
            else if (value == TgaPixelFormat.Bpp32Argb)
            {
                return (TgaImageType.TrueColor, 32, 8);
            }

            throw new ArgumentException($"Unknown TgaPixelFormat : {value}");
        }

        public static TgaPixelFormat ToTgaPixelFormat(this TgaImageType type, byte pixelDepth, byte alphaBits)
        {
            if (type == TgaImageType.NoImage)
            {
                return TgaPixelFormat.NoImage;
            }
            else if (type == TgaImageType.ColorMapped)
            {
                if (pixelDepth == 8 && alphaBits == 0) return TgaPixelFormat.Bpp8Indexed;
            }
            else if (type == TgaImageType.Grayscale)
            {
                if (pixelDepth == 8 && alphaBits == 0) return TgaPixelFormat.Bpp8Grayscale;
                else if (pixelDepth == 16 && alphaBits == 8) return TgaPixelFormat.Bpp16AGrayscale;
            }
            else if (type == TgaImageType.TrueColor)
            {
                if (pixelDepth == 24 && alphaBits == 0) return TgaPixelFormat.Bpp24Rgb;
                else if (pixelDepth == 32 && alphaBits == 8) return TgaPixelFormat.Bpp32Argb;
            }

            throw new ArgumentException($"Unknown ImageType, PixelDepth, AlphaBits : {type}, {pixelDepth}, {alphaBits}");
        }

        public static TgaPixelFormat ToTgaPixelFormat(this PixelFormat value)
        {
            if (value == PixelFormat.Format8bppIndexed)
            {
                return TgaPixelFormat.Bpp8Indexed;
            }
            else if (value == PixelFormat.Format8bppGrayscale)
            {
                return TgaPixelFormat.Bpp8Grayscale;
            }
            else if (value == PixelFormat.Format16bppAGrayscale)
            {
                return TgaPixelFormat.Bpp16AGrayscale;
            }
            else if (value == PixelFormat.Format24bppRgb888)
            {
                return TgaPixelFormat.Bpp24Rgb;
            }
            else if (value == PixelFormat.Format32bppArgb8888)
            {
                return TgaPixelFormat.Bpp32Argb;
            }
            else
            {
                throw new ArgumentException($"Unknown PixelFormat : {value}");
            }

        }

        public static PixelFormat ToPixelFormat(this TgaPixelFormat value)
        {
            if (value == TgaPixelFormat.Bpp8Indexed)
            {
                return PixelFormat.Format8bppIndexed;
            }
            if (value == TgaPixelFormat.Bpp8Grayscale)
            {
                return PixelFormat.Format8bppGrayscale;
            }
            else if (value == TgaPixelFormat.Bpp16AGrayscale)
            {
                return PixelFormat.Format16bppAGrayscale;
            }
            else if (value == TgaPixelFormat.Bpp24Rgb)
            {
                return PixelFormat.Format24bppRgb888;
            }
            else if (value == TgaPixelFormat.Bpp32Argb)
            {
                return PixelFormat.Format32bppArgb8888;
            }
            else
            {
                throw new ArgumentException($"Unknown TgaPixelFormat : {value}");
            }

        }

    }

}
