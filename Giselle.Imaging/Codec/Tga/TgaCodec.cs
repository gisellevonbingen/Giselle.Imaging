using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaCodec : ImageCodec
    {
        public const bool IsLittleEndian = true;

        public static TgaCodec Instance { get; } = new TgaCodec();

        public static Encoding Encoding => Encoding.ASCII;

        public static DataProcessor CreateTgaProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = IsLittleEndian };

        public TgaCodec()
        {

        }

        public override int BytesForTest => TgaRawHeader.Length;

        public override bool SupportMultiFrame => false;

        public override IEnumerable<string> GetExtensions()
        {
            yield return "tga";
            yield return "icb";
            yield return "vda";
            yield return "vst";
        }

        protected override bool TestAsStream(Stream stream)
        {
            var header = new TgaRawHeader(CreateTgaProcessor(stream));
            return header.Width > 0 && header.Height > 0 && header.PixelDepth > 0;
        }

        public override ImageArgb32Container Read(Stream input)
        {
            var raw = new TgaRawImage(input);
            return new ImageArgb32Container(raw.Decode());
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions _options)
        {
            var frame = container.First();
            var options = _options.CastOrDefault<TgaSaveOptions>();
            var raw = new TgaRawImage(frame, options);
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
            yield return PixelFormat.Format8bppIndexed;
        }

    }

}
