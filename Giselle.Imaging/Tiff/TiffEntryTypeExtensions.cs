using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Tiff
{
    public static class TiffEntryTypeExtensions
    {
        public static TiffValueType ToTiffEntryType(this short value) => TiffValueType.FromId(value);
    }

}
