using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public abstract class InternalStream : WrappedStream
    {
        protected bool ReadingMode { get; }

        public InternalStream(Stream baseStream, bool readingMode) : this(baseStream, readingMode, false)
        {

        }

        public InternalStream(Stream baseStream, bool readingMode, bool leaveOpen) : base(baseStream, leaveOpen)
        {
            this.ReadingMode = readingMode;
        }

        protected override long InternalPosition
        {
            get => base.InternalPosition;
            set
            {
                if (this.TryGetLength(out var length) == false)
                {
                    base.InternalPosition = value;
                }
                else if (0 <= value && value <= length)
                {
                    base.InternalPosition = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }

            }

        }

        public override bool CanRead => base.CanRead && this.ReadingMode == true;

        public override bool CanWrite => base.CanWrite && this.ReadingMode == false;

        public override bool CanSeek => base.CanSeek;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readingCount;

            if (this.TryGetLength(out var length) == true)
            {
                readingCount = (int)Math.Min(length - this.Position, count);
            }
            else
            {
                readingCount = count;
            }

            return base.Read(buffer, offset, readingCount);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int writingCount;

            if (this.TryGetLength(out var length) == true)
            {
                writingCount = (int)Math.Min(length - this.Position, count);
            }
            else
            {
                writingCount = count;
            }

            if (writingCount > 0)
            {
                base.Write(buffer, offset, writingCount);
            }

        }

    }

}
