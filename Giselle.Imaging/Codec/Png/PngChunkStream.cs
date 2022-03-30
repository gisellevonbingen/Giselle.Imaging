using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Checksum;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Png
{
    public class PngChunkStream : Stream
    {
        public enum PNGChunkStreamMode : byte
        {
            Read = 0,
            Write = 1,
        }

        public const int TypeLength = 4;

        public override bool CanRead => this.Mode == PNGChunkStreamMode.Read;

        public override bool CanSeek => false;

        public override bool CanWrite => this.Mode == PNGChunkStreamMode.Write;

        public string Type { get; }
        public override long Length { get; }
        private long _Position = 0;
        public override long Position { get => this._Position; set => throw new NotImplementedException(); }
        public uint AccumulatingCRC { get; private set; }

        public PNGChunkStreamMode Mode { get; private set; }
        public DataProcessor BaseProcessor { get; private set; }
        public bool IgnoreCRC { get; set; }

        protected PngChunkStream()
        {
            this._Position = 0;
            this.AccumulatingCRC = CRCUtils.CRC32Seed;
            this.IgnoreCRC = false;
        }

        public PngChunkStream(DataProcessor input) : this()
        {
            this.Mode = PNGChunkStreamMode.Read;
            this.BaseProcessor = input;
            this.Length = input.ReadInt();

            var typeBytes = input.ReadBytes(TypeLength);
            this.Type = Encoding.ASCII.GetString(typeBytes);
            this.AccumulatingCRC = CRCUtils.AccumulateCRC32(this.AccumulatingCRC, typeBytes);
        }

        public PngChunkStream(DataProcessor output, string type) : this()
        {
            this.Mode = PNGChunkStreamMode.Write;
            this.BaseProcessor = output;
            this.Length = 0;

            var typeBytes = this.FixType(Encoding.ASCII.GetBytes(type));
            this.Type = Encoding.ASCII.GetString(typeBytes);
            this.AccumulatingCRC = CRCUtils.AccumulateCRC32(this.AccumulatingCRC, typeBytes);
            output.WriteBytes(typeBytes);
        }

        protected byte[] FixType(byte[] value)
        {
            if (value is null)
            {
                return new byte[TypeLength];
            }
            else if (value.Length == TypeLength)
            {
                return value.ToArray();
            }
            else
            {
                return value.TakeElse(TypeLength).ToArray();
            }

        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = Math.Min(count, (int)(this.Length - this.Position));
            var length = this.BaseProcessor.Read(buffer, offset, count);
            this._Position += length;
            this.AccumulatingCRC = CRCUtils.AccumulateCRC32(this.AccumulatingCRC, buffer, offset, length);

            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.BaseProcessor.Write(buffer, offset, count);

            this._Position += count;
            this.AccumulatingCRC = CRCUtils.AccumulateCRC32(this.AccumulatingCRC, buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.IgnoreCRC == true)
            {
                return;
            }

            var ccrc = CRCUtils.FinalizeCalculateCRC32(this.AccumulatingCRC);

            if (this.Mode == PNGChunkStreamMode.Read)
            {
                var rcrc = this.BaseProcessor.ReadUInt();

                if (ccrc != rcrc)
                {
                    throw new CRCException($"Read CRC are mismatch with Calculcated CRC - {rcrc} vs {ccrc}");
                }

            }
            else if (this.Mode == PNGChunkStreamMode.Write)
            {
                this.BaseProcessor.WriteUInt(ccrc);
            }

        }

    }

}
