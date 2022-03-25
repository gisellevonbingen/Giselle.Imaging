using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Bmp
{
    public enum BmpBitsPerPixel : byte
    {
        Undefined = 0,
        Bpp1Indexed = 1,
        Bpp4Indexed = 4,
        Bpp8Indexed = 8,
        Bpp16Rgb555 = 16,
        Bpp24Rgb = 24,
        Bpp32Argb = 32,
    }

}
