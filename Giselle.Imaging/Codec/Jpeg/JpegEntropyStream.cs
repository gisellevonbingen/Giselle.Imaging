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
                base.ReadEncodedByte();
            }

            return b;
        }

        protected override void WriteEncodedByte(byte value)
        {
            base.WriteEncodedByte(value);

            if (value == 0xFF)
            {
                base.WriteEncodedByte(0);
            }

        }

        public void SetCodeTable(Dictionary<byte, HuffmanCode> codeTable)
        {
            this.CodeTable = codeTable;
        }

    }

}
