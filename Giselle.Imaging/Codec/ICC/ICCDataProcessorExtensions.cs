using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.ICC
{
    public static class ICCDataProcessorExtensions
    {
        public static Version ReadVersion(this DataProcessor processor)
        {
            var major = processor.ReadByte();
            var minor = processor.ReadByte();
            var build = processor.ReadByte();
            var revision = processor.ReadByte();
            return new Version(major, minor, build, revision);
        }

        public static void WriteVersion(this DataProcessor processor, Version version)
        {
            processor.WriteByte((byte)version.Major);
            processor.WriteByte((byte)version.Minor);
            processor.WriteByte((byte)version.Build);
            processor.WriteByte((byte)version.Revision);
        }

        public static float ReadS15Fixed16(this DataProcessor processor)
        {
            var exponent = processor.ReadShort();
            var fractionRaw = processor.ReadUShort();
            var fraction = (float)fractionRaw / ushort.MaxValue;

            return exponent + fraction;
        }

        public static void WriteS15Fixed16(this DataProcessor processor, float value)
        {
            var exponent = (ushort)Math.Floor(value);
            processor.WriteUShort(exponent);
            var fraction = value - exponent;
            var fractionRaw = (ushort)Math.Round(fraction * ushort.MaxValue);
            processor.WriteUShort(fractionRaw);
        }

    }

}
