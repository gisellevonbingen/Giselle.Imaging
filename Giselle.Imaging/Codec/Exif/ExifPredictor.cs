using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Exif
{
    public enum ExifPredictor : ushort
    {
        Undefined = 0,
        NoPrediction = 1,
        HorizontalDifferencing = 2,
    }

}
