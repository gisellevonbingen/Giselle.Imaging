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

        protected override int CastToSigned(ushort value) => value;

        protected override uint CastToUnsigned(ushort value) => value;

        protected override ushort CastToValue(int i) => (ushort)i;

        protected override ushort CastToValue(uint i) => (ushort)i;

        public override ushort ReadElement(DataProcessor processor) => processor.ReadUShort();

        public override void WriteElement(ushort element, DataProcessor processor) => processor.WriteUShort(element);
    }

}
