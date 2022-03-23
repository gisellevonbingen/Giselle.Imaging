using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Checksum
{
    public static class CRCUtils
    {
        public const uint CRC32Seed = 0xFFFFFFFF;
        public const uint CRC32Polynomial = 0xEDB88320;

        private readonly static uint[] CRC32Table = null;

        static CRCUtils()
        {
            var table = CRC32Table = new uint[byte.MaxValue + 1];

            for (int b = 0; b < table.Length; b++)
            {
                table[b] = GenerateCRC32TableEntry(CRC32Polynomial, (byte)b);
            }

        }

        private static uint GenerateCRC32TableEntry(uint polynomial, byte data)
        {
            uint entry = data;

            for (int i = 0; i < 8; i++)
            {
                if ((entry & 1) == 1)
                {
                    entry = (entry >> 1) ^ polynomial;
                }
                else
                {
                    entry >>= 1;
                }

            }

            return entry;
        }

        public static uint AccumulateCRC32(IList<byte> data)
        {
            return AccumulateCRC32(CRC32Seed, data);
        }

        public static uint AccumulateCRC32(uint seed, IList<byte> data)
        {
            return AccumulateCRC32(seed, data, 0, data.Count);
        }

        public static uint CalculateCRC32(IList<byte> data)
        {
            return CalculateCRC32(CRC32Seed, data);
        }

        public static uint CalculateCRC32(uint seed, IList<byte> data)
        {
            var crc = AccumulateCRC32(seed, data, 0, data.Count);
            return FinalizeCalculateCRC32(crc);
        }

        public static uint AccumulateCRC32(uint crc, IList<byte> data, int offset, int length)
        {
            var c = crc;

            for (var i = 0; i < length; i++)
            {
                var v = data[offset + i];
                c = AccumulateCRC32(c, v);
            }

            return c;
        }

        public static uint AccumulateCRC32(uint seed, byte b)
        {
            return (seed >> 8) ^ CRC32Table[(b ^ seed) & 0xFF];
        }

        public static uint FinalizeCalculateCRC32(uint crc)
        {
            return ~crc;
        }

    }

}
