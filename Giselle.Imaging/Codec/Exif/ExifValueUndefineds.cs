using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueUndefineds : ExifValueArray<byte>
    {
        public ExifValueUndefineds()
        {

        }

        public override ExifValueType Type => ExifValueType.Rational;

        public override byte ReadElement(int raw) => throw new NotSupportedException();

        public override byte ReadElement(DataProcessor processor) => processor.ReadByte();

        public override int WriteElement(byte element) => throw new NotSupportedException();

        public override void WriteElement(byte element, DataProcessor processor) { processor.WriteByte(element); }
    }

}
