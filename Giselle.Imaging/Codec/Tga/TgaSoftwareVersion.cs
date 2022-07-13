using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tga
{
    public struct TgaSoftwareVersion : IEquatable<TgaSoftwareVersion>
    {
        public ushort VersionNumber { get; set; }
        public byte VersionLetter { get; set; }

        public TgaSoftwareVersion(DataProcessor input) :this()
        {
            this.Read(input);
        }

        public void Read(DataProcessor input)
        {
            this.VersionNumber = input.ReadUShort();
            this.VersionLetter = input.ReadByte();
        }

        public void Write(DataProcessor output)
        {
            output.WriteUShort(this.VersionNumber);
            output.WriteByte(this.VersionLetter);
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 31 + this.VersionNumber.GetHashCode();
            hash = hash * 31 + this.VersionLetter.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj) => obj is TgaSoftwareVersion other && this.Equals(other);

        public bool Equals(TgaSoftwareVersion other) => this.VersionNumber == other.VersionNumber && this.VersionLetter == other.VersionLetter;

        public override string ToString() => $"{this.VersionNumber / 100.0D:F2}{(char)this.VersionLetter}";
    }

}
