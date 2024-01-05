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
    public struct GifGraphicControlPackedFields : IEquatable<GifGraphicControlPackedFields>
    {
        public static GifGraphicControlPackedFields Empty { get; } = new GifGraphicControlPackedFields();

        public static BitVector32.Section TransparentColorFlagSection { get; } = BitVector32.CreateSection(0x01);
        public static BitVector32.Section UserInputFlagSection { get; } = BitVector32.CreateSection(0x01, TransparentColorFlagSection);
        public static BitVector32.Section DisposalMethodSection { get; } = BitVector32.CreateSection(0x07, UserInputFlagSection);
        public static BitVector32.Section ReservedSection { get; } = BitVector32.CreateSection(0x01, DisposalMethodSection);

        public byte Reserved { get; set; }
        public GifDisposalMethod DisposalMethod { get; set; }
        public bool UserInputFlag { get; set; }
        public bool TransparentColorFlag { get; set; }

        public byte Raw
        {
            get => (byte)new BitVector32(0)
            {
                [ReservedSection] = this.Reserved,
                [DisposalMethodSection] = (byte)this.DisposalMethod,
                [UserInputFlagSection] = Convert.ToInt32(this.UserInputFlag),
                [TransparentColorFlagSection] = Convert.ToInt32(this.TransparentColorFlag),
            }.Data;

            set
            {
                var packedFields = new BitVector32(value);
                this.Reserved = (byte)packedFields[ReservedSection];
                this.DisposalMethod = (GifDisposalMethod)packedFields[DisposalMethodSection];
                this.UserInputFlag = Convert.ToBoolean(packedFields[UserInputFlagSection]);
                this.TransparentColorFlag = Convert.ToBoolean(packedFields[TransparentColorFlagSection]);
            }

        }

        public GifGraphicControlPackedFields Normalized => new() { Raw = this.Raw };

        public override int GetHashCode() => this.Raw;

        public override string ToString() => $"{nameof(Reserved)}:{this.Reserved}, {nameof(DisposalMethod)}:{this.DisposalMethod}, {nameof(UserInputFlag)}:{this.UserInputFlag}, {nameof(TransparentColorFlag)}:{this.TransparentColorFlag}";

        public override bool Equals(object obj) => obj is GifGraphicControlPackedFields other && this.Equals(other);

        public bool Equals(GifGraphicControlPackedFields other) => this.Raw == other.Raw;

        public static bool operator ==(GifGraphicControlPackedFields left, GifGraphicControlPackedFields right) => left.Equals(right) == true;

        public static bool operator !=(GifGraphicControlPackedFields left, GifGraphicControlPackedFields right) => left.Equals(right) == false;
    }

}
