using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.BMP
{
    public class BMPCodec : ImageCodec
    {
        public const int SignatureLength = 2;
        public static IList<byte> SignatureBM { get; } = Array.AsReadOnly(new byte[] { 0x42, 0x4D });
        public static IList<byte> SignatureBA { get; } = Array.AsReadOnly(new byte[] { 0x42, 0x41 });
        public static IList<byte> SignatureCI { get; } = Array.AsReadOnly(new byte[] { 0x43, 0x49 });
        public static IList<byte> SignatureCP { get; } = Array.AsReadOnly(new byte[] { 0x43, 0x50 });
        public static IList<byte> SignatureIC { get; } = Array.AsReadOnly(new byte[] { 0x49, 0x43 });
        public static IList<byte> SignaturePT { get; } = Array.AsReadOnly(new byte[] { 0x50, 0x54 });
        public static IList<IList<byte>> Signatures { get; } = Array.AsReadOnly(new IList<byte>[] { SignatureBM, SignatureBA, SignatureCI, SignatureCP, SignatureIC, SignaturePT });

        public static DataProcessor CreateBMPProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = true };

        public override bool Test(byte[] bytes) => Signatures.Any(s => bytes.StartsWith(s));

        public override ScanData Read(Stream input)
        {
            var processor = CreateBMPProcessor(input);
            var origin = processor.ReadLength;

            // Header
            var signature = processor.ReadBytes(SignatureLength);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            var fileSize = processor.ReadInt();
            var reserved = processor.ReadInt();
            var dataOffset = processor.ReadInt();

            // Info Header
            var headerSize = processor.ReadInt();
            var width = processor.ReadInt();
            var height = processor.ReadInt();
            var planes = processor.ReadShort();
            var bitsPerPixel = processor.ReadShort();
            var compressionMethod = processor.ReadInt();
            var compressedImageSize = processor.ReadInt();
            var widthPixelsPerMeter = processor.ReadInt();
            var heightPixelsPerMeter = processor.ReadInt();
            var colorsUsed = processor.ReadInt();
            var importantColors = processor.ReadInt();
            var numOfColors = bitsPerPixel == 1 ? 1 : (int)Math.Pow(2, bitsPerPixel);

            // Color Table
            var colorTable = new Color[colorsUsed];
            var useColorTable = bitsPerPixel <= 8;

            if (useColorTable == true)
            {
                ReadColorTable(processor, colorTable, colorsUsed);
            }

            var current = processor.ReadLength;
            var skip = dataOffset - current - origin;

            if (skip > 0)
            {
                processor.SkipByRead(skip);
            }

            var readingStride = ScanProcessor.GetStride(width, bitsPerPixel);
            var readingScan = new byte[height * readingStride];

            for (var y = height - 1; y > -1; y--)
            {
                for (var x = 0; x < readingStride; x++)
                {
                    readingScan[y * readingStride + x] = processor.ReadByte();
                }

            }

            var pixelFormat = this.GetPixelFormat(bitsPerPixel);
            return new ScanData(width, height, readingStride, pixelFormat, readingScan, colorTable);
        }

        public PixelFormat GetPixelFormat(int bitsPerPixel)
        {
            if (bitsPerPixel == 1)
            {
                return PixelFormat.Format1bppIndexed;
            }
            else if (bitsPerPixel == 4)
            {
                return PixelFormat.Format4bppIndexed;
            }
            else if (bitsPerPixel == 8)
            {
                return PixelFormat.Format8bppIndexed;
            }
            else if (bitsPerPixel == 24)
            {
                return PixelFormat.Format24bppRgb;
            }
            else if (bitsPerPixel == 32)
            {
                return PixelFormat.Format32bppArgb;
            }
            else
            {
                return PixelFormat.Undefined;
            }

        }

        public void ReadColorTable(DataProcessor processor, Color[] colorTable, int numOfColors)
        {
            for (var i = 0; i < numOfColors; i++)
            {
                var b = processor.ReadByte();
                var g = processor.ReadByte();
                var r = processor.ReadByte();
                var _ = processor.ReadByte();
                colorTable[i] = Color.FromArgb(r, g, b);
            }

        }

    }

}
