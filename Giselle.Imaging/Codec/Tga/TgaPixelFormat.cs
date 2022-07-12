using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public enum TgaPixelFormat : byte
    {
        NoImage = 0,

        Bpp8Indexed = 1,

        Bpp8Grayscale = 2,
        Bpp16AGrayscale = 3,

        Bpp24Rgb = 4,
        Bpp32Argb = 5,
    }

}
