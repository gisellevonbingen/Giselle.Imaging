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
            var originOffset = processor.ReadLength;

            // Signature
            var signature = processor.ReadBytes(BytesForTest);

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

            uint rMask = 0;
            uint gMask = 0;
            uint bMask = 0;
            uint aMask = 0;

            if (compressionMethod == BmpCompressionMethod.BitFields)
            {
                rMask = processor.ReadUInt();
                gMask = processor.ReadUInt();
                bMask = processor.ReadUInt();
                aMask = processor.ReadUInt();
            }

            // Color Table
            var colorTable = new Argb32[colorsUsed];

            if (colorsUsed > 0)
            {
                for (var i = 0; i < colorsUsed; i++)
                {
                    var b = processor.ReadByte();
                    var g = processor.ReadByte();
                    var r = processor.ReadByte();
                    var _ = processor.ReadByte();
                    colorTable[i] = new Argb32(r, g, b);
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

            var scanData = new ScanData(width, height, stride, (int)bitsPerPixel, scan, colorTable);

            ScanProcessor scanProcessor;

            if (compressionMethod == BmpCompressionMethod.BitFields)
            {
                scanProcessor = ScanProcessor.CreateScanProcessor((int)bitsPerPixel, aMask, rMask, gMask, bMask);
            }
            else
            {
                scanProcessor = ScanProcessor.GetScanProcessor(bitsPerPixel.ToPixelFormat());
            }

            var image = new ImageArgb32(width, height)
            {
                WidthResoulution = new PhysicalDensity(widthPixelsPerMeter, PhysicalUnit.Meter),
                HeightResoulution = new PhysicalDensity(heightPixelsPerMeter, PhysicalUnit.Meter),
            };
            scanProcessor.Read(scanData, image.Scan);
            return image;
        }

        public override void Write(Stream output, ImageArgb32 image, BmpEncodeOptions options)
        {
            var bitsPerPixel = options.BitsPerPixel;
            var usedColors = new Argb32[0];

            if (bitsPerPixel != BmpBitsPerPixel.Undefined)
            {
                var colorTableLength = bitsPerPixel.ToPixelFormat().GetColorTableLength();

                if (colorTableLength > 0)
                {
                    usedColors = image.Colors.Distinct().ToArray();

                    if (usedColors.Length > colorTableLength)
                    {
                        throw new ArgumentException($"image's used colors kind({usedColors.Length}) exceeds ColorTableLength({colorTableLength})", nameof(image));
                    }

                }

            }
            else
            {
                usedColors = image.Colors.Distinct().ToArray();
                var useAlpha = usedColors.Any(c => c.A < 255);

                if (useAlpha == true)
                {
                    bitsPerPixel = BmpBitsPerPixel.Bpp32Argb;
                }
                else
                {
                    bitsPerPixel = this.GetPrefferedBitsPerPixel(usedColors.Length);
                }

            }

            var format = bitsPerPixel.ToPixelFormat();
            uint rMask = 0;
            uint gMask = 0;
            uint bMask = 0;
            uint aMask = 0;

            var compressionMethod = bitsPerPixel == BmpBitsPerPixel.Bpp32Argb ? BmpCompressionMethod.BitFields : BmpCompressionMethod.Rgb;

            if (compressionMethod == BmpCompressionMethod.BitFields)
            {
                rMask = 0x00FF0000;
                gMask = 0x0000FF00;
                bMask = 0x000000FF;
                aMask = 0xFF000000;
            }

            ScanProcessor scanProcessor;

            if (compressionMethod == BmpCompressionMethod.BitFields)
            {
                scanProcessor = ScanProcessor.CreateScanProcessor((int)bitsPerPixel, aMask, rMask, gMask, bMask);
            }
            else
            {
                scanProcessor = ScanProcessor.GetScanProcessor(format);
            }

            var stride = ScanProcessor.GetStride(image.Width, (int)bitsPerPixel);
            var scanData = new ScanData(image.Width, image.Height, stride, (int)bitsPerPixel);
            scanProcessor.Write(scanData, image.Scan);

            var bitFiledsSize = compressionMethod == BmpCompressionMethod.BitFields ? 68 : 0;
            var infoSize = 40 + bitFiledsSize;
            var colorTableSize = usedColors.Length * 4; ;
            var scanOffset = 14 + infoSize + colorTableSize;

            var processor = CreateBmpProcessor(output);

            // File Header
            processor.WriteBytes(SignatureBM);
            processor.WriteInt(scanOffset + scanData.Scan.Length);
            processor.WriteShort(0);
            processor.WriteShort(0);
            processor.WriteInt(scanOffset);

            // Bitmap Info Header
            processor.WriteInt(infoSize);
            processor.WriteInt(image.Width);
            processor.WriteInt(image.Height);
            processor.WriteShort(1);
            processor.WriteShort((short)bitsPerPixel);
            processor.WriteInt((int)compressionMethod);
            processor.WriteInt(0);
            processor.WriteInt((int)(image.WidthResoulution.GetConvertValue(PhysicalUnit.Meter)));
            processor.WriteInt((int)(image.HeightResoulution.GetConvertValue(PhysicalUnit.Meter)));
            processor.WriteInt(usedColors.Length);
            processor.WriteInt(usedColors.Length);

            // BitFields
            if (compressionMethod == BmpCompressionMethod.BitFields)
            {
                processor.WriteUInt(rMask);
                processor.WriteUInt(gMask);
                processor.WriteUInt(bMask);
                processor.WriteUInt(aMask);
                processor.WriteUInt(0x206E6957);
                processor.WriteBytes(new byte[48]);
            }

            if (usedColors.Length > 0)
            {
                for (var i = 0; i < usedColors.Length; i++)
                {
                    var color = usedColors[i];
                    processor.WriteByte(color.B);
                    processor.WriteByte(color.G);
                    processor.WriteByte(color.R);
                    processor.WriteByte(255);
                }

            }

            for (var y = image.Height - 1; y > -1; y--)
            {
                for (var x = 0; x < stride; x++)
                {
                    processor.WriteByte(scanData.Scan[y * stride + x]);
                }

            }

        }

        public BmpBitsPerPixel GetPrefferedBitsPerPixel(int colorCount)
        {
            var lastCount = 0;
            var lastBPP = BmpBitsPerPixel.Bpp24Rgb;

            foreach (var e in Enum.GetValues(typeof(BmpBitsPerPixel)) as BmpBitsPerPixel[])
            {
                var colorTableLength = e.ToPixelFormat().GetColorTableLength();

                if (colorTableLength > 0 && colorTableLength >= colorCount)
                {
                    if (lastBPP == BmpBitsPerPixel.Bpp24Rgb || colorTableLength < lastCount)
                    {
                        lastBPP = e;
                        lastCount = colorTableLength;
                    }

                }

            }

            return lastBPP;
        }

    }

}
