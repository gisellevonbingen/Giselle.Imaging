using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Streams.IO;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifCodec : ImageCodec
    {
        public const bool IsLittleEndian = true;
        public const int SignatureLength = 3;

        public static GifCodec Instance { get; } = new GifCodec();
        public static Encoding Encoding => Encoding.ASCII;
        /// <summary>
        /// GIF
        /// </summary>
        public static byte[] Signature { get; } = new byte[SignatureLength] { 0x47, 0x49, 0x46 };
        /// <summary>
        /// 89a
        /// </summary>
        public static byte[] EncoderVersion { get; } = new byte[SignatureLength] { 0x38, 0x39, 0x61 };

        public static DataProcessor CreateGifProcessor(Stream stream) => new(stream) { IsLittleEndian = IsLittleEndian };

        public GifCodec()
        {

        }

        public override int BytesForTest => Signature.Length;

        public override bool SupportMultiFrame => true;

        public override IEnumerable<string> GetExtensions()
        {
            yield return "gif";
        }

        public override PixelFormat GetPreferredPixelFormat(ImageArgb32Frame frame) => PixelFormat.Format8bppIndexed;

        protected override bool TestAsBytes(byte[] bytes) => bytes.StartsWith(Signature);

        public override ImageArgb32Container Read(Stream input)
        {
            var rawContainer = new GifContainer(input);
            return rawContainer.Decode();
        }

        public override void Write(Stream output, ImageArgb32Container container, ISaveOptions _options)
        {
            throw new NotImplementedException();

            var rawContainer = new GifContainer();
            var options = _options.CastOrDefault<GifSaveOptions>();
            rawContainer.Write(output);
        }

    }

}
