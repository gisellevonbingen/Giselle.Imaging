using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
{
    public struct Int32ChannelMask : IEquatable<Int32ChannelMask>
    {
        public static implicit operator Int32ChannelMask(int raw) => FromRaw(raw);

        public static implicit operator Int32ChannelMask(uint raw) => FromRaw(raw);

        public static bool operator ==(Int32ChannelMask left, Int32ChannelMask right) => left.Equals(right) == true;
        public static bool operator !=(Int32ChannelMask left, Int32ChannelMask right) => left.Equals(right) == false;

        public static Int32ChannelMask FromRaw(int raw) => FromRaw((uint)raw);

        public static Int32ChannelMask FromRaw(uint raw)
        {
            var offset = 0;
            var length = 0;
            var bits = sizeof(uint) * 8;
            int i;

            for (i = 0; i < bits; i++)
            {
                if ((raw & 1) == 1)
                {
                    offset = i;
                    break;
                }
                else
                {
                    raw >>= 1;
                }

            }

            for (i = offset + 1; i < bits; i++)
            {
                if ((raw & 1) == 0)
                {
                    length = i - offset - 1;
                    break;
                }
                else
                {
                    raw >>= 1;
                }

            }

            if (i == bits)
            {
                length = bits - offset;
            }

            return new Int32ChannelMask(offset, length);
        }

        public int Offset { get; }
        public int Length { get; }
        public int Mask { get; }

        public Int32ChannelMask(int offset, int length) : this()
        {
            this.Offset = offset;
            this.Length = length;
            this.Mask = (int)((uint)Math.Pow(2, length) - 1);
        }

        public byte SplitByte(int mergedValue, byte fallback = 0)
        {
            if (this.Mask == 0) return fallback;
            var chunk = (mergedValue >> this.Offset) & this.Mask;
            return (byte)((chunk * byte.MaxValue) / this.Mask);
        }

        public int MergeByte(int mergedValue, byte value)
        {
            if (this.Mask == 0) return mergedValue;
            var raw = (value * this.Mask) / byte.MaxValue;
            return mergedValue | raw << this.Offset;
        }

        public override string ToString() => $"[Offset: {this.Offset}, Length: {this.Length}]";

        public override int GetHashCode() => HashCode.Combine(Offset, Length);

        public override bool Equals(object obj)
        {
            return obj is Int32ChannelMask other && this.Equals(other);
        }

        public bool Equals(Int32ChannelMask other)
        {
            return this.Offset == other.Offset && this.Length == other.Length;
        }

    }

}
