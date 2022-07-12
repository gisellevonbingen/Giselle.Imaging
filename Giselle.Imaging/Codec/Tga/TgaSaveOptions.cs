using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaSaveOptions : SaveOptions
    {
        public TgaPixelFormat PixelFormat { get; set; } = TgaPixelFormat.Bpp32Argb;
        public bool Compression { get; set; } = true;
        public bool FlipX { get; set; } = false;
        public bool FlipY { get; set; } = false;

        public TgaSaveOptions()
        {

        }

    }

}
