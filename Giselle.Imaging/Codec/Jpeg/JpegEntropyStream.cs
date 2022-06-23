using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Algorithms.Huffman;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpegEntropyStream : AbstractHuffmanStream
    {
        private Dictionary<byte, HuffmanCode> CodeTable;

        public JpegEntropyStream(Stream baseStream) : this(baseStream, false)
        {

        }

        public JpegEntropyStream(Stream baseStream, bool leaveOpen) : base(new JpegEntropyUnaryStream(baseStream, leaveOpen), leaveOpen)
        {
            this.CodeTable = new Dictionary<byte, HuffmanCode>();
        }

        protected override Dictionary<byte, HuffmanCode> NextReadingCodes() => this.CodeTable;

        protected override Dictionary<byte, HuffmanCode> NextWritingCodes() => this.CodeTable;

        public int ReadBit() => this.BitBaseStream.ReadBit();

        public void WriteBit(int bit) => this.BitBaseStream.WriteBit(bit);

        public void SetCodeTable(Dictionary<byte, HuffmanCode> codeTable)
        {
            this.CodeTable = codeTable;
        }

        private class JpegEntropyUnaryStream : WrappedByteStream
        {
            public JpegEntropyUnaryStream(Stream baseStream) : base(baseStream)
            {

            }

            public JpegEntropyUnaryStream(Stream baseStream, bool leaveOpen) : base(baseStream, leaveOpen)
            {

            }

            public override int ReadByte()
            {
                var b = this.BaseStream.ReadByte();

                if (b == 0xFF)
                {
                    // Must be zero
                    this.BaseStream.ReadByte();
                }

                return b;
            }

            public override void WriteByte(byte value)
            {
                this.BaseStream.WriteByte(value);

                if (value == 0xFF)
                {
                    this.BaseStream.WriteByte(0);
                }

            }

            public override long Length => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();
        }

    }

}
