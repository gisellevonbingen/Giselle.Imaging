using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public class SiphonBlock : IDisposable
    {
        public Stream BaseStream { get; }
        public int Origin { get; }
        public int Length { get; }

        private readonly MemoryStream Memory;

        public SiphonStream Siphon { get; }

        public SiphonBlock(Stream stream, int origin, int length)
        {
            this.BaseStream = stream;
            this.Origin = origin;
            this.Length = length;

            if (stream.CanSeek == false)
            {
                var buffer = new byte[length];
                stream.Read(buffer, 0, length);
                this.Memory = new MemoryStream(buffer);
                this.Siphon = new SiphonStream(this.Memory, length, true, true);
            }
            else
            {
                this.Siphon = new SiphonStream(stream, length, true, true);
                stream.Seek(length, SeekOrigin.Current);
            }

        }

        public void SetPosition(long positionFromOrigin)
        {
            this.Siphon.Position = positionFromOrigin - this.Origin;
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Siphon.Dispose();

            if (this.Memory != null)
            {
                this.Memory.Dispose();
            }

            if(this.BaseStream.CanSeek == true)
            {
                this.BaseStream.Position = this.Origin + this.Length;
            }

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        ~SiphonBlock()
        {
            this.Dispose(false);
        }

    }

}
