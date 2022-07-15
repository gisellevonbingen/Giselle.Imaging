using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class PixelFormatExtensions
    {
        public static bool HasAlphaChannel(this PixelFormat value)
        {
            if (value == PixelFormat.Format16bppAGrayscale)
            {
                return true;
            }
            else
            {
                return IsArgb(value);
            }

        }

        public static bool IsGrayscale(this PixelFormat value)
        {
            if (value == PixelFormat.Format8bppGrayscale)
            {
                return true;
            }
            else if (value == PixelFormat.Format16bppAGrayscale)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool IsArgb(this PixelFormat value)
        {
            if (value == PixelFormat.Format16bppArgb1555)
            {
                return true;
            }
            else if (value == PixelFormat.Format32bppArgb8888)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool IsRgb(this PixelFormat value)
        {
            if (value == PixelFormat.Format16bppRgb555 || value == PixelFormat.Format16bppRgb565 || value == PixelFormat.Format16bppArgb1555)
            {
                return true;
            }
            else if (value == PixelFormat.Format24bppRgb888)
            {
                return true;
            }
            else if (value == PixelFormat.Format32bppRgb888 || value == PixelFormat.Format32bppArgb8888)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool IsIndexed(this PixelFormat value) => GetColorTableLength(value) > 0;

        public static int GetColorTableLength(this PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.Format1bppIndexed: return 2;
                case PixelFormat.Format2bppIndexed: return 4;
                case PixelFormat.Format4bppIndexed: return 16;
                case PixelFormat.Format8bppIndexed: return 256;
                default: return 0;
            };

        }

        public static bool IsColorTableLE(this PixelFormat own, PixelFormat with)
        {
            return own.GetColorTableLength() <= with.GetColorTableLength();
        }

        public static int GetBitsPerPixel(this PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.Format1bppIndexed: return 1;
                case PixelFormat.Format2bppIndexed: return 2;
                case PixelFormat.Format4bppIndexed: return 4;
                case PixelFormat.Format8bppIndexed: return 8;

                case PixelFormat.Format8bppGrayscale: return 8;
                case PixelFormat.Format16bppAGrayscale: return 16;

                case PixelFormat.Format16bppRgb555: return 16;
                case PixelFormat.Format16bppRgb565: return 16;
                case PixelFormat.Format16bppArgb1555: return 16;

                case PixelFormat.Format24bppRgb888: return 24;

                case PixelFormat.Format32bppRgb888: return 32;
                case PixelFormat.Format32bppArgb8888: return 32;

                default: return 0;
            };

        }

    }

}
