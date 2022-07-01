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
            var bytes = processor.ReadBytes(4);
            return new Version(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        public static void WriteVersion(this DataProcessor processor, Version version)
        {
            var bytes = new byte[4] { (byte)version.Major, (byte)version.Minor, (byte)version.Build, (byte)version.Revision };
            processor.WriteBytes(bytes);
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
