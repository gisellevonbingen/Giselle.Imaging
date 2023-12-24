using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Gif
{
    public enum GifBlockCode : byte
    {
        ExtensionIntroducer = 0x21,
        ImageDescriptor = 0x2C,
        Trailer = 0x3B,

        GraphicControlExtension = 0xF9,
        Comment = 0xFE,
        ApplicationExtension = 0xFF,
    }

}
