using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public enum TgaPixelFormat : byte
    {
        Undefined = 0,
        NoImage = 1,

        Bpp8Indexed = 2,

        Bpp8Grayscale = 3,
        Bpp16AGrayscale = 4,

        Bpp24Rgb = 5,
        Bpp32Argb = 6,
    }

}
