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
    public class PngChunkStream : InternalStream
    {
        public const int TypeLength = 4;

        public PngChunkName Name { get; }
        public string DisplayName => this.Name.ToDisplayString();
        public uint AccumulatingCRC { get; private set; }

        public bool IgnoreCRC { get; set; }
        protected bool InternalReading { get; set; }

        protected PngChunkStream(Stream baseStream, bool readingMode, bool leaveOpen) : base(baseStream, readingMode, leaveOpen)
        {
            this.AccumulatingCRC = CRCUtils.CRC32Seed;
            this.IgnoreCRC = false;
        }

        public PngChunkStream(Stream input) : this(input, true, true)
        {
            this.Length = PngCodec.CreatePngProcessor(input).ReadInt();

            try
            {
                this.InternalReading = true;
                this.Name = (PngChunkName)PngCodec.CreatePngProcessor(this).ReadInt();
            }
            finally
            {
                this.InternalReading = false;
            }

        }

        public PngChunkStream(Stream output, PngChunkName name, int length) : this(output, false, true)
        {
            this.Length = length;
            PngCodec.CreatePngProcessor(output).WriteInt(length);

            try
            {
                this.InternalReading = true;
                this.Name = name;
                PngCodec.CreatePngProcessor(this).WriteInt((int)name);
            }
            finally
            {
                this.InternalReading = false;
            }

        }

        public override long Length { get; }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int length;

            if (this.InternalReading == true)
            {
                length = this.BaseStream.Read(buffer, offset, count);
            }
            else
            {
                length = base.Read(buffer, offset, count);
            }

            this.AccumulatingCRC = CRCUtils.AccumulateCRC32(this.AccumulatingCRC, buffer, offset, length);

            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.InternalReading == true)
            {
                this.BaseStream.Write(buffer, offset, count);
            }
            else
            {
                base.Write(buffer, offset, count);

            }

            this.AccumulatingCRC = CRCUtils.AccumulateCRC32(this.AccumulatingCRC, buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            var ccrc = CRCUtils.FinalizeCalculateCRC32(this.AccumulatingCRC);
            var baseProcessor = PngCodec.CreatePngProcessor(this.BaseStream);

            if (this.ReadingMode == true)
            {
                var rcrc = baseProcessor.ReadUInt();

                if (this.IgnoreCRC == false && ccrc != rcrc)
                {
                    throw new CRCException($"Read CRC are mismatch with Calculcated CRC - {rcrc} vs {ccrc}");
                }

            }
            else
            {
                baseProcessor.WriteUInt(ccrc);
            }

            base.Dispose(disposing);
        }

    }

}
