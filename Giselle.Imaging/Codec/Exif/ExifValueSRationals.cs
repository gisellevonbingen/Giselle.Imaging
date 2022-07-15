using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueSRationals : ExifValueArray<ExifSRational>
    {
        public ExifValueSRationals()
        {

        }

        public override ExifValueType Type => ExifValueType.Rational;

        public override ExifSRational ReadElement(DataProcessor processor) => new ExifSRational(processor);

        public override void WriteElement(ExifSRational element, DataProcessor processor) => element.Write(processor);
    }

}
