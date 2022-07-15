using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Png
{
    public class PngCodec : ImageCodec
    {
        public const bool IsLittleEndian = false;
        public const int SignatureLength = 8;
        public static PngCodec Instance { get; } = new PngCodec();
        public static byte[] Signature { get; } = new byte[SignatureLength] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public static DataProcessor CreatePngProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = IsLittleEndian };

        public PngCodec()
        {

        }

        public override int BytesForTest => Signature.Length;

        public override bool SupportMultiFrame => false;

        public override IEnumerable<string> GetExtensions()
        {
            yield return "png";
        }

        protected override bool TestAsBytes(byte[] bytes, int offset, int count) => bytes.StartsWith(Signature, offset);

        public override ImageArgb32Container Read(Stream input)
        {
            var raw = new PngRawImage(input);
            return new ImageArgb32Container(raw.Decode());
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions _options)
        {
            var frame = container.FirstOrDefault();
            var options = _options.CastOrDefault<PngSaveOptions>();
            var raw = new PngRawImage(frame, options);
            raw.Write(output);
        }

    }

}
