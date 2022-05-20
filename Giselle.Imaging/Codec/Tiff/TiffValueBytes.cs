using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueBytes : TiffValueNumbers<byte>
    {
        public TiffValueBytes()
        {

        }

        public override TiffValueType Type => TiffValueType.Byte;

        public override byte ReadElement(int raw) => (byte)raw;

        public override byte ReadElement(DataProcessor processor) => processor.ReadByte();

        public override int WriteElement(byte element) => element;

        public override void WriteElement(byte element, DataProcessor processor) => processor.WriteByte(element);
    }

}
