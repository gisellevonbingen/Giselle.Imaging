using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.ICC
{
    [Flags]
    public enum ICCProfileFlags : int
    {
        Embeded = 1,
        CannotBeUsedIndependentlyOfTheEmbeddedColourData = 2,
    }

}
