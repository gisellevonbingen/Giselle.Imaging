using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public class SiphonStream : InternalStream
    {
        private readonly long _Length = 0L;
        public override long Length => this._Length;

        public SiphonStream(Stream baseStream, long length, bool readingMode) : this(baseStream, length, readingMode, false)
        {

        }

        public SiphonStream(Stream baseStream, long length, bool readingMode, bool leaveOpen) : base(baseStream, readingMode, leaveOpen)
        {
            this._Length = length;
        }

    }

}
