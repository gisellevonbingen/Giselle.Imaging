using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueSShorts : ExifValueIntegers<short>
    {
        public ExifValueSShorts()
        {

        }

        public override ExifValueType Type => ExifValueType.SShort;

        public override short ReadElement(DataProcessor processor) => processor.ReadShort();

        public override void WriteElement(short element, DataProcessor processor) => processor.WriteShort(element);
    }

}
