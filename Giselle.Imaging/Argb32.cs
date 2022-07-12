using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public struct Argb32 : IEquatable<Argb32>
    {
        public const int Samples = 4;
        public const float SampleMinAsFloat = byte.MinValue;
        public const float SampleMaxAsFloat = byte.MaxValue;

        public static Argb32 White { get; } = new Argb32(0xFF, 0xFF, 0xFF);
        public static Argb32 Black { get; } = new Argb32(0x00, 0x00, 0x00);

        public static bool operator ==(Argb32 o1, Argb32 o2) => o1.GetHashCode() == o2.GetHashCode();

        public static bool operator !=(Argb32 o1, Argb32 o2) => o1.GetHashCode() != o2.GetHashCode();

        public static Argb32 operator ^(Argb32 o1, Argb32 o2) => CalcaulateSamples(o1, o2, (b1, b2) => (byte)(b1 ^ b2));

        public static Argb32 operator &(Argb32 o1, Argb32 o2) => CalcaulateSamples(o1, o2, (b1, b2) => (byte)(b1 & b2));

        public static Argb32 operator |(Argb32 o1, Argb32 o2) => CalcaulateSamples(o1, o2, (b1, b2) => (byte)(b1 | b2));

        public static Argb32 operator +(Argb32 o1, Argb32 o2) => CalcaulateSamples(o1, o2, (b1, b2) => (byte)Math.Min(b1 + b2, byte.MaxValue));

        public static Argb32 operator -(Argb32 o1, Argb32 o2) => CalcaulateSamples(o1, o2, (b1, b2) => (byte)Math.Max(b1 - b2, byte.MinValue));

        public static Argb32 operator *(Argb32 o1, Argb32 o2) => CalcaulateSamples(o1, o2, MulSample);

        public static Argb32 operator /(Argb32 o1, Argb32 o2) => CalcaulateSamples(o1, o2, DivSample);

        public static Argb32 operator *(Argb32 o, float mul) => CalcaulateSamples(o, mul, (s, c) => s * c);

        public static Argb32 operator /(Argb32 o, float div) => CalcaulateSamples(o, div, (s, c) => s / c);

        private static Argb32 CalcaulateSamples(Argb32 o1, Argb32 o2, Func<byte, byte, byte> func)
        {
            var samples1 = o1.GetSamples();
            var samples2 = o2.GetSamples();
            var finalSamples = new byte[Samples];

            for (var i = 0; i < Samples; i++)
            {
                finalSamples[i] = func(samples1[i], samples2[i]);
            }

            return new Argb32(finalSamples);
        }

        private static Argb32 CalcaulateSamples(Argb32 o, float coefficient, Func<float, float, float> func)
        {
            var samples = o.GetSamples();
            var finalSamples = new byte[Samples];

            for (var i = 0; i < Samples; i++)
            {
                finalSamples[i] = (byte)(func(samples[i] / SampleMaxAsFloat, coefficient) * SampleMaxAsFloat);
            }

            return new Argb32(finalSamples);
        }

        private static byte MulSample(byte b1, byte b2)
        {
            var f1 = b1 / SampleMaxAsFloat;
            var f2 = b2 / SampleMaxAsFloat;
            return (byte)((f1 * f2) * SampleMaxAsFloat);
        }

        private static byte DivSample(byte b1, byte b2)
        {
            var f1 = b1 / SampleMaxAsFloat;
            var f2 = b2 / SampleMaxAsFloat;
            return (byte)((f1 / f2) * SampleMaxAsFloat);
        }

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

        public Argb32(byte[] samples)
        {
            if (samples.Length == 3)
            {
                this.A = byte.MaxValue;
                this.R = samples[0];
                this.G = samples[1];
                this.B = samples[2];
            }
            else if (samples.Length == 4)
            {
                this.A = samples[0];
                this.R = samples[1];
                this.G = samples[2];
                this.B = samples[3];
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

        }

        public byte[] GetSamples() => new byte[Samples] { this.A, this.R, this.G, this.B };

        public byte this[int sampleIndex]
        {
            get
            {
                if (sampleIndex == 0) return this.A;
                else if (sampleIndex == 1) return this.R;
                else if (sampleIndex == 2) return this.G;
                else if (sampleIndex == 3) return this.B;
                else throw new ArgumentOutOfRangeException();
            }

            set
            {
                if (sampleIndex == 0) this.A = value;
                else if (sampleIndex == 1) this.R = value;
                else if (sampleIndex == 2) this.G = value;
                else if (sampleIndex == 3) this.B = value;
                else throw new ArgumentOutOfRangeException();
            }

        }

        public byte Luminance => (byte)(this.R * 0.2126D + this.G * 0.7152D + this.B * 0.0722D);

        public Argb32 DeriveA(byte a) => new Argb32(a, this.R, this.G, this.B);

        public override bool Equals(object obj) => obj is Argb32 other && this.Equals(other);

        public bool Equals(Argb32 other) => (this.A == other.A) && (this.R == other.R) && (this.G == other.G) && (this.B == other.B);

        public override int GetHashCode() => (this.A << 0x18) | (this.R << 0x10) | (this.G << 0x08) | (this.B << 0x00);

        public override string ToString() => $"{this.GetHashCode():X8}";

    }

}
