using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Exif
{
    public enum ExifPhotometricInterpretation : ushort
    {
        WhiteIsZero = 0,
        BlackIsZero = 1,
        Rgb = 2,
        PaletteColor = 3,
        TransparencyMask = 4,
    }

}
