using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public enum PngPixelFormat : byte
    {
        Undefined = 0,

        Greyscale = 1,
        GreyscaleWithAlpha = 2,

        Bpp1Indexed = 3,
        Bpp2Indexed = 4,
        Bpp4Indexed = 5,
        Bpp8Indexed = 6,

        Bpp24Truecolor = 7,
        Bpp32TruecolorWithAlpha = 8,
    }

}

