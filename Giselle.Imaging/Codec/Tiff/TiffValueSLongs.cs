using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueSLongs : TiffValueIntegers<int>
    {
        public TiffValueSLongs()
        {

        }

        public override TiffValueType Type => TiffValueType.SLong;

        public override int ReadElement(int raw) => raw;

        public override int ReadElement(DataProcessor processor) => processor.ReadInt();

        public override int WriteElement(int element) => element;

        public override void WriteElement(int element, DataProcessor processor) => processor.WriteInt(element);
    }

}
