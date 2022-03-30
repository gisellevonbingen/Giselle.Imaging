using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Bmp
{
    public enum BmpCompressionMethod : int
    {
        Rgb = 0,
        Rle8 = 1,
        Rle4 = 2,
        BitFields = 3,
        Jpeg = 4,
        Png = 5,
        AlphaBitsFields = 6,
        Cmky = 11,
        CmkyRle8 = 12,
        CmkyRle4 = 13,
    }

}
