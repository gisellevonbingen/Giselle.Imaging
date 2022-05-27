using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public class PipeReadStream : Stream
    {
        private readonly List<byte> Buffer;
        private readonly AutoResetEvent ResetEvent;

        private int _Position;

        public PipeReadStream(List<byte> buffer, AutoResetEvent resetEvent)
        {
            this.Buffer = buffer;
            this.ResetEvent = resetEvent;

            this._Position = 0;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => this._Position; set => throw new NotSupportedException(); }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        private int Read0(byte[] buffer, int offset, int count)
        {
            lock (this.Buffer)
            {
                var available = Math.Min(this.Buffer.Count - this._Position, count);

                if (available > 0)
                {
                    for (var i = 0; i < available; i++)
                    {
                        buffer[offset + i] = this.Buffer[this._Position++];
                    }

                }

                return available;
            }

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readingOffset = offset;
            var readingCount = count;
            var length = 0;

            while (true)
            {
                length += this.Read0(buffer, readingOffset, readingCount);
                readingOffset = offset + length;
                readingCount = count - length;

                if (length == count)
                {
                    break;
                }

                this.ResetEvent.WaitOne();
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
