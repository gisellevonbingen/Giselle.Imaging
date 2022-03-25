using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Bmp
{
    public class BmpCodec : ImageCodec
    {
        public const int SignatureLength = 2;
        public static IList<byte> SignatureBM { get; } = Array.AsReadOnly(new byte[] { 0x42, 0x4D });
        public static IList<byte> SignatureBA { get; } = Array.AsReadOnly(new byte[] { 0x42, 0x41 });
        public static IList<byte> SignatureCI { get; } = Array.AsReadOnly(new byte[] { 0x43, 0x49 });
        public static IList<byte> SignatureCP { get; } = Array.AsReadOnly(new byte[] { 0x43, 0x50 });
        public static IList<byte> SignatureIC { get; } = Array.AsReadOnly(new byte[] { 0x49, 0x43 });
        public static IList<byte> SignaturePT { get; } = Array.AsReadOnly(new byte[] { 0x50, 0x54 });
        public static IList<IList<byte>> Signatures { get; } = Array.AsReadOnly(new IList<byte>[] { SignatureBM, SignatureBA, SignatureCI, SignatureCP, SignatureIC, SignaturePT });

        public const double DPICoefficient = 25.4D / 1000.0D;

        public static DataProcessor CreateBMPProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = true };

        public override bool Test(byte[] bytes) => Signatures.Any(s => bytes.StartsWith(s));

        public override void Write(Stream output, ScanData data)
        {
            var width = data.Width;
            var height = data.Height;
            var bitsPerPixel = data.Format.ToBitsPerPixel();
            var colorTable = data.ColorTable;
            var colorsUsed = bitsPerPixel.IsUseColorTable() ? colorTable.Length : 0;
            var stride = data.Stride;
            var scan = data.Scan;

            var infoSize = 40;
            var colorTableSize = colorsUsed * 4;
            var scanSize = scan.Length;
            var scanOffset = 14 + infoSize + colorTableSize;

            var processor = CreateBMPProcessor(output);

            // File Header
            processor.WriteBytes(SignatureBM);
            processor.WriteInt(scanOffset + scanSize);
            processor.WriteShort(0);
            processor.WriteShort(0);
            processor.WriteInt(scanOffset);

            //// Bitmap Info Header
            processor.WriteInt(infoSize);
            processor.WriteInt(width);
            processor.WriteInt(height);
            processor.WriteShort(1);
            processor.WriteShort((short)bitsPerPixel);
            processor.WriteInt(0);
            processor.WriteInt(0);
            processor.WriteInt((int)(data.WidthResoulution / DPICoefficient));
            processor.WriteInt((int)(data.HeightResoulution / DPICoefficient));
            processor.WriteInt(colorsUsed);
            processor.WriteInt(colorsUsed);

            if (colorsUsed > 0)
            {
                for (var i = 0; i < colorsUsed; i++)
                {
                    var color = colorTable[i];
                    processor.WriteByte(color.B);
                    processor.WriteByte(color.G);
                    processor.WriteByte(color.R);
                    processor.WriteByte(255);
                }

            }

            for (var y = height - 1; y > -1; y--)
            {
                for (var x = 0; x < stride; x++)
                {
                    processor.WriteByte(scan[y * stride + x]);
                }

            }

        }

        public override ScanData Read(Stream input)
        {
            var processor = CreateBMPProcessor(input);
            var originOffset = processor.ReadLength;

            // Signature
            var signature = processor.ReadBytes(SignatureLength);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            // File Header
            var fileSize = processor.ReadInt();
            var reserved1 = processor.ReadShort(); // Depends on application
            var reserved2 = processor.ReadShort(); // Depends on application
            var scanDataOffset = processor.ReadInt();

            // Bitmap Info Header
            var headerSize = processor.ReadInt(); // Must be 40
            var width = processor.ReadInt();
            var height = processor.ReadInt();
            var planes = processor.ReadShort(); // Must be 1
            var bitsPerPixel = (BmpBitsPerPixel)processor.ReadShort();
            var compressionMethod = (BmpCompressionMethod)processor.ReadInt();
            var compressedImageSize = processor.ReadInt(); // Dummy 0 can be when unused
            var widthPixelsPerMeter = processor.ReadInt();  // Pixels per meter
            var heightPixelsPerMeter = processor.ReadInt(); // Pixels per meter
            var colorsUsed = processor.ReadInt(); // 0 or 2^bitsPerPixel
            var importantColors = processor.ReadInt(); // Generally ingored

            // Color Table
            var colorTable = new Color[colorsUsed];

            if (colorsUsed > 0)
            {
                for (var i = 0; i < colorsUsed; i++)
                {
                    var b = processor.ReadByte();
                    var g = processor.ReadByte();
                    var r = processor.ReadByte();
                    var _ = processor.ReadByte();
                    colorTable[i] = Color.FromArgb(r, g, b);
                }

            }

            var gap1Offset = processor.ReadLength;
            var gap1Length = scanDataOffset - gap1Offset - originOffset;

            // Read Gap1
            if (gap1Length > 0)
            {
                processor.SkipByRead(gap1Length);
            }

            var stride = ScanProcessor.GetStride(width, (short)bitsPerPixel);
            var scan = new byte[height * stride];

            for (var y = height - 1; y > -1; y--)
            {
                for (var x = 0; x < stride; x++)
                {
                    scan[y * stride + x] = processor.ReadByte();
                }

            }

            var gap2Offset = processor.ReadLength;
            var gap2Length = fileSize - gap2Offset - originOffset;

            // Read Gap2
            if (gap2Length > 0)
            {
                processor.SkipByRead(gap2Length);
            }

            return new ScanData(width, height, stride, bitsPerPixel.ToPixelFormat(), scan, colorTable)
            {
                WidthResoulution = widthPixelsPerMeter * DPICoefficient,
                HeightResoulution = heightPixelsPerMeter * DPICoefficient,
            };
        }

        public override ScanData Encode(Image32Argb image) => this.Encode(image, new BmpEncodingOptions());

        public ScanData Encode(Image32Argb image, BmpEncodingOptions options)
        {
            if (options.BitsPerPixel != BmpBitsPerPixel.Undefined)
            {
                var colorTableLength = options.BitsPerPixel.GetColorTableLength();

                if (colorTableLength > 0)
                {
                    var colorCount = image.Colors.Distinct().Count();

                    if (colorCount > colorTableLength)
                    {
                        throw new ArgumentException($"image's used colors kind({colorCount}) exceeds ColorTableLength({colorTableLength})", nameof(image));
                    }

                }

            }

            return new ScanData(image);
        }

    }

}
