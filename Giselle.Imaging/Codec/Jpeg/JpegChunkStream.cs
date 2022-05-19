using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpegChunkStream : Stream
    {
        public enum JpegChunkStreamMode : byte
        {
            Read = 0,
            Write = 1,
        }

        public override bool CanRead => this.Mode == JpegChunkStreamMode.Read;

        public override bool CanSeek => false;

        public override bool CanWrite => this.Mode == JpegChunkStreamMode.Write;

        public JpegMarker Marker { get; }
        public override long Length { get; }
        private long _Position = 0;
        public override long Position { get => this._Position; set => throw new NotImplementedException(); }

        public JpegChunkStreamMode Mode { get; private set; }
        public DataProcessor BaseProcessor { get; private set; }

        protected JpegChunkStream()
        {
            this._Position = 0;
        }

        public JpegChunkStream(DataProcessor input) : this()
        {
            this.Mode = JpegChunkStreamMode.Read;
            this.BaseProcessor = input;
            this.Marker = (JpegMarker)input.ReadUShort();

            if (this.Marker == JpegMarker.SOI || this.Marker == JpegMarker.EOI)
            {
                this.Length = 0;
            }
            else
            {
                this.Length = input.ReadUShort() - 2;
            }

        }

        public JpegChunkStream(DataProcessor output, JpegMarker marker, ushort lengthWithoutSelf) : this()
        {
            this.Mode = JpegChunkStreamMode.Write;
            this.BaseProcessor = output;

            this.Marker = marker;
            output.WriteUShort((ushort)marker);

            if (this.Marker == JpegMarker.SOI || this.Marker == JpegMarker.EOI)
            {
                var length = (ushort)(lengthWithoutSelf + 2);
                this.Length = length;
                output.WriteUShort(length);
            }
            else
            {
                this.Length = 0;
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

            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.BaseProcessor.Write(buffer, offset, count);

            this._Position += count;
        }

    }

}
