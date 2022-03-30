using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public enum PngColorType : byte
    {
        Greyscale = 0,
        Truecolor = 2,
        IndexedColor = 3,
        WithAlphaMask = 4,

        GreyscaleWithAlpha = Greyscale | WithAlphaMask,
        TruecolorWithAlpha = Truecolor | WithAlphaMask,
    }

}
