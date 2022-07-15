using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Bmp
{
    public class BmpCodec : ImageCodec
    {
        public const bool IsLittleEndian = true;
        public const int SignatureLength = 2;

        public static BmpCodec Instance { get; } = new BmpCodec();
        public static byte[] SignatureBM { get; } = new byte[] { 0x42, 0x4D };
        public static byte[] SignatureBA { get; } = new byte[] { 0x42, 0x41 };
        public static byte[] SignatureCI { get; } = new byte[] { 0x43, 0x49 };
        public static byte[] SignatureCP { get; } = new byte[] { 0x43, 0x50 };
        public static byte[] SignatureIC { get; } = new byte[] { 0x49, 0x43 };
        public static byte[] SignaturePT { get; } = new byte[] { 0x50, 0x54 };
        public static IList<byte[]> Signatures { get; } = new List<byte[]> { SignatureBM, SignatureBA, SignatureCI, SignatureCP, SignatureIC, SignaturePT }.AsReadOnly();

        public static DataProcessor CreateBmpProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = IsLittleEndian };

        public BmpCodec()
        {

        }

        public override int BytesForTest => SignatureLength;

        public override bool SupportMultiFrame => false;

        public override IEnumerable<string> GetExtensions()
        {
            yield return "bmp";
            yield return "dib";
            yield return "rle";
        }

        protected override bool TestAsBytes(byte[] bytes, int offset, int count) => Signatures.Any(s => bytes.StartsWith(s, offset));

        public override ImageArgb32Container Read(Stream input)
        {
            var raw = new BmpRawImage(input);
            return new ImageArgb32Container(raw.Decode());
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions _options)
        {
            var frame = container.FirstOrDefault();
            var options = _options.CastOrDefault<BmpSaveOptions>();
            var raw = new BmpRawImage(frame, options);
            raw.Write(output);
        }

    }

}
