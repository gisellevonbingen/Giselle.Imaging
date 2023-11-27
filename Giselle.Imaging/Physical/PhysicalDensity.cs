using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Physical
{
    public struct PhysicalDensity : IPhysicalValue
    {
        public PhysicalUnit Unit { get; set; }
        public double Value { get; set; }

        public PhysicalDensity(double value, PhysicalUnit unit) : this()
        {
            this.Unit = unit;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"{this.Value:F2} pixels/{this.Unit.ToDisplayString()}";
        }

        public double GetConvertValue(PhysicalUnit unit) => PhysicalValueUtils.ConvertDenominator(this, unit);
    }

}
