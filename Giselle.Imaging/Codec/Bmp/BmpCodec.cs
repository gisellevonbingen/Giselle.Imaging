using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Bmp
{
    public class BmpCodec : ImageCodec<BmpEncodeOptions>
    {
        public static BmpCodec Instance { get; } = new BmpCodec();
        public static IList<byte> SignatureBM { get; } = Array.AsReadOnly(new byte[] { 0x42, 0x4D });
        public static IList<byte> SignatureBA { get; } = Array.AsReadOnly(new byte[] { 0x42, 0x41 });
        public static IList<byte> SignatureCI { get; } = Array.AsReadOnly(new byte[] { 0x43, 0x49 });
        public static IList<byte> SignatureCP { get; } = Array.AsReadOnly(new byte[] { 0x43, 0x50 });
        public static IList<byte> SignatureIC { get; } = Array.AsReadOnly(new byte[] { 0x49, 0x43 });
        public static IList<byte> SignaturePT { get; } = Array.AsReadOnly(new byte[] { 0x50, 0x54 });
        public static IList<IList<byte>> Signatures { get; } = Array.AsReadOnly(new IList<byte>[] { SignatureBM, SignatureBA, SignatureCI, SignatureCP, SignatureIC, SignaturePT });

        public static DataProcessor CreateBmpProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = true };

        public BmpCodec()
        {

        }

        public override int BytesForTest => 2;

        public override bool Test(byte[] bytes) => Signatures.Any(s => bytes.StartsWith(s));

        public override ImageArgb32 Read(Stream input)
        {
            var processor = CreateBmpProcessor(input);

            // Signature
            var signature = processor.ReadBytes(BytesForTest);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            // File Header
            var raw = new BmpRawImage();
            raw.Read(processor);
            var image = raw.Decode();
            return image;
        }

        public override void Write(Stream output, ImageArgb32 image, BmpEncodeOptions options)
        {
            var processor = CreateBmpProcessor(output);
            processor.WriteBytes(SignatureBM);

            var raw = new BmpRawImage();
            raw.Encode(image, options);
            raw.Write(processor);
        }

    }

}
