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

        Bpp8Grayscale = 1,
        Bpp16AGrayscale = 2,

        Bpp24Rgb = 3,
        Bpp32Argb = 4,
    }

}
