using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public static class TiffEntryExtensions
    {
        public static TiffValueTypeASCII CastValueTypeASCII(this TiffEntry entry) => CastValueType<TiffValueTypeASCII>(entry.ValueType, "ASCII");

        public static TiffValueTypeRational CastValueTypeRational(this TiffEntry entry) => CastValueType<TiffValueTypeRational>(entry.ValueType, "Rational");

        public static TiffValueTypeInteter CastValueTypeInteger(this TiffEntry entry) => CastValueType<TiffValueTypeInteter>(entry.ValueType, "Integer");

        public static T CastValueType<T>(this TiffEntry entry, string name) where T : TiffValueType => CastValueType<T>(entry.ValueType, name);

        public static T CastValueType<T>(TiffValueType type, string name) where T : TiffValueType
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

        public static T[] ReadArray<T>(this TiffEntry entry, DataProcessor processor, Func<DataProcessor, T> reader)
        {
            var array = new T[entry.ValueCount];

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = reader(processor);
            }

            return array;
        }

        public static void WriteArray<T>(this TiffEntry entry, DataProcessor processor, Action<DataProcessor, T> writer, T[] array)
        {
            entry.ValueCount = array.Length;

            for (var i = 0; i < array.Length; i++)
            {
                writer(processor, array[i]);
            }

        }

        public static int ReadAsSigned(this TiffEntry entry, DataProcessor processor) => ReadAsSigneds(entry, processor)[0];

        public static void WriteAsSigned(this TiffEntry entry, DataProcessor processor, int value) => WriteAsSigneds(entry, processor, new[] { value });

        public static int[] ReadAsSigneds(this TiffEntry entry, DataProcessor processor)
        {
            var integer = entry.CastValueTypeInteger();

            if (entry.ValueCount > 1)
            {
                return ReadArray(entry, processor, integer.ReadAsSigned);
            }
            else
            {
                return new[] { entry.ValueOrOffset };
            }

        }

        public static void WriteAsSigneds(this TiffEntry entry, DataProcessor processor, int[] values)
        {
            var integer = entry.CastValueTypeInteger();

            if (values.Length > 1)
            {
                WriteArray(entry, processor, integer.WriteAsSigned, values);
            }
            else
            {
                entry.ValueCount = 1;
                entry.ValueOrOffset = values[0];
            }

        }

        public static uint ReadAsUnsigned(this TiffEntry entry, DataProcessor processor) => ReadAsUnsigneds(entry, processor)[0];

        public static void WriteAsUnsigned(this TiffEntry entry, DataProcessor processor, uint value) => WriteAsUnsigneds(entry, processor, new[] { value });

        public static uint[] ReadAsUnsigneds(this TiffEntry entry, DataProcessor processor)
        {
            var integer = entry.CastValueTypeInteger();

            if (entry.ValueCount > 1)
            {
                return ReadArray(entry, processor, integer.ReadAsUnsigned);
            }
            else
            {
                return new[] { (uint)entry.ValueOrOffset };
            }

        }

        public static void WriteAsUnsigneds(this TiffEntry entry, DataProcessor processor, uint[] values)
        {
            var integer = entry.CastValueTypeInteger();

            if (values.Length > 1)
            {
                WriteArray(entry, processor, integer.WriteAsUnsigned, values);
            }
            else
            {
                entry.ValueCount = values.Length;
                entry.ValueOrOffset = (int)values[0];
            }

        }

        public static TiffRational ReadAsRational(this TiffEntry entry, DataProcessor processor) => ReadAsRationals(entry, processor)[0];

        public static void WriteAsRational(this TiffEntry entry, DataProcessor processor, TiffRational value) => WriteAsRationals(entry, processor, new[] { value });

        public static TiffRational[] ReadAsRationals(this TiffEntry entry, DataProcessor processor)
        {
            var rational = entry.CastValueTypeRational();
            return ReadArray(entry, processor, rational.Read);
        }

        public static void WriteAsRationals(this TiffEntry entry, DataProcessor processor, TiffRational[] values)
        {
            var rational = entry.CastValueTypeRational();
            WriteArray(entry, processor, rational.Write, values);
        }

        public static string ReadAsASCII(this TiffEntry entry, DataProcessor processor)
        {
            var ascii = entry.CastValueTypeASCII();
            return ascii.Read(processor, entry.ValueCount);
        }

        public static void WriteAsASCII(this TiffEntry entry, DataProcessor processor, string value)
        {
            var ascii = entry.CastValueTypeASCII();
            entry.ValueCount = ascii.Write(processor, value);
        }

    }

}
