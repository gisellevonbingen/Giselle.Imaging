using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formats.Exif;
using Giselle.Imaging.Physical;

namespace Giselle.Imaging.Formats.Exif
{
    public static class ExifResolutionUnitExtensions
    {
        public static PhysicalUnit ToPhysicalUnit(this ExifResolutionUnit value)
        {
            if (value == ExifResolutionUnit.Undefined || value == ExifResolutionUnit.NoAbsolute)
            {
                return PhysicalUnit.Undefined;
            }
            else if (value == ExifResolutionUnit.Inch)
            {
                return PhysicalUnit.Inch;
            }
            else if (value == ExifResolutionUnit.Centimeter)
            {
                return PhysicalUnit.Centimeter;
            }
            else
            {
                return PhysicalUnit.Undefined;
            }

        }

        public static ExifResolutionUnit ToExifResolutionUnit(this PhysicalUnit value)
        {
            if (value == PhysicalUnit.Undefined)
            {
                return ExifResolutionUnit.NoAbsolute;
            }
            else if (value == PhysicalUnit.Inch)
            {
                return ExifResolutionUnit.Inch;
            }
            else if (value == PhysicalUnit.Centimeter)
            {
                return ExifResolutionUnit.Centimeter;
            }
            else
            {
                return ExifResolutionUnit.Undefined;
            }

        }

    }

}
