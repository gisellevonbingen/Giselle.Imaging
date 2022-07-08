using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tiff
{
    public enum TiffCompressionMethod : ushort
    {
        Undefined = 0,
        NoCompression = 1,
        ModifiedHuffman = 2,
        T4Encoding = 3,
        T6Encoding = 4,
        LZW = 5,
        JPEG_Old = 6,
        JPEG_NEW = 7,
        Deflate = 8,
        PackBits = 32773,
    }

}
