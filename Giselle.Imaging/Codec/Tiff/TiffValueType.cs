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

        public static TiffValueType Byte = Register(new TiffValueType("Byte", 1, false, () => new TiffValueBytes()));
        public static TiffValueType ASCII = Register(new TiffValueType("ASCII", 2, true, () => new TiffValueASCII()));
        public static TiffValueType Short = Register(new TiffValueType("Short", 3, false, () => new TiffValueShorts()));
        public static TiffValueType Long = Register(new TiffValueType("Long", 4, false, () => new TiffValueLongs()));
        public static TiffValueType Rational = Register(new TiffValueType("Rational", 5, true, () => new TiffValueRationals()));
        public static TiffValueType SByte = Register(new TiffValueType("SByte", 6, false, () => new TiffValueSBytes()));
        public static TiffValueType Undefined = Register(new TiffValueType("Undefined", 7, false, null));
        public static TiffValueType SShort = Register(new TiffValueType("SShort", 8, false, () => new TiffValueSShorts()));
        public static TiffValueType SLong = Register(new TiffValueType("SLong", 9, false, () => new TiffValueSLongs()));
        public static TiffValueType SRational = Register(new TiffValueType("SRational", 10, true, null));
        public static TiffValueType Float = Register(new TiffValueType("Float", 11, false, null));
        public static TiffValueType Double = Register(new TiffValueType("Double", 12, false, null));

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

        public Func<TiffValue> ValueGenerator { get; }

        public TiffValueType(string name, short id, bool defaultOffset, Func<TiffValue> valueGenerator)
        {
            this.Name = name;
            this.Id = id;
            this.DefaultOffset = defaultOffset;
            this.ValueGenerator = valueGenerator;
        }

        public override string ToString() => this.Name;

        public override int GetHashCode() => this.Id;

        public override bool Equals(object obj) => this == obj;

    }

    public class TiffValueType<H, V> : TiffValueType where H : TiffValue
    {
        public TiffValueType(string name, short id, bool defaultOffset, Func<H> valueGenerator) : base(name, id, defaultOffset, valueGenerator)
        {

        }

    }

}
