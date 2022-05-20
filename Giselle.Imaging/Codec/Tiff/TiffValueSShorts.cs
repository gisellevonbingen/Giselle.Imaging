using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueSShorts : TiffValueIntegers<short>
    {
        public TiffValueSShorts()
        {

        }

        public override TiffValueType Type => TiffValueType.SShort;

        public override short ReadElement(int raw) => (short)raw;

        public override short ReadElement(DataProcessor processor) => processor.ReadShort();

        public override int WriteElement(short element) => element;

        public override void WriteElement(short element, DataProcessor processor) => processor.WriteShort(element);
    }

}
