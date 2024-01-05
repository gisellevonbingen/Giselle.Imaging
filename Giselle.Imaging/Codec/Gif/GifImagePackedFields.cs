using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifImagePackedFields : IEquatable<GifImagePackedFields>
    {
        public static GifImagePackedFields Empty { get; } = new GifImagePackedFields();

        public static BitVector32.Section LocalColorTableSizeSection { get; } = BitVector32.CreateSection(0x07);
        public static BitVector32.Section ReservedSection { get; } = BitVector32.CreateSection(0x03, LocalColorTableSizeSection);
        public static BitVector32.Section SortFlagSection { get; } = BitVector32.CreateSection(0x01, ReservedSection);
        public static BitVector32.Section InterlaceFlagSection { get; } = BitVector32.CreateSection(0x01, SortFlagSection);
        public static BitVector32.Section LocalColorTableFlagSection { get; } = BitVector32.CreateSection(0x01, InterlaceFlagSection);

        public bool LocalColorTableFlag { get; set; }
        public bool Interlace { get; set; }
        public bool SortFlag { get; set; }
        public int LocalColorTableSize { get; set; }

        public byte Raw
        {
            get => (byte)new BitVector32(0)
            {
                [LocalColorTableFlagSection] = Convert.ToInt32(this.LocalColorTableFlag),
                [InterlaceFlagSection] = Convert.ToInt32(this.Interlace),
                [SortFlagSection] = Convert.ToInt32(this.SortFlag),
                [LocalColorTableSizeSection] = this.LocalColorTableFlag ? (int)Math.Ceiling(Math.Log2(this.LocalColorTableSize)) - 1 : 0,
            }.Data;

            set
            {
                var packedFields = new BitVector32(value);
                this.LocalColorTableFlag = Convert.ToBoolean(packedFields[LocalColorTableFlagSection]);
                this.Interlace = Convert.ToBoolean(packedFields[InterlaceFlagSection]);
                this.SortFlag = Convert.ToBoolean(packedFields[SortFlagSection]);
                this.LocalColorTableSize = 1 << (packedFields[LocalColorTableSizeSection] + 1);
            }

        }

        public override int GetHashCode() => this.Raw;

        public override string ToString() => $"{nameof(LocalColorTableFlag)}:{this.LocalColorTableFlag}, {nameof(Interlace)}:{this.Interlace}, {nameof(SortFlag)}:{this.SortFlag}, {nameof(LocalColorTableSize)}:{this.LocalColorTableSize}";

        public override bool Equals(object obj) => obj is GifImagePackedFields other && this.Equals(other);

        public bool Equals(GifImagePackedFields other) => this.Raw == other.Raw;

        public static bool operator ==(GifImagePackedFields left, GifImagePackedFields right) => left.Equals(right) == true;

        public static bool operator !=(GifImagePackedFields left, GifImagePackedFields right) => left.Equals(right) == false;
    }

}
