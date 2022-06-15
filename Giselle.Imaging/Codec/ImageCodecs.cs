using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.Codec.Exif;
using Giselle.Imaging.Codec.Tiff;
using Giselle.Imaging.Codec.Jpeg;

namespace Giselle.Imaging.Codec
{
    public static class ImageCodecs
    {
        private static readonly List<ImageCodec> Codecs = new List<ImageCodec>();

        public static IEnumerable<ImageCodec> GetCodecs() => Codecs;

        public static C Register<C>(C codec) where C : ImageCodec
        {
            Codecs.Add(codec);
            return codec;
        }

        public static ImageCodec FindCodec(byte[] bytes) => GetCodecs().FirstOrDefault(c => c.Test(bytes));

        static ImageCodecs()
        {
            Register(BmpCodec.Instance);
            Register(PngCodec.Instance);
            Register(TiffCodec.Instance);
            Register(JpegCodec.Instance);
        }

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

        public static ImageArgb32Container FromStream(Stream input) => FindCodec(input).Read(input);

        public static ImageArgb32Container FromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return FromStream(ms);
            }

        }

    }

}
