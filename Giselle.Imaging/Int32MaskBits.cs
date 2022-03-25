using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public struct Int32MaskBits : IEquatable<Int32MaskBits>
    {
        public static implicit operator Int32MaskBits(int raw) => FromRaw(raw);

        public static implicit operator Int32MaskBits(uint raw) => FromRaw(raw);

        public static Int32MaskBits FromRaw(int raw) => FromRaw((uint)raw);

        public static Int32MaskBits FromRaw(uint raw)
        {
            var offset = 0;
            var length = 0;
            var bits = sizeof(uint) * 8;
            var i = 0;

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

            return new Int32MaskBits(offset, length);
        }

        public int Offset { get; }
        public int Length { get; }
        public int Mask { get; }

        public Int32MaskBits(int offset, int length) : this()
        {
            this.Offset = offset;
            this.Length = length;
            this.Mask = (int)((uint)Math.Pow(2, length) - 1);
        }

        public byte SplitByte(int mergedValue)
        {
            if (this.Mask == 0) return 0;
            var chunk = (mergedValue >> this.Offset) & this.Mask;
            return (byte)((chunk * byte.MaxValue) / this.Mask);
        }

        public int MergeByte(int mergedValue, byte value)
        {
            if (this.Mask == 0) return 0;
            var raw = (value * this.Mask) / byte.MaxValue;
            return mergedValue | raw << this.Offset;
        }

        public override string ToString() => $"[Offset={this.Offset}, Length={this.Length}]";

        public override int GetHashCode()
        {
            var seed = 17;
            seed = seed * 31 + this.Offset.GetHashCode();
            seed = seed * 31 + this.Length.GetHashCode();
            return seed;
        }

        public override bool Equals(object obj)
        {
            return obj is Int32MaskBits other && this.Equals(other);
        }

        public bool Equals(Int32MaskBits other)
        {
            return this.Offset == other.Offset && this.Length == other.Length;
        }

    }

}
