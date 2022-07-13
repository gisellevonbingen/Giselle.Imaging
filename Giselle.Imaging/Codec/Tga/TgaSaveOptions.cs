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
        public ushort OriginX { get; set; } = 0;
        public ushort OriginY { get; set; } = 0;
        public bool FlipX { get; set; } = false;
        public bool FlipY { get; set; } = false;
        public TgaExtensionArea ExtensionArea { get; set; } = null;

        public TgaSaveOptions()
        {

        }

    }

}
