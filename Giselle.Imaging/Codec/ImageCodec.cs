using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec
{
    public interface IImageCodec
    {
        int BytesForTest { get; }

        bool Test(byte[] bytes);

        bool Test(Stream stream);

        IRawImage CreateEmpty();

        IRawImage Read(Stream input);

        void Write(Stream output, IRawImage image);

    }

    public interface IImageCodec<T> : IImageCodec where T : IRawImage, new()
    {
        new T CreateEmpty();

        new T Read(Stream output);

        void Write(Stream output, T image);
    }

    public abstract class ImageCodec<T> : IImageCodec<T> where T : IRawImage, new()
    {
        public abstract int BytesForTest { get; }

        public virtual bool Test(Stream stream)
        {
            if (stream.CanSeek == false)
            {
                return false;
            }

            var start = stream.Position;

            try
            {
                var bytes = new byte[this.BytesForTest];
                var prev = stream.Position;
                var len = stream.Read(bytes, 0, bytes.Length);
                stream.Position = prev;

                if (len != bytes.Length)
                {
                    return false;
                }

                return this.Test(bytes);
            }
            finally
            {
                stream.Seek(start, SeekOrigin.Begin);
            }

        }

        public abstract bool Test(byte[] bytes);

        public T CreateEmpty() => new T();

        IRawImage IImageCodec.CreateEmpty() => this.CreateEmpty();

        public abstract T Read(Stream input);

        IRawImage IImageCodec.Read(Stream input) => this.Read(input);

        public abstract void Write(Stream output, T image);

        void IImageCodec.Write(Stream output, IRawImage image) => this.Write(output, (T)image);
    }

}
