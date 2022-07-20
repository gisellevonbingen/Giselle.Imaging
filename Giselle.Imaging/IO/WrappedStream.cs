using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public class WrappedStream : Stream
    {
        protected virtual Stream BaseStream { get; }
        protected bool LeaveOpen { get; }
        protected long StartBasePosition { get; }
        private long _InternalPosition = 0L;
        protected virtual long InternalPosition
        {
            get => this._InternalPosition;
            set => this._InternalPosition = value;
        }

        public WrappedStream(Stream baseStream) : this(baseStream, false)
        {

        }

        public WrappedStream(Stream baseStream, bool leaveOpen)
        {
            this.BaseStream = baseStream;
            this.LeaveOpen = leaveOpen;

            try
            {
                this.StartBasePosition = baseStream.Position;
            }
            catch (Exception)
            {
                this.StartBasePosition = -1L;
            }

        }

        public override bool CanRead => this.BaseStream.CanRead;

        public override bool CanWrite => this.BaseStream.CanWrite;

        public override bool CanSeek => this.BaseStream.CanSeek;

        public override long Position
        {
            get => this.InternalPosition;
            set
            {
                if (this.CanSeek == false)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    this.InternalPosition = value;
                    this.BaseStream.Position = this.StartBasePosition + value;
                }

            }

        }

        public override long Length => throw new NotSupportedException();

        public override void Flush() => this.BaseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.CanRead == false)
            {
                throw new NotSupportedException();
            }

            var readLength = this.BaseStream.Read(buffer, offset, count);
            this.InternalPosition += count;
            return readLength;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.CanWrite == false)
            {
                throw new NotSupportedException();
            }

            this.BaseStream.Write(buffer, offset, count);
            this.InternalPosition += count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this.CanSeek == false)
            {
                throw new NotSupportedException();
            }
            else if (origin == SeekOrigin.Begin)
            {
                this.Position = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                this.Position += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                this.Position = this.Length - offset;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            return this.Position;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.LeaveOpen == false)
            {
                this.BaseStream.Dispose();
            }

        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

    }

}
