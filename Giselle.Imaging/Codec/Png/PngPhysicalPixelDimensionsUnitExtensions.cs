using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public static class PngPhysicalPixelDimensionsUnitExtensions
    {
        public static PhysicalUnit ToDensityUnit(this PngPhysicalPixelDimensionsUnit value)
        {
            if (value == PngPhysicalPixelDimensionsUnit.AspectRatio) return PhysicalUnit.Undefined;
            else if (value == PngPhysicalPixelDimensionsUnit.Meter) return PhysicalUnit.Meter;
            else return PhysicalUnit.Undefined;
        }

    }

}
