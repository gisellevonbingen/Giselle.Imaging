using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaSaveOptions : SaveOptions
    {
        public byte PixelDepth { get; set; } = 32;
        public TgaImageType ImageType { get; set; } = TgaImageType.RunLengthEncodedTrueColor;

        public TgaSaveOptions()
        {

        }

    }

}
