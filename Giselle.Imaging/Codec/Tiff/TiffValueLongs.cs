using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueLongs : TiffValueIntegers<uint>
    {
        public TiffValueLongs()
        {

        }

        public override TiffValueType Type => TiffValueType.Long;

        public override uint ReadElement(int raw) => (uint)raw;

        public override uint ReadElement(DataProcessor processor) => processor.ReadUInt();

        public override int WriteElement(uint element) => (int)element;

        public override void WriteElement(uint element, DataProcessor processor) => processor.WriteUInt(element);

    }

}
