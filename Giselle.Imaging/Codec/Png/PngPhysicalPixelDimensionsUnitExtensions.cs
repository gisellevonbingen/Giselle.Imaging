using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Physical;

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

        public static PngPhysicalPixelDimensionsUnit ToPngPhysicalPixelDimensionsUnit(this PhysicalUnit value)
        {
            if (value == PhysicalUnit.Undefined) return PngPhysicalPixelDimensionsUnit.AspectRatio;
            else if (value == PhysicalUnit.Meter) return PngPhysicalPixelDimensionsUnit.Meter;
            else return PngPhysicalPixelDimensionsUnit.AspectRatio;
        }

    }

}
