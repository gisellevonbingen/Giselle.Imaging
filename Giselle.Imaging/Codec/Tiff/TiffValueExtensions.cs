using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tiff
{
    public static class TiffValueExtensions
    {
        public static TiffValueType ToTiffEntryType(this short value) => TiffValueType.FromId(value);

        public static T CastValueType<T>(TiffValue type, string name) where T : ITiffValue
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

        public static TiffValueASCII AsASCII(this TiffValue value) => CastValueType<TiffValueASCII>(value, "ASCII");

        public static TiffValueRationals AsRtaionals(this TiffValue value) => CastValueType<TiffValueRationals>(value, "Rationals");

        public static ITiffValueIntegers AsNumbers(this TiffValue value) => CastValueType<ITiffValueIntegers>(value, "Integers");

    }

}
