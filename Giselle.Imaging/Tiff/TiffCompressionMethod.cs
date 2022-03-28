using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Tiff
{
    public enum TiffCompressionMethod : ushort
    {
        Undefined = 0,
        NoCompression = 1,
        CCITT = 2,
        T4Encoding = 3,
        T6Encoding = 4,
        Jpeg = 5,
        PackBits = 32773,
    }

}
