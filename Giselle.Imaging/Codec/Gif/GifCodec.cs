using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifCodec : ImageCodec
    {
        public const bool IsLittleEndian = true;

        public static GifCodec Instance { get; } = new GifCodec();

        public static DataProcessor CreateGifProcessor(Stream stream) => new(stream) { IsLittleEndian = IsLittleEndian };

        public GifCodec()
        {

        }

        public override int BytesForTest => 3;

        public override bool SupportMultiFrame => true;

        public override IEnumerable<string> GetExtensions()
        {
            yield return "gif";
        }

        public override PixelFormat GetPreferredPixelFormat(ImageArgb32Frame frame) => PixelFormat.Format8bppIndexed;

        protected override bool TestAsBytes(byte[] bytes) => bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46;

        public override ImageArgb32Container Read(Stream input)
        {
            var rawContainer = new GifContainer(input);
            return rawContainer.Decode();
        }

        public override void Write(Stream output, ImageArgb32Container container, ISaveOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
