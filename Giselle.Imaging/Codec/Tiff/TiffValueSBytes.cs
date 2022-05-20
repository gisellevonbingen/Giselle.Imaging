using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueSBytes : TiffValueIntegers<sbyte>
    {
        public TiffValueSBytes()
        {

        }

        public override TiffValueType Type => TiffValueType.SByte;

        public override sbyte ReadElement(int raw) => (sbyte)raw;

        public override sbyte ReadElement(DataProcessor processor) => processor.ReadSByte();

        public override int WriteElement(sbyte element) => element;

        public override void WriteElement(sbyte element, DataProcessor processor) => processor.WriteSByte(element);
    }

}
