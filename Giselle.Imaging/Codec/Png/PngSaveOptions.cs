using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public class PngSaveOptions : SaveOptions
    {
        public PngPixelFormat PixelFormat { get; set; } = PngPixelFormat.Undefined;
        public CommonCompressionLevel CompressionLevel { get; set; } = CommonCompressionLevel.BestSpeed;
        public byte Interlace { get; set; } = 0;

        public PngSaveOptions()
        {

        }

        public PngSaveOptions(PngSaveOptions other) : base(other)
        {
            this.PixelFormat = other.PixelFormat;
            this.CompressionLevel = other.CompressionLevel;
            this.Interlace = other.Interlace;
        }

    }

}
