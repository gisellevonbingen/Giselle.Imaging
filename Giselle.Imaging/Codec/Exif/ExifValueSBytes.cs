using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueSBytes : ExifValueIntegers<sbyte>
    {
        public ExifValueSBytes()
        {

        }

        public override ExifValueType Type => ExifValueType.SByte;

        protected override int CastToSigned(sbyte value) => value;

        protected override uint CastToUnsigned(sbyte value) => (uint)value;

        protected override sbyte CastToValue(int i) => (sbyte)i;

        protected override sbyte CastToValue(uint i) => (sbyte)i;

        public override sbyte ReadElement(DataProcessor processor) => processor.ReadSByte();

        public override void WriteElement(sbyte element, DataProcessor processor) => processor.WriteSByte(element);
    }

}
