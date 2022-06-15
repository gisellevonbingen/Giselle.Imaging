using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueRationals : ExifValueArray<ExifRational>
    {
        public ExifValueRationals()
        {

        }

        public override ExifValueType Type => ExifValueType.Rational;

        public override ExifRational ReadElement(int raw) => throw new NotSupportedException();

        public override ExifRational ReadElement(DataProcessor processor) => new ExifRational(processor);

        public override int WriteElement(ExifRational element) => throw new NotSupportedException();

        public override void WriteElement(ExifRational element, DataProcessor processor) => element.Write(processor);
    }

}
