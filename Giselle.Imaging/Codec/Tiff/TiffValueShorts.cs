using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueShorts : TiffValueIntegers<ushort>
    {
        public TiffValueShorts()
        {

        }

        public override TiffValueType Type => TiffValueType.Short;

        public override ushort ReadElement(int raw) => (ushort)raw;

        public override ushort ReadElement(DataProcessor processor) => processor.ReadUShort();

        public override int WriteElement(ushort element) => element;

        public override void WriteElement(ushort element, DataProcessor processor) => processor.WriteUShort(element);
    }

}
