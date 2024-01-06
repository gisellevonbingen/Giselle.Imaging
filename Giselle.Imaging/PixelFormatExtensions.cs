using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class PixelFormatExtensions
    {
        public static bool HasAlphaChannel(this PixelFormat value) => value switch
        {
            PixelFormat.Format16bppAGrayscale => true,
            _ => value.IsArgb()
        };

        public static bool IsGrayscale(this PixelFormat value) => value switch
        {
            PixelFormat.Format8bppGrayscale => true,
            PixelFormat.Format16bppAGrayscale => true,
            _ => false
        };

        public static bool IsArgb(this PixelFormat value) => value switch
        {
            PixelFormat.Format16bppArgb1555 => true,
            PixelFormat.Format32bppArgb8888 => true,
            _ => false
        };

        public static bool IsRgb(this PixelFormat value) => value switch
        {
            PixelFormat.Format16bppRgb555 or PixelFormat.Format16bppRgb565 or PixelFormat.Format16bppArgb1555 => true,
            PixelFormat.Format24bppRgb888 => true,
            PixelFormat.Format32bppRgb888 or PixelFormat.Format32bppArgb8888 => true,
            _ => false
        };

        public static bool IsIndexed(this PixelFormat value) => GetColorTableLength(value) > 0;

        public static int GetColorTableLength(this PixelFormat value) => value switch
        {
            PixelFormat.Format1bppIndexed => 2,
            PixelFormat.Format2bppIndexed => 4,
            PixelFormat.Format4bppIndexed => 16,
            PixelFormat.Format8bppIndexed => 256,
            _ => 0,
        };

        public static bool IsColorTableLE(this PixelFormat own, PixelFormat with) => own.GetColorTableLength() <= with.GetColorTableLength();

        public static int GetBitsPerPixel(this PixelFormat value) => value switch
        {
            PixelFormat.Format1bppIndexed => 1,
            PixelFormat.Format2bppIndexed => 2,
            PixelFormat.Format4bppIndexed => 4,
            PixelFormat.Format8bppIndexed => 8,
            PixelFormat.Format8bppGrayscale => 8,
            PixelFormat.Format16bppAGrayscale => 16,
            PixelFormat.Format16bppRgb555 => 16,
            PixelFormat.Format16bppRgb565 => 16,
            PixelFormat.Format16bppArgb1555 => 16,
            PixelFormat.Format24bppRgb888 => 24,
            PixelFormat.Format32bppRgb888 => 32,
            PixelFormat.Format32bppArgb8888 => 32,
            _ => 0,
        };

        public static Argb32[] GetColorTable(this PixelFormat value, IEnumerable<Argb32> colors)
        {
            var colorTableLength = value.GetColorTableLength();

            if (colorTableLength > 0)
            {
                var usedColors = colors.ToHashSet();

                if (usedColors.Count > colorTableLength)
                {
                    throw new ArgumentException($"Image's Used Colors Kind({usedColors.Count}) Exceeds ColorTableLength({colorTableLength})");
                }

                return usedColors.ToArray();
            }
            else
            {
                return Array.Empty<Argb32>();
            }

        }

    }

}
