using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaCodec : ImageCodec
    {
        public const bool IsLittleEndian = true;

        public static TgaCodec Instance { get; } = new TgaCodec();

        public static DataProcessor CreateTgaProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = IsLittleEndian };

        public TgaCodec()
        {

        }

        public override int BytesForTest => TgaRawHeader.Length;

        public override bool SupportMultiFrame => false;

        public override string PrimaryExtension => "tga";

        public override IEnumerable<string> GetExtensions()
        {
            yield return PrimaryExtension;
            yield return "icb";
            yield return "vda";
            yield return "vst";
        }

        protected override bool TestAsBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var header = new TgaRawHeader(CreateTgaProcessor(ms));
                return header.Width > 0 && header.Height > 0 && header.PixelDepth > 0;
            }

        }

        public override ImageArgb32Container Read(Stream input)
        {
            var raw = new TgaRawImage(input);
            return new ImageArgb32Container(raw.Decode());
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions options)
        {
            var raw = new TgaRawImage(container.FirstOrDefault(), options.CastOrDefault<TgaSaveOptions>());
            raw.Write(output);
        }

    }

}
