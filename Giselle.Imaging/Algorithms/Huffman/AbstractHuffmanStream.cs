using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Algorithms.Huffman
{
    public abstract class AbstractHuffmanStream : WrappedByteStream
    {
        protected virtual BitStream BitBaseStream { get; }
        protected override Stream BaseStream => this.BitBaseStream;

        protected AbstractHuffmanStream(Stream baseStream) : this(baseStream, false)
        {

        }

        protected AbstractHuffmanStream(Stream baseStream, bool leaveOpen) : base(null, leaveOpen)
        {
            this.BitBaseStream = new BitStream(baseStream, leaveOpen);
        }

        protected abstract Dictionary<byte, HuffmanCode> NextReadingCodes();

        protected abstract Dictionary<byte, HuffmanCode> NextWritingCodes();

        public override int ReadByte()
        {
            var rawCode = 0;
            var codes = this.NextReadingCodes();

            for (var i = 0; ; i++)
            {
                var bit = this.BitBaseStream.ReadBit();

                if (bit == -1)
                {
                    return bit;
                }

                rawCode = rawCode << 1 | bit;
                var length = i + 1;
                var looksMaxLength = 1;

                foreach (var pair in codes)
                {
                    var code = pair.Value;

                    if (code.Raw == rawCode && code.Length == length)
                    {
                        return pair.Key;
                    }
                    else if (code.Length > looksMaxLength)
                    {
                        looksMaxLength = code.Length;
                    }

                }

                if (length >= looksMaxLength)
                {
                    throw new ArgumentException($"RawCode 0x{rawCode:X2} is not exist in HuffmanTable");
                }

            }

        }

        public override void WriteByte(byte value)
        {
            var codes = this.NextWritingCodes();

            if (codes.TryGetValue(value, out var code) == false)
            {
                throw new ArgumentException($"Byte 0x{value:X2} is not exist in HuffmanTable");
            }

            for (var i = 0; i < code.Length; i++)
            {
                var shift = code.Length - 1 - i;
                var bitMask = 1 << shift;
                var bit = (code.Raw & bitMask) >> shift;
                this.BitBaseStream.WriteBit(bit);
            }

        }

        protected override void Dispose(bool disposing)
        {
            this.BitBaseStream.Dispose();

            base.Dispose(disposing);
        }

    }

}
