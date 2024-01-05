using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.Codec.Tiff;
using Giselle.Imaging.Codec.Jpeg;
using Giselle.Imaging.Codec.Ico;
using Giselle.Imaging.Codec.Ani;
using Giselle.Imaging.Codec.Tga;
using Streams.IO;
using Giselle.Imaging.Codec.Gif;

namespace Giselle.Imaging.Codec
{
    public static class ImageCodecs
    {
        private static readonly List<ImageCodec> Codecs = new();

        public static IEnumerable<ImageCodec> GetCodecs() => Codecs;

        public static C Register<C>(C codec) where C : ImageCodec
        {
            Codecs.Add(codec);
            return codec;
        }

        static ImageCodecs()
        {
            Register(BmpCodec.Instance);
            Register(PngCodec.Instance);
            Register(TiffCodec.Instance);
            Register(JpegCodec.Instance);
            Register(IcoCodec.Instance);
            Register(AniCodec.Instance);
            Register(GifCodec.Instance);
            Register(TgaCodec.Instance);
        }

        public static ImageCodec FindCodec(byte[] bytes) => FindCodec(bytes, 0, bytes.Length);

        public static ImageCodec FindCodec(byte[] bytes, int offset, int count) => GetCodecs().FirstOrDefault(c => c.Test(bytes, offset, count));

        public static ImageCodec FindCodec(Stream input)
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
                        var buffer = new byte[num];
                        var prev = input.Position;
                        var readLength = input.Read(buffer, 0, buffer.Length);
                        input.Position = prev;
                        ms.Position = 0L;
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

        public static ImageArgb32Container FromSiphonBlock(SiphonBlock siphonBlock)
        {
            var stream = siphonBlock.SiphonStream;

            foreach (var codec in GetCodecs())
            {
                if (codec.Test(stream) == true)
                {
                    return codec.Read(stream);
                }

            }

            throw new IOException("Codec Not Found");
        }

        public static ImageArgb32Container FromStream(Stream input, long length)
        {
            using var siphonBlock = SiphonBlock.ByLength(input, length);
            return FromSiphonBlock(siphonBlock);
        }

        public static ImageArgb32Container FromStream(Stream input)
        {
            using var siphonBlock = SiphonBlock.ByRemain(input);
            return FromSiphonBlock(siphonBlock);
        }

        public static ImageArgb32Container FromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            return FromStream(ms);
        }

        public static ImageArgb32Container FromBytes(byte[] bytes, int offset, int count)
        {
            using var ms = new MemoryStream(bytes, offset, count);
            return FromStream(ms);
        }

    }

}
