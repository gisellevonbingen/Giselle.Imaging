using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Physical
{
    public interface IPhysicalValue
    {
        double Value { get; set; }
        PhysicalUnit Unit { get; set; }
        double GetConvertValue(PhysicalUnit unit);
    }

}
