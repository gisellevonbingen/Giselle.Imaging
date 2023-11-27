using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Physical
{
    public static class PhysicalUnitUtils
    {
        public static string ToDisplayString(this PhysicalUnit unit) => unit switch
        {
            PhysicalUnit.Inch => "inch",
            PhysicalUnit.Centimeter => "cm",
            PhysicalUnit.Meter => "m",
            _ => unit.ToString(),
        };

        public static double GetValuesPerInch(this PhysicalUnit unit) => unit switch
        {
            PhysicalUnit.Inch => 1.0D,
            PhysicalUnit.Centimeter => 2.54D,
            PhysicalUnit.Meter => 0.0254D,
            _ => 0.0D,
        };

        public static PhysicalUnit NormalizeUnits(params PhysicalUnit[] units)
        {
            if (Array.IndexOf(units, PhysicalUnit.Inch) > -1)
            {
                return PhysicalUnit.Inch;
            }
            else if (Array.IndexOf(units, PhysicalUnit.Centimeter) > -1)
            {
                return PhysicalUnit.Centimeter;
            }
            else if (Array.IndexOf(units, PhysicalUnit.Meter) > -1)
            {
                return PhysicalUnit.Meter;
            }
            else
            {
                return units.FirstOrDefault();
            }

        }

    }

}
