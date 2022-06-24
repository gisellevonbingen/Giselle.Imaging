using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public class PngSaveOptions : SaveOptions
    {
        public PngColorType? ColorType { get; set; } = null;
        public byte BitDepth { get; set; } = 0;
        public CommonCompressionLevel CompressionLevel { get; set; } = CommonCompressionLevel.BestSpeed;
        public byte Interlace { get; set; } = 0;

        public PngSaveOptions()
        {

        }

        public PngSaveOptions(PngSaveOptions other) : base(other)
        {
            this.ColorType = other.ColorType;
            this.BitDepth = other.BitDepth;
            this.CompressionLevel = other.CompressionLevel;
            this.Interlace = other.Interlace;
        }

    }

}
