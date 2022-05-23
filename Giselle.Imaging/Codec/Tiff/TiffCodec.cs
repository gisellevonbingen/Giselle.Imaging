using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffCodec : ImageCodec<TiffRawImage>
    {
        public static TiffCodec Instance { get; } = new TiffCodec();

        public static IList<byte> SignatureLittleEndian { get; } = Array.AsReadOnly(new byte[] { 0x49, 0x49 });
        public static IList<byte> SignatureBigEndian { get; } = Array.AsReadOnly(new byte[] { 0x4D, 0x4D });
        public static IList<IList<byte>> Signatures { get; } = Array.AsReadOnly(new IList<byte>[] { SignatureLittleEndian, SignatureBigEndian });

        public const short EndianChecker = 0x002A;

        public static DataProcessor CreateTiffProcessor(Stream stream) => new DataProcessor(stream) { };

        public static DataProcessor CreateTiffProcessor(Stream stream, DataProcessor processor) => new DataProcessor(stream) { IsLittleEndian = processor.IsLittleEndian };

        public TiffCodec()
        {

        }

        public override int BytesForTest => 2;

        public override bool Test(byte[] bytes) => Signatures.Any(s => bytes.StartsWith(s));

        public override TiffRawImage Read(Stream input)
        {
            var processor = CreateTiffProcessor(input);
            // Signature
            var signature = processor.ReadBytes(BytesForTest);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            processor.IsLittleEndian = this.IsLittleEndian(signature);
            var endianChecker = processor.ReadShort();

            if (endianChecker != EndianChecker)
            {
                throw new IOException($"Endian Check Failed : Reading={endianChecker:X4}, Require={EndianChecker:X4}");
            }

            var image = new TiffRawImage();
            image.Read(processor);

            return image;
        }

        public override void Write(Stream output, TiffRawImage image)
        {
            throw new NotImplementedException();
        }

        public bool IsLittleEndian(byte[] signature) => SignatureLittleEndian.StartsWith(signature);

    }

}
