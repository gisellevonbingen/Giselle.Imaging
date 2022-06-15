using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public class MergedStream : Stream
    {
        protected byte[] PrefixBytes { get; }
        protected Stream BaseStream { get; }
        protected bool LeaveOpen { get; }

        private long _Position;
        public override long Position { get => this._Position; set => throw new NotSupportedException(); }

        public MergedStream(byte[] prefixBytes, Stream baseStream) : this(prefixBytes, baseStream, false)
        {

        }

        public MergedStream(byte[] prefixBytes, Stream baseStream, bool leaveOpen)
        {
            this.PrefixBytes = prefixBytes;
            this.BaseStream = baseStream;
            this.LeaveOpen = leaveOpen;

            this._Position = 0L;
        }

        public override bool CanRead => this.BaseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();


        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var prefixReading = (int)Math.Max(count - this.Position, count);
            count -= prefixReading;

            var length = 0;

            if (prefixReading > 0)
            {
                for (var i = 0; i < prefixReading; i++)
                {
                    buffer[offset++] = this.PrefixBytes[this._Position++];
                }

                length += prefixReading;
            }

            if (count > 0)
            {
                var streamReading = this.BaseStream.Read(buffer, offset, count);
                this._Position += streamReading;
                length += streamReading;
            }

            return length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

    }

}
