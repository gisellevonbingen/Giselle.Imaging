using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

namespace Giselle.Imaging.Codec.ICC
{
    public struct ICCXYZ : IEquatable<ICCXYZ>
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public ICCXYZ(float x, float y, float z) : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public ICCXYZ(DataProcessor processor) : this()
        {
            this.Read(processor);
        }

        public void Read(DataProcessor processor)
        {
            this.X = processor.ReadS15Fixed16();
            this.Y = processor.ReadS15Fixed16();
            this.Z = processor.ReadS15Fixed16();
        }

        public void Write(DataProcessor processor)
        {
            processor.WriteS15Fixed16(this.X);
            processor.WriteS15Fixed16(this.Y);
            processor.WriteS15Fixed16(this.Z);
        }

        public override string ToString()
        {
            return $"[{this.X}, {this.Y}, {this.Z}]";
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 31 + this.X.GetHashCode();
            hash = hash * 31 + this.Y.GetHashCode();
            hash = hash * 31 + this.Z.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            return obj is ICCXYZ other && this.Equals(other);
        }

        public bool Equals(ICCXYZ other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

    }

}
