using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Physical
{
    public static class PhysicalUnitUtils
    {
        public static string ToDisplayString(this PhysicalUnit unit)
        {
            switch (unit)
            {
                case PhysicalUnit.Inch: return "inch";
                case PhysicalUnit.Centimeter: return "cm";
                case PhysicalUnit.Meter: return "m";
                default: return unit.ToString();
            }

        }

        public static double GetInchPerCoefficient(this PhysicalUnit unit)
        {
            switch (unit)
            {
                case PhysicalUnit.Inch: return 1.0D;
                case PhysicalUnit.Centimeter: return 25.4D;
                case PhysicalUnit.Meter: return 0.0254D;
                default: return 0.0D;
            }

        }

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
