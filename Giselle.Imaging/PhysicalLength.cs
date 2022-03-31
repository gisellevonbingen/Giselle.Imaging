using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public struct PhysicalLength : IPhysicalValue
    {
        public PhysicalUnit Unit { get; set; }
        public double Value { get; set; }

        public PhysicalLength(double value, PhysicalUnit unit) : this()
        {
            this.Unit = unit;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"{this.Value:F2} {this.Unit.ToDisplayString()}";
        }

    }

}
