using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Bmp
{
    public class BmpSaveOptions : SaveOptions
    {
        public BmpBitsPerPixel BitsPerPixel { get; set; } = BmpBitsPerPixel.Undefined;
        public BmpCompressionMethod? Compression { get; set; } = null;
    }

}
