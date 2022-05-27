using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueRationals : TiffValueArray<TiffRational>
    {
        public TiffValueRationals()
        {

        }

        public override TiffValueType Type => TiffValueType.Rational;

        public override TiffRational ReadElement(int raw) => throw new NotSupportedException();

        public override TiffRational ReadElement(DataProcessor processor) => new TiffRational(processor);

        public override int WriteElement(TiffRational element) => throw new NotSupportedException();

        public override void WriteElement(TiffRational element, DataProcessor processor) => element.Write(processor);
    }

}
