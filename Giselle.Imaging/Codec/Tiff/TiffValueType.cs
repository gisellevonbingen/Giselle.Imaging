using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueType
    {
        public static IEnumerable<TiffValueType> Values => _Values.AsReadOnly();
        private readonly static List<TiffValueType> _Values = new List<TiffValueType>();
        private readonly static Dictionary<short, TiffValueType> Lookups = new Dictionary<short, TiffValueType>();

        public static TiffValueType Byte = Register(new TiffValueTypeInteter("Byte", 1, dp => dp.ReadByte(), (dp, v) => dp.WriteByte((byte)v)));
        public static TiffValueType ASCII = Register(new TiffValueTypeASCII("ASCII", 2));
        public static TiffValueType Short = Register(new TiffValueTypeInteter("Short", 3, dp => dp.ReadUShort(), (dp, v) => dp.WriteUShort((ushort)v)));
        public static TiffValueType Long = Register(new TiffValueTypeInteter("Long", 4, dp => (int)dp.ReadUInt(), (dp, v) => dp.WriteUInt((uint)v)));
        public static TiffValueType Rational = Register(new TiffValueTypeRational("Rational", 5));
        public static TiffValueType SByte = Register(new TiffValueTypeInteter("SByte", 6, dp => dp.ReadSByte(), (dp, v) => dp.WriteSByte((sbyte)v)));
        public static TiffValueType Undefined = Register(new TiffValueType("Undefined", 7, false));
        public static TiffValueType SShort = Register(new TiffValueTypeInteter("SShort", 8, dp => dp.ReadShort(), (dp, v) => dp.WriteShort((short)v)));
        public static TiffValueType SLong = Register(new TiffValueTypeInteter("SLong", 9, dp => dp.ReadInt(), (dp, v) => dp.WriteInt(v)));
        public static TiffValueType SRational = Register(new TiffValueType("SRational", 10, true));
        public static TiffValueType Float = Register(new TiffValueType("Float", 11, false));
        public static TiffValueType Double = Register(new TiffValueType("Double", 12, false));

        public static TiffValueType FromId(short id, TiffValueType fallback = default) => Lookups.TryGetValue(id, out var value) ? value : fallback;

        private static T Register<T>(T value) where T : TiffValueType
        {
            _Values.Add(value);
            Lookups[value.Id] = value;
            return value;
        }

        public string Name { get; }
        public short Id { get; }
        public bool DefaultOffset { get; }

        public TiffValueType(string name, short id, bool defaultOffset)
        {
            this.Name = name;
            this.Id = id;
            this.DefaultOffset = defaultOffset;
        }

        public override string ToString() => this.Name;

        public override int GetHashCode() => this.Id;

        public override bool Equals(object obj) => this == obj;

    }

    public class TiffValueTypeInteter : TiffValueType
    {
        private Func<DataProcessor, int> Reader { get; }
        private Action<DataProcessor, int> Writer { get; }

        public TiffValueTypeInteter(string name, short id, Func<DataProcessor, int> reader, Action<DataProcessor, int> writer) : base(name, id, false)
        {
            this.Reader = reader;
            this.Writer = writer;
        }

        public int ReadAsSigned(DataProcessor processor) => this.Reader(processor);

        public void WriteAsSigned(DataProcessor processor, int value) => this.Writer(processor, value);

        public uint ReadAsUnsigned(DataProcessor processor) => (uint)this.ReadAsSigned(processor);

        public void WriteAsUnsigned(DataProcessor processor, uint value) => this.WriteAsSigned(processor, (int)value);
    }

    public class TiffValueTypeRational : TiffValueType
    {
        public TiffValueTypeRational(string name, short id) : base(name, id, true)
        {

        }

        public TiffRational Read(DataProcessor processor) => new TiffRational()
        {
            Numerator = processor.ReadInt(),
            Denominator = processor.ReadInt(),
        };

        public void Write(DataProcessor processor, TiffRational value)
        {
            processor.WriteInt(value.Numerator);
            processor.WriteInt(value.Denominator);
        }

    }

    public class TiffValueTypeASCII : TiffValueType
    {
        public TiffValueTypeASCII(string name, short id) : base(name, id, true)
        {

        }

        public string Read(DataProcessor processor, int lengthWithNull)
        {
            var bytes = processor.ReadBytes(lengthWithNull);

            if (bytes[lengthWithNull - 1] != '\0')
            {
                throw new IOException("ASCII fianl bytes is not '\\0'");
            }

            return Encoding.ASCII.GetString(bytes, 0, bytes.Length - 1);
        }

        public int Write(DataProcessor processor, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value + '\0');
            processor.WriteBytes(bytes);
            return bytes.Length;
        }

    }

}
