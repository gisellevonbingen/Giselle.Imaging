using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Tiff
{
    public enum TiffPredictor : ushort
    {
        Undefined = 0,
        NoPrediction = 1,
        HorizontalDifferencing = 2,
    }

}
