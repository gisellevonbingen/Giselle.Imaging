using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class PixelFormatExtensions
    {
        public static int GetBitsPerPixel(this PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.Format1bppIndexed: return 1;
                case PixelFormat.Format4bppIndexed: return 4;
                case PixelFormat.Format8bppIndexed: return 8;
                case PixelFormat.Format16bppRgb555: return 16;
                case PixelFormat.Format24bppRgb: return 24;
                case PixelFormat.Format32bppArgb: return 32;
                default: return 0;
            };

        }

    }

}
