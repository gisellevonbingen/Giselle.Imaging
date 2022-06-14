using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public struct Argb32 : IEquatable<Argb32>
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Argb32(byte r, byte g, byte b)
        {
            this.A = byte.MaxValue;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public Argb32(byte a, byte r, byte g, byte b)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public Argb32 DeriveA(byte a) => new Argb32(a, this.R, this.G, this.B);

        public override bool Equals(object obj) => obj is Argb32 other && this.Equals(other);

        public bool Equals(Argb32 other) => (this.A == other.A) && (this.R == other.R) && (this.G == other.G) && (this.B == other.B);

        public override int GetHashCode() => (this.A << 0x18) | (this.R << 0x10) | (this.G << 0x08) | (this.B << 0x00);

        public override string ToString() => $"{this.GetHashCode():X8}";

    }

}
