using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Png
{
    public class PngCodec : ImageCodec<PngEncodeOptions>
    {
        public static PngCodec Instance { get; } = new PngCodec();
        public static IList<byte> Signature { get; } = Array.AsReadOnly(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        public static DataProcessor CreatePngProcessor(Stream stream) => new DataProcessor(stream) { IsBigEndian = true };

        public PngCodec()
        {

        }

        public override int BytesForTest => Signature.Count;

        public override bool Test(byte[] bytes) => bytes.StartsWith(Signature);

        public override ImageArgb32 Read(Stream input)
        {
            var processor = CreatePngProcessor(input);
            var signature = processor.ReadBytes(BytesForTest);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            var raw = new PngRawImage();
            raw.Read(processor);
            var image = raw.Decode();
            return image;
        }

        public override void Write(Stream output, ImageArgb32 image, PngEncodeOptions options)
        {
            var processor = CreatePngProcessor(output);
            processor.WriteBytes(Signature);

            var raw = new PngRawImage();
            raw.Encode(image, options);
            raw.Write(processor);
        }

    }

}
