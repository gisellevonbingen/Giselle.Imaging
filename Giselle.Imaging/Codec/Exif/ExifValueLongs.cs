using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueLongs : ExifValueIntegers<uint>
    {
        public ExifValueLongs()
        {

        }

        public override ExifValueType Type => ExifValueType.Long;

        public override uint ReadElement(DataProcessor processor) => processor.ReadUInt();

        public override void WriteElement(uint element, DataProcessor processor) => processor.WriteUInt(element);

    }

}
