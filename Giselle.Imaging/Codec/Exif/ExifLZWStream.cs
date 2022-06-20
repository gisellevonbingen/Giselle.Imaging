using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Algorithms.LZW;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifLZWStream : WrappedByteStream
    {
        public const int ClearCode = 256;
        public const int EoiCode = 257;
        public const int TableOffset = 2;

        public static int GetUsingBits(int value)
        {
            if (value < 512) return 9;
            if (value < 1024) return 10;
            if (value < 2048) return 11;
            return 12;
        }

        protected readonly BitStream BaseBitStream;
        protected readonly ExifLZWCompressionMode Mode;
        protected readonly LZWProcessor Processor;

        protected int ReadingDataKey { get; private set; } = -1;
        protected IReadOnlyList<byte> ReadingData { get; private set; } = new byte[0];
        protected int ReadingPosition { get; private set; } = 0;

        public ExifLZWStream(Stream baseStream, ExifLZWCompressionMode mode) : this(baseStream, mode, false)
        {

        }

        public ExifLZWStream(Stream baseStream, ExifLZWCompressionMode mode, bool leaveOpen) : base(baseStream, leaveOpen)
        {
            if (mode == ExifLZWCompressionMode.Compress)
            {
                if (baseStream.CanWrite == false)
                {
                    throw new ArgumentException("BaseStream can't write", nameof(baseStream));
                }

            }
            else if (mode == ExifLZWCompressionMode.Decompress)
            {
                if (baseStream.CanRead == false)
                {
                    throw new ArgumentException("BaseStream can't read", nameof(baseStream));
                }

            }

            this.BaseBitStream = new BitStream(baseStream, leaveOpen);
            this.Mode = mode;
            this.Processor = new LZWProcessor(TableOffset);

            if (mode == ExifLZWCompressionMode.Compress)
            {
                this.WriteKey(ClearCode);
            }

        }

        public override bool CanRead => base.CanRead && this.Mode == ExifLZWCompressionMode.Decompress;

        public override bool CanWrite => base.CanWrite && this.Mode == ExifLZWCompressionMode.Compress;

        protected int ReadCode()
        {
            var nextKey = this.Processor.NextKey;
            var bits = GetUsingBits(nextKey + 1);
            var code = 0;

            for (var i = 0; i < bits; i++)
            {
                var b = this.BaseBitStream.ReadBit();

                if (b == -1)
                {
                    return EoiCode;
                }

                var shift = bits - i - 1;
                code |= b << shift;
            }

            return code;
        }

        protected void WriteKey(int key)
        {
            var nextKey = this.Processor.NextKey;
            var bits = GetUsingBits(nextKey - 1);

            for (var i = 0; i < bits; i++)
            {
                var shift = bits - i - 1;
                var mask = 1 << shift;
                var bit = (key & mask) >> shift;
                this.BaseBitStream.WriteBit(bit);
            }

        }

        protected bool FetchKey()
        {
            var code = this.ReadCode();

            if (code == EoiCode)
            {
                this.ReadingDataKey = code;
                return false;
            }
            else if (code == ClearCode)
            {
                this.Processor.ClearTable();
                var code2 = this.ReadCode();
                var key = this.Processor.Decode(code2);
                this.ReadingDataKey = key;
                return code2 != EoiCode;
            }
            else
            {
                this.ReadingDataKey = this.Processor.Decode(code);
                return true;
            }

        }

        public override int ReadByte()
        {
            if (this.ReadingDataKey == EoiCode)
            {
                return -1;
            }
            else if (this.ReadingPosition >= this.ReadingData.Count)
            {
                if (this.FetchKey() == false)
                {
                    return -1;
                }
                else
                {
                    this.ReadingPosition = 0;
                    this.ReadingData = this.Processor.Table[this.ReadingDataKey].Values;
                }

            }

            var data = this.ReadingData[this.ReadingPosition++];
            return data;
        }

        public override void WriteByte(byte value)
        {
            this.WriteCode(value);
        }

        protected void WriteCode(int value)
        {
            var key = this.Processor.Encode(value);

            if (key > -1)
            {
                this.WriteKey(key);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (this.Mode == ExifLZWCompressionMode.Compress)
            {
                this.WriteCode(-1);
                this.WriteKey(EoiCode);
            }

            this.BaseBitStream.Dispose();

            base.Dispose(disposing);
        }

    }

}
