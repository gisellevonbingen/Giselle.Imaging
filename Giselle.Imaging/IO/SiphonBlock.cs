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

        public SiphonStream SiphonSteam { get; }

        public static SiphonBlock ByLength(Stream baseStream, int length)
        {
            if (baseStream.CanSeek == false)
            {
                return ByLength(baseStream, 0, length);
            }
            else
            {
                var origin = baseStream.Position;
                return ByLength(baseStream, (int)origin, length);
            }

        }

        public static SiphonBlock ByLength(Stream baseStream, int blockOrigin, int length)
        {
            if (baseStream.CanSeek == false)
            {
                var buffer = new byte[length];
                baseStream.Read(buffer, 0, length);
                var memory = new MemoryStream(buffer);
                var siphon = new SiphonStream(memory, length, true, true);
                return new SiphonBlock(baseStream, siphon, blockOrigin, memory);
            }
            else
            {
                var siphon = new SiphonStream(baseStream, length, true, true);
                return new SiphonBlock(baseStream, siphon, blockOrigin);
            }

        }

        public static SiphonBlock ByRemain(Stream baseStream)
        {
            if (baseStream.CanSeek == false)
            {
                return ByRemain(baseStream, 0);
            }
            else
            {
                var origin = baseStream.Position;
                return ByRemain(baseStream, (int)origin);
            }

        }

        public static SiphonBlock ByRemain(Stream baseStream, int blockOrigin)
        {
            if (baseStream.CanSeek == false)
            {
                var memory = new MemoryStream();
                baseStream.CopyTo(memory);
                var siphon = new SiphonStream(memory, memory.Length, true, true);
                return new SiphonBlock(baseStream, siphon, blockOrigin, memory);
            }
            else
            {
                var length = baseStream.GetRemain();
                var siphon = new SiphonStream(baseStream, length, true, true);
                return new SiphonBlock(baseStream, siphon, blockOrigin);
            }

        }

        private SiphonBlock(Stream baseStream, SiphonStream siphonStream, int origin)
        {
            this.BaseStream = baseStream;
            this.Origin = origin;
            this.Length = (int)siphonStream.Length;

            this.Memory = null;
            this.SiphonSteam = siphonStream;
        }

        private SiphonBlock(Stream baseStream, SiphonStream siphonStream, int origin, MemoryStream memoryStream) : this(baseStream, siphonStream, origin)
        {
            this.Memory = memoryStream;
        }

        public void SetBasePosition(long positionFromOrigin)
        {
            this.SiphonSteam.Position = positionFromOrigin - this.Origin;
        }

        protected virtual void Dispose(bool disposing)
        {
            this.SiphonSteam.Dispose();

            if (this.Memory != null)
            {
                this.Memory.Dispose();
            }

            if (this.BaseStream.CanSeek == true)
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
