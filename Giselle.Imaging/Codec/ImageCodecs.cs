using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.Codec.Jpeg;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.Codec.Tiff;

namespace Giselle.Imaging.Codec
{
    public static class ImageCodecs
    {
        private static readonly List<IImageCodec> Codecs = new List<IImageCodec>();

        public static IEnumerable<IImageCodec> GetCodecs() => Codecs;

        public static C Register<C>(C codec) where C : IImageCodec
        {
            Codecs.Add(codec);
            return codec;
        }

        public static IImageCodec FindCodec(byte[] bytes) => GetCodecs().FirstOrDefault(c => c.Test(bytes));

        static ImageCodecs()
        {
            Register(BmpCodec.Instance);
            Register(TiffCodec.Instance);
            Register(PngCodec.Instance);
            Register(JpegCodec.Instance);
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

        public static IRawImage FromStream(Stream input) => FindCodec(input).Read(input);

        public static IRawImage FromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return FromStream(ms);
            }

        }

    }

}
