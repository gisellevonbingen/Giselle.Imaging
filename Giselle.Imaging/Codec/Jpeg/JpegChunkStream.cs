using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpegChunkStream : InternalStream
    {
        public JpegMarker Marker { get; }

        public JpegChunkStream(Stream input) : this(input, false)
        {

        }

        public JpegChunkStream(Stream input, bool leaveOpen) : base(input, true, leaveOpen)
        {
            var processor = JpegCodec.CreateJpegProcessor(input);
            this.Marker = (JpegMarker)processor.ReadUShort();

            if (this.Marker == JpegMarker.SOI || this.Marker == JpegMarker.EOI)
            {
                this.Length = 0;
            }
            else
            {
                this.Length = processor.ReadUShort() - 2;
            }

        }

        public JpegChunkStream(Stream output, JpegMarker marker, ushort lengthWithoutSelf) : this(output, marker, lengthWithoutSelf, false)
        {

        }

        public JpegChunkStream(Stream output, JpegMarker marker, ushort lengthWithoutSelf, bool leaveOpen) : base(output, false, leaveOpen)
        {
            var processor = JpegCodec.CreateJpegProcessor(output);
            this.Marker = marker;
            processor.WriteUShort((ushort)marker);

            if (this.Marker == JpegMarker.SOI || this.Marker == JpegMarker.EOI)
            {
                var length = (ushort)(lengthWithoutSelf + 2);
                this.Length = length;
                processor.WriteUShort(length);
            }
            else
            {
                this.Length = 0;
            }

        }

        public override long Length { get; }

        public override void SetLength(long value) => throw new NotSupportedException();

    }

}
