using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public class PngSaveOptions : SaveOptions
    {
        public PngPixelFormat PixelFormat { get; set; } 
        public CommonCompressionLevel CompressionLevel { get; set; } 
        public byte Interlace { get; set; }

        public PngSaveOptions()
        {
            this.PixelFormat = PngPixelFormat.Undefined;
            this.CompressionLevel = CommonCompressionLevel.Default;
            this.Interlace = 0;
        }

        public PngSaveOptions(PngSaveOptions other) : base(other)
        {
            this.PixelFormat = other.PixelFormat;
            this.CompressionLevel = other.CompressionLevel;
            this.Interlace = other.Interlace;
        }

    }

}
