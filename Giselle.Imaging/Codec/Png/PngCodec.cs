using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Streams.IO;

namespace Giselle.Imaging.Codec.Png
{
    public class PngCodec : ImageCodec
    {
        public const bool IsLittleEndian = false;
        public const int SignatureLength = 8;
        public static PngCodec Instance { get; } = new PngCodec();
        public static byte[] Signature { get; } = new byte[SignatureLength] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        public static Encoding Encoding => Encoding.ASCII;

        public static DataProcessor CreatePngProcessor(Stream stream) => new(stream) { IsLittleEndian = IsLittleEndian };

        public PngCodec()
        {

        }

        public override int BytesForTest => Signature.Length;

        public override bool SupportMultiFrame => false;

        public override IEnumerable<string> GetExtensions()
        {
            yield return "png";
        }

        protected override bool TestAsBytes(byte[] bytes) => bytes.StartsWith(Signature);

        public override ImageArgb32Container Read(Stream input)
        {
            var raw = new PngRawImage(input);
            return new ImageArgb32Container(raw.Decode());
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions _options)
        {
            var frame = container.First();
            var options = _options.CastOrDefault<PngSaveOptions>();
            var raw = new PngRawImage(frame, options);
            raw.Write(output);
        }

        public override PixelFormat GetPreferredPixelFormat(ImageArgb32Frame frame)
        {
            var report = frame.GetPreferredIndexedPixelFormat(true, this.GetSupportIndexedPixelFormats());

            if (report.IndexedPixelFormat == PixelFormat.Undefined)
            {
                return report.HasAlpha ? PixelFormat.Format32bppArgb8888 : PixelFormat.Format24bppRgb888;
            }
            else
            {
                return report.IndexedPixelFormat;
            }

        }

        public override IEnumerable<PixelFormat> GetSupportIndexedPixelFormats()
        {
            yield return PixelFormat.Format1bppIndexed;
            yield return PixelFormat.Format2bppIndexed;
            yield return PixelFormat.Format4bppIndexed;
            yield return PixelFormat.Format8bppIndexed;
        }

    }

}
