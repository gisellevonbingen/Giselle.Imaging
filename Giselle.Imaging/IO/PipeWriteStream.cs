using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public class PipeWriteStream : Stream
    {
        private readonly List<byte> Buffer;
        private readonly AutoResetEvent ResetEvent;

        public PipeWriteStream(List<byte> buffer, AutoResetEvent resetEvent)
        {
            this.Buffer = buffer;
            this.ResetEvent = resetEvent;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => this.Buffer.Count;

        public override long Position { get => this.Buffer.Count; set => throw new NotSupportedException(); }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (this.Buffer)
            {
                for (var i = 0; i < count; i++)
                {
                    this.Buffer.Add(buffer[offset + i]);
                }

            }

            this.ResetEvent.Set();
        }

    }

}
