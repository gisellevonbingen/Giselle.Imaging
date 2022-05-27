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
        public enum PngChunkStreamMode : byte
        {
            Read = 0,
            Write = 1,
        }

        public const int TypeLength = 4;

        public override bool CanRead => this.Mode == PngChunkStreamMode.Read;

        public override bool CanSeek => false;

        public override bool CanWrite => this.Mode == PngChunkStreamMode.Write;

        public PngChunkName Name { get; }
        public string DisplayName => this.Name.ToDisplayString();
        public override long Length { get; }
        private long _Position = 0;
        public override long Position { get => this._Position; set => throw new NotSupportedException(); }
        public uint AccumulatingCRC { get; private set; }

        public PngChunkStreamMode Mode { get; private set; }
        public DataProcessor BaseProcessor { get; private set; }
        public bool IgnoreCRC { get; set; }
        protected bool InternalReading { get; set; }

        protected PngChunkStream()
        {
            this._Position = 0;
            this.AccumulatingCRC = CRCUtils.CRC32Seed;
            this.IgnoreCRC = false;
        }

        public PngChunkStream(DataProcessor input) : this()
        {
            this.Mode = PngChunkStreamMode.Read;
            this.BaseProcessor = input;
            this.Length = input.ReadInt();

            try
            {
                this.InternalReading = true;
                var processor = PngCodec.CreatePngProcessor(this);
                this.Name = (PngChunkName)processor.ReadInt();
            }
            finally
            {
                this.InternalReading = false;
            }

        }

        public PngChunkStream(DataProcessor output, PngChunkName name, int length) : this()
        {
            this.Mode = PngChunkStreamMode.Write;
            this.BaseProcessor = output;
            this.Length = length;
            output.WriteInt(length);

            try
            {
                this.InternalReading = true;
                var processor = PngCodec.CreatePngProcessor(this);
                this.Name = name;
                processor.WriteInt((int)name);
            }
            finally
            {
                this.InternalReading = false;
            }

        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.InternalReading == false)
            {
                count = Math.Min(count, (int)(this.Length - this.Position));
            }

            var length = this.BaseProcessor.Read(buffer, offset, count);

            if (this.InternalReading == false)
            {
                this._Position += length;
            }

            this.AccumulatingCRC = CRCUtils.AccumulateCRC32(this.AccumulatingCRC, buffer, offset, length);

            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.BaseProcessor.Write(buffer, offset, count);

            if (this.InternalReading == false)
            {
                this._Position += count;
            }

            this.AccumulatingCRC = CRCUtils.AccumulateCRC32(this.AccumulatingCRC, buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            var ccrc = CRCUtils.FinalizeCalculateCRC32(this.AccumulatingCRC);

            if (this.Mode == PngChunkStreamMode.Read)
            {
                var rcrc = this.BaseProcessor.ReadUInt();

                if (this.IgnoreCRC == false && ccrc != rcrc)
                {
                    throw new CRCException($"Read CRC are mismatch with Calculcated CRC - {rcrc} vs {ccrc}");
                }

            }
            else if (this.Mode == PngChunkStreamMode.Write)
            {
                this.BaseProcessor.WriteUInt(ccrc);
            }

        }

    }

}
