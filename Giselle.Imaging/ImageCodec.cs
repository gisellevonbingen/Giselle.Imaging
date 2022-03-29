﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Bmp;
using Giselle.Imaging.IO;
using Giselle.Imaging.Tiff;

namespace Giselle.Imaging
{
    public interface IImageCodec
    {
        int BytesForTest { get; }

        bool Test(byte[] bytes);

        bool Test(Stream stream);

        Image32Argb Read(byte[] input);

        Image32Argb Read(Stream input);

        void Write(Stream output, Image32Argb data);

        void Write(Stream output, Image32Argb image, EncodeOptions options);

    }

    public interface IImageCodec<in T> : IImageCodec where T : EncodeOptions, new()
    {
        void Write(Stream output, Image32Argb image, T options);
    }

    public abstract class ImageCodec : IImageCodec
    {
        private static readonly List<IImageCodec> Codecs = new List<IImageCodec>();

        public static IEnumerable<IImageCodec> GetCodecs() => Codecs;

        public static C Register<C>(C codec) where C : IImageCodec
        {
            Codecs.Add(codec);
            return codec;
        }

        public static IImageCodec FindCodec(byte[] bytes) => GetCodecs().FirstOrDefault(c => c.Test(bytes));

        static ImageCodec()
        {
            Register(BmpCodec.Instance);
            Register(TiffCodec.Instance);
        }

        public static IImageCodec FindCodec(Stream input)
        {
            if (input.CanSeek == false)
            {
                throw new ArithmeticException("Input require CanSeek");
            }

            using (var ms = new MemoryStream())
            {
                foreach (var codec in GetCodecs())
                {
                    var num = codec.BytesForTest;

                    if (num > ms.Length)
                    {
                        var buffer = new byte[num - ms.Length];
                        var prev = input.Position;
                        var readLength = input.Read(buffer, 0, buffer.Length);
                        input.Position = prev;
                        ms.Position = ms.Length;
                        ms.Write(buffer, 0, readLength);

                        if (buffer.Length != readLength)
                        {
                            continue;
                        }

                    }

                    ms.Position = 0;

                    if (codec.Test(ms) == false)
                    {
                        continue;
                    }

                    return codec;
                }

            }

            return null;
        }

        public static Image32Argb FromBytes(byte[] bytes) => FindCodec(bytes)?.Read(bytes);

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

        public Image32Argb Read(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return this.Read(ms);
            }

        }

        public abstract Image32Argb Read(Stream input);

        public abstract void Write(Stream output, Image32Argb image);

        public abstract void Write(Stream output, Image32Argb image, EncodeOptions options);
    }

    public abstract class ImageCodec<T> : ImageCodec, IImageCodec<T> where T : EncodeOptions, new()
    {
        public override void Write(Stream output, Image32Argb image) => this.Write(output, image, new T());

        public abstract void Write(Stream output, Image32Argb image, T options);

        public override void Write(Stream output, Image32Argb image, EncodeOptions options) => this.Write(output, image, (options as T) ?? new T());
    }

}
