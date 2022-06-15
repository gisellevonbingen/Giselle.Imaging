using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Exif
{
    public static class ExifValueExtensions
    {
        public static ExifValueType ToExifEntryType(this short value) => ExifValueType.FromId(value);

        public static T CastValueType<T>(ExifValue type, string name) where T : IExifValue
        {
            if (type is T cast)
            {
                return cast;
            }
            else
            {
                throw new IOException($"ValueType({type}) is not ${name}");
            }

        }

        public static ExifValueASCII AsASCII(this ExifValue value) => CastValueType<ExifValueASCII>(value, "ASCII");

        public static ExifValueRationals AsRtaionals(this ExifValue value) => CastValueType<ExifValueRationals>(value, "Rationals");

        public static IExifValueIntegers AsNumbers(this ExifValue value) => CastValueType<IExifValueIntegers>(value, "Integers");

    }

}
