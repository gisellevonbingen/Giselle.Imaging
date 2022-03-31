using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class PixelFormatExtensions
    {
        public static PixelFormat GetPrefferedIndexedFormat(this int colorCount, PixelFormat fallback = PixelFormat.Undefined)
        {
            var lastCount = 0;
            var lastPixelFormat = fallback;

            foreach (var e in Enum.GetValues(typeof(PixelFormat)) as PixelFormat[])
            {
                var colorTableLength = e.GetColorTableLength();

                if (colorTableLength > 0 && colorTableLength >= colorCount)
                {
                    if (lastCount == 0 || colorTableLength < lastCount)
                    {
                        lastPixelFormat = e;
                        lastCount = colorTableLength;
                    }

                }

            }

            return lastPixelFormat;
        }

        public static bool IsUseColorTable(this PixelFormat value) => GetColorTableLength(value) > 0;

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

        public static int GetBitsPerPixel(this PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.Format1bppIndexed: return 1;
                case PixelFormat.Format2bppIndexed: return 2;
                case PixelFormat.Format4bppIndexed: return 4;
                case PixelFormat.Format8bppIndexed: return 8;

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
