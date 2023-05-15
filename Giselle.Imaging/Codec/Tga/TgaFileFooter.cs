using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaFileFooter
    {
        public const int Length = 26;
        public static byte[] Signature { get; } = Encoding.ASCII.GetBytes("TRUEVISION-XFILE");

        public int ExtensionOffset { get; set; }
        public int DeveloperAreaOffset { get; set; }

        public TgaFileFooter()
        {

        }

        public TgaFileFooter(DataProcessor input) : this()
        {
            this.Read(input);
        }

        public void Read(DataProcessor input)
        {
            this.ExtensionOffset = input.ReadInt();
            this.DeveloperAreaOffset = input.ReadInt();

            var signature = input.ReadBytes(Signature.Length);

            if (signature.SequenceEqual(Signature) == false)
            {
                throw new IOException("TgaFileFooter Signature Mismatched");
            }

            var dot = input.ReadByte();

            if (dot != 0x2E)
            {
                throw new IOException("TgaFileFooter Constant Mismatched");
            }

            var nil = input.ReadByte();

            if (nil != 0x00)
            {
                throw new IOException("TgaFileFooter Constant Mismatched");
            }

        }

        public void Write(DataProcessor output)
        {
            output.WriteInt(this.ExtensionOffset);
            output.WriteInt(this.DeveloperAreaOffset);
            output.WriteBytes(Signature);
            output.WriteByte(0x2E);
            output.WriteByte(0x00);
        }

    }

}
