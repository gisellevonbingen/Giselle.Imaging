using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public class PngEncodeOptions : EncodeOptions
    {
        public PngColorType? ColorType { get; set; } = null;
        public byte BitDepth { get; set; } = 0;
        public CommonCompressionLevel CompressionLevel { get; set; } = CommonCompressionLevel.BestSpeed;
        public bool Interlace { get; set; } = false;
    }

}
