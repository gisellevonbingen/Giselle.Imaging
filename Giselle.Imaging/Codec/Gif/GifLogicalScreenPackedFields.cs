using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Gif
{
    public struct GifLogicalScreenPackedFields : IEquatable<GifLogicalScreenPackedFields>
    {
        public static GifLogicalScreenPackedFields Empty { get; } = new GifLogicalScreenPackedFields();

        public static BitVector32.Section GlobalColorTableSizeSection { get; } = BitVector32.CreateSection(0x07);
        public static BitVector32.Section SortFlagSection { get; } = BitVector32.CreateSection(0x01, GlobalColorTableSizeSection);
        public static BitVector32.Section ColorResolutionSection { get; } = BitVector32.CreateSection(0x07, SortFlagSection);
        public static BitVector32.Section GlobalColorTableFlagSection { get; } = BitVector32.CreateSection(0x01, ColorResolutionSection);

        public bool GlobalColorTableFlag { get; set; }
        public int ColorResolution { get; set; }
        public bool SortFlag { get; set; }
        public int GlobalColorTableSize { get; set; }

        public byte Raw
        {
            get => (byte)new BitVector32(0)
            {
                [GlobalColorTableFlagSection] = Convert.ToInt32(this.GlobalColorTableFlag),
                [ColorResolutionSection] = this.ColorResolution - 1,
                [SortFlagSection] = Convert.ToInt32(this.SortFlag),
                [GlobalColorTableSizeSection] = this.GlobalColorTableFlag ? (int)Math.Ceiling(Math.Log2(this.GlobalColorTableSize)) - 1 : 0,
            }.Data;

            set
            {
                var packedFields = new BitVector32(value);
                this.GlobalColorTableFlag = Convert.ToBoolean(packedFields[GlobalColorTableFlagSection]);
                this.ColorResolution = packedFields[ColorResolutionSection] + 1;
                this.SortFlag = Convert.ToBoolean(packedFields[SortFlagSection]);
                this.GlobalColorTableSize = 1 << (packedFields[GlobalColorTableSizeSection] + 1);
            }

        }

        public GifLogicalScreenPackedFields Normalized => new() { Raw = this.Raw };

        public override int GetHashCode() => this.Raw;

        public override string ToString() => $"{nameof(GlobalColorTableFlag)}:{this.GlobalColorTableFlag}, {nameof(ColorResolution)}:{this.ColorResolution}, {nameof(SortFlag)}:{this.SortFlag}, {nameof(GlobalColorTableSize)}:{this.GlobalColorTableSize}";

        public override bool Equals(object obj) => obj is GifLogicalScreenPackedFields other && this.Equals(other);

        public bool Equals(GifLogicalScreenPackedFields other) => this.Raw == other.Raw;

        public static bool operator ==(GifLogicalScreenPackedFields left, GifLogicalScreenPackedFields right) => left.Equals(right) == true;

        public static bool operator !=(GifLogicalScreenPackedFields left, GifLogicalScreenPackedFields right) => left.Equals(right) == false;
    }

}
