using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Bmp
{
    public class BmpEncodeOptions : EncodeOptions
    {
        public BmpBitsPerPixel BitsPerPixel { get; set; } = BmpBitsPerPixel.Undefined;
    }

}
