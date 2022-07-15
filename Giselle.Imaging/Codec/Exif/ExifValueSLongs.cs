using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueSLongs : ExifValueIntegers<int>
    {
        public ExifValueSLongs()
        {

        }

        public override ExifValueType Type => ExifValueType.SLong;

        public override int ReadElement(DataProcessor processor) => processor.ReadInt();

        public override void WriteElement(int element, DataProcessor processor) => processor.WriteInt(element);
    }

}
