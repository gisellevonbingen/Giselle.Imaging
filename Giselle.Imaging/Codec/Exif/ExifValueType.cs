using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifValueType
    {
        public static IEnumerable<ExifValueType> Values => _Values.AsReadOnly();
        private readonly static List<ExifValueType> _Values = new List<ExifValueType>();
        private readonly static Dictionary<short, ExifValueType> Lookups = new Dictionary<short, ExifValueType>();

        public static ExifValueType Byte = Register(new ExifValueType("Byte", 1, false, () => new ExifValueBytes()));
        public static ExifValueType ASCII = Register(new ExifValueType("ASCII", 2, true, () => new ExifValueASCII()));
        public static ExifValueType Short = Register(new ExifValueType("Short", 3, false, () => new ExifValueShorts()));
        public static ExifValueType Long = Register(new ExifValueType("Long", 4, false, () => new ExifValueLongs()));
        public static ExifValueType Rational = Register(new ExifValueType("Rational", 5, true, () => new ExifValueRationals()));
        public static ExifValueType SByte = Register(new ExifValueType("SByte", 6, false, () => new ExifValueSBytes()));
        public static ExifValueType Undefined = Register(new ExifValueType("Undefined", 7, false, null));
        public static ExifValueType SShort = Register(new ExifValueType("SShort", 8, false, () => new ExifValueSShorts()));
        public static ExifValueType SLong = Register(new ExifValueType("SLong", 9, false, () => new ExifValueSLongs()));
        public static ExifValueType SRational = Register(new ExifValueType("SRational", 10, true, null));
        public static ExifValueType Float = Register(new ExifValueType("Float", 11, false, null));
        public static ExifValueType Double = Register(new ExifValueType("Double", 12, false, null));

        public static ExifValueType FromId(short id, ExifValueType fallback = default) => Lookups.TryGetValue(id, out var value) ? value : fallback;

        private static T Register<T>(T value) where T : ExifValueType
        {
            _Values.Add(value);
            Lookups[value.Id] = value;
            return value;
        }

        public string Name { get; }
        public short Id { get; }
        public bool DefaultOffset { get; }

        public Func<ExifValue> ValueGenerator { get; }

        public ExifValueType(string name, short id, bool defaultOffset, Func<ExifValue> valueGenerator)
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

    public class TiffValueType<H, V> : ExifValueType where H : ExifValue
    {
        public TiffValueType(string name, short id, bool defaultOffset, Func<H> valueGenerator) : base(name, id, defaultOffset, valueGenerator)
        {

        }

    }

}
