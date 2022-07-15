using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueShorts : ExifValueIntegers<ushort>
    {
        public ExifValueShorts()
        {

        }

        public override ExifValueType Type => ExifValueType.Short;

        public override ushort ReadElement(DataProcessor processor) => processor.ReadUShort();

        public override void WriteElement(ushort element, DataProcessor processor) => processor.WriteUShort(element);
    }

}
