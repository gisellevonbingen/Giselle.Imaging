using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tiff
{
    public enum TiffPhotometricInterpretation : ushort
    {
        WhiteIsZero = 0,
        BlackIsZero = 1,
        Rgb = 2,
        PaletteColor = 3,
        TransparencyMask = 4,
    }

}
