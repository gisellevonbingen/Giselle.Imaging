using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Riff
{
    public class RiffChunkStream : InternalStream
    {
        public RiffChunkHeader Header { get; }
        private readonly int _Length;

        public RiffChunkStream(Stream output, RiffChunkHeader header) : base(output, false, true)
        {
            var processor = RiffChunk.CreateRiffDataProcessor(output);
            processor.WriteInt(header.TypeKey);
            processor.WriteInt(this._Length = header.Length);
            this.Header = header;
        }

        public RiffChunkStream(Stream input) : base(input, true, true)
        {
            var processor = RiffChunk.CreateRiffDataProcessor(input);
            var typeKey = processor.ReadInt();
            var length = processor.ReadInt();
            this.Header = new RiffChunkHeader(typeKey, length);
            this._Length = length;
        }

        public override long Length => this._Length;

        public override void SetLength(long value) => throw new NotSupportedException();

    }

}
