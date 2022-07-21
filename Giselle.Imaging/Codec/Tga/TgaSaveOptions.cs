using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaSaveOptions : SaveOptions
    {
        public TgaPixelFormat PixelFormat { get; set; }
        public bool Compression { get; set; }
        public ushort OriginX { get; set; }
        public ushort OriginY { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public TgaExtensionArea ExtensionArea { get; set; }

        public TgaSaveOptions()
        {
            this.PixelFormat = TgaPixelFormat.Undefined;
            this.Compression = true;
            this.OriginX = 0;
            this.OriginY = 0;
            this.FlipX = false;
            this.FlipY = false;
            this.ExtensionArea = null;
        }

        public TgaSaveOptions(TgaSaveOptions other) : base(other)
        {
            this.PixelFormat = other.PixelFormat;
            this.Compression = other.Compression;
            this.OriginX = other.OriginX;
            this.OriginY = other.OriginY;
            this.FlipX = other.FlipX;
            this.FlipY = other.FlipY;
            this.ExtensionArea = other.ExtensionArea;
        }

    }

}
