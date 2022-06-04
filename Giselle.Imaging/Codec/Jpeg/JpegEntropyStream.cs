using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Huffman;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpegEntropyStream : AbstractHuffmanStream
    {
        private Dictionary<byte, HuffmanCode> CodeTable;

        public JpegEntropyStream(Stream baseStream) : this(baseStream, false)
        {

        }

        public JpegEntropyStream(Stream baseStream, bool leaveOpen) : base(baseStream, leaveOpen)
        {
            this.CodeTable = new Dictionary<byte, HuffmanCode>();
        }

        protected override Dictionary<byte, HuffmanCode> NextReadingCodes() => this.CodeTable;

        protected override Dictionary<byte, HuffmanCode> NextWritingCodes() => this.CodeTable;

        protected override int ReadEncodedByte()
        {
            var b = base.ReadEncodedByte();

            if (b == 0xFF)
            {
                // Must be zero
                this.BaseStream.ReadByte();
            }

            return b;
        }

        protected override bool TryWriteEncodedByte(byte value, int position, bool disposing)
        {
            if (base.TryWriteEncodedByte(value, position, disposing) == true)
            {
                if (value == 0xFF)
                {
                    this.BaseStream.WriteByte(0);
                }

                return true;
            }
            else
            {
                return false;
            }

        }

        public void SetCodeTable(Dictionary<byte, HuffmanCode> codeTable)
        {
            this.CodeTable = codeTable;
        }

    }

}
