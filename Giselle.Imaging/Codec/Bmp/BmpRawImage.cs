using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
using Giselle.Imaging.Physical;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Bmp
{
    public class BmpRawImage
    {
        public short Reserved1 { get; set; }
        public short Reserved2 { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public BmpBitsPerPixel BitsPerPixel { get; set; }
        public BmpCompressionMethod CompressionMethod { get; set; }
        public int WidthPixelsPerMeter { get; set; }
        public int HeightPixelsPerMeter { get; set; }
        public uint RChannelMask { get; set; }
        public uint GChannelMask { get; set; }
        public uint BChannelMask { get; set; }
        public uint AChannelMask { get; set; }
        public Argb32[] ColorTable { get; set; } = new Argb32[0];
        public byte[] ScanData { get; set; } = new byte[0];

        public BmpRawImage()
        {

        }

        public int Stride => ScanProcessor.GetStride(this.Width, (short)this.BitsPerPixel);

        public void Read(DataProcessor input)
        {
            var fileSize = input.ReadInt();
            this.Reserved1 = input.ReadShort(); // Depends on application
            this.Reserved2 = input.ReadShort(); // Depends on application
            var scanDataOffset = input.ReadInt();

            // Bitmap Info Header
            var headerSize = input.ReadInt(); // Must be 40
            this.Width = input.ReadInt();
            this.Height = input.ReadInt();
            var planes = input.ReadShort(); // Must be 1
            this.BitsPerPixel = (BmpBitsPerPixel)input.ReadShort();
            this.CompressionMethod = (BmpCompressionMethod)input.ReadInt();
            var compressedImageSize = input.ReadInt(); // Dummy 0 can be when unused
            this.WidthPixelsPerMeter = input.ReadInt();  // Pixels per meter
            this.HeightPixelsPerMeter = input.ReadInt(); // Pixels per meter
            var colorsUsed = input.ReadInt(); // 0 or 2^bitsPerPixel
            var importantColors = input.ReadInt(); // Generally ingored

            if (this.CompressionMethod == BmpCompressionMethod.BitFields)
            {
                this.RChannelMask = input.ReadUInt();
                this.GChannelMask = input.ReadUInt();
                this.BChannelMask = input.ReadUInt();
                this.AChannelMask = input.ReadUInt();
            }

            // Color Table
            this.ColorTable = new Argb32[colorsUsed];

            if (colorsUsed > 0)
            {
                for (var i = 0; i < colorsUsed; i++)
                {
                    var b = input.ReadByte();
                    var g = input.ReadByte();
                    var r = input.ReadByte();
                    var _ = input.ReadByte();
                    this.ColorTable[i] = new Argb32(r, g, b);
                }

            }

            var gap1Offset = input.ReadLength;
            var gap1Length = scanDataOffset - gap1Offset;

            // Read Gap1
            if (gap1Length > 0)
            {
                input.SkipByRead(gap1Length);
            }

            var stride = this.Stride;
            this.ScanData = new byte[this.Height * stride];

            for (var y = this.Height - 1; y > -1; y--)
            {
                for (var x = 0; x < stride; x++)
                {
                    this.ScanData[y * stride + x] = input.ReadByte();
                }

            }

            var gap2Offset = input.ReadLength;
            var gap2Length = fileSize - gap2Offset;

            // Read Gap2
            if (gap2Length > 0)
            {
                input.SkipByRead(gap2Length);
            }

        }

        public ImageArgb32 Decode()
        {
            var scanData = new ScanData(this.Width, this.Height, this.Stride, (int)this.BitsPerPixel, this.ScanData) { ColorTable = this.ColorTable };

            ScanProcessor scanProcessor;

            if (this.CompressionMethod == BmpCompressionMethod.BitFields)
            {
                scanProcessor = ScanProcessor.CreateScanProcessor((int)this.BitsPerPixel, this.AChannelMask, this.RChannelMask, this.GChannelMask, this.BChannelMask);
            }
            else
            {
                scanProcessor = ScanProcessor.GetScanProcessor(this.BitsPerPixel.ToPixelFormat());
            }

            var image = new ImageArgb32(this.Width, this.Height)
            {
                WidthResoulution = new PhysicalDensity(this.WidthPixelsPerMeter, PhysicalUnit.Meter),
                HeightResoulution = new PhysicalDensity(this.HeightPixelsPerMeter, PhysicalUnit.Meter),
            };
            scanProcessor.Read(scanData, image.Scan);
            return image;
        }

        public void Encode(ImageArgb32 image, BmpEncodeOptions options)
        {
            this.Width = image.Width;
            this.Height = image.Height;
            this.BitsPerPixel = options.BitsPerPixel;

            var colorTable = new Argb32[0];

            if (options.BitsPerPixel != BmpBitsPerPixel.Undefined)
            {
                colorTable = image.GetColorTable(options.BitsPerPixel.ToPixelFormat());
            }
            else
            {
                var usedColors = image.Colors.Distinct().ToArray();
                var useAlpha = usedColors.Any(c => c.A < 255);

                if (useAlpha == true)
                {
                    this.BitsPerPixel = BmpBitsPerPixel.Bpp32Argb;
                }
                else
                {
                    var format = usedColors.Length.GetPrefferedIndexedFormat();
                    this.BitsPerPixel = format.ToBmpBitsPerPixel();
                    if (format.IsUseColorTable() == true) colorTable = usedColors;
                }

            }

            this.CompressionMethod = this.BitsPerPixel == BmpBitsPerPixel.Bpp32Argb ? BmpCompressionMethod.BitFields : BmpCompressionMethod.Rgb;
            this.WidthPixelsPerMeter = (int)image.WidthResoulution.GetConvertValue(PhysicalUnit.Meter);
            this.HeightPixelsPerMeter = (int)image.HeightResoulution.GetConvertValue(PhysicalUnit.Meter);

            if (this.CompressionMethod == BmpCompressionMethod.BitFields)
            {
                this.RChannelMask = 0x00FF0000;
                this.GChannelMask = 0x0000FF00;
                this.BChannelMask = 0x000000FF;
                this.AChannelMask = 0xFF000000;
            }

            this.ColorTable = colorTable;

            ScanProcessor scanProcessor;

            if (this.CompressionMethod == BmpCompressionMethod.BitFields)
            {
                scanProcessor = ScanProcessor.CreateScanProcessor((int)this.BitsPerPixel, this.AChannelMask, this.RChannelMask, this.GChannelMask, this.BChannelMask);
            }
            else
            {
                var format = this.BitsPerPixel.ToPixelFormat();
                scanProcessor = ScanProcessor.GetScanProcessor(format);
            }

            var stride = ScanProcessor.GetStride(image.Width, (int)this.BitsPerPixel);
            var scanData = new ScanData(image.Width, image.Height, stride, (int)this.BitsPerPixel) { ColorTable = this.ColorTable };
            scanProcessor.Write(scanData, image.Scan);
            this.ScanData = scanData.Scan;
        }

        public void Write(DataProcessor output)
        {
            var bitFiledsSize = this.CompressionMethod == BmpCompressionMethod.BitFields ? 68 : 0;
            var infoSize = 40 + bitFiledsSize;
            var colorTable = this.BitsPerPixel.ToPixelFormat().IsUseColorTable() ? this.ColorTable : new Argb32[0];
            var colorTableSize = colorTable.Length * 4;
            var scanOffset = 14 + infoSize + colorTableSize;

            // File Header
            output.WriteInt(scanOffset + this.ScanData.Length);
            output.WriteShort(0);
            output.WriteShort(0);
            output.WriteInt(scanOffset);

            // Bitmap Info Header
            output.WriteInt(infoSize);
            output.WriteInt(this.Width);
            output.WriteInt(this.Height);
            output.WriteShort(1);
            output.WriteShort((short)this.BitsPerPixel);
            output.WriteInt((int)this.CompressionMethod);
            output.WriteInt(0);
            output.WriteInt(this.WidthPixelsPerMeter);
            output.WriteInt(this.HeightPixelsPerMeter);
            output.WriteInt(colorTable.Length);
            output.WriteInt(colorTable.Length);

            // BitFields
            if (this.CompressionMethod == BmpCompressionMethod.BitFields)
            {
                output.WriteUInt(this.RChannelMask);
                output.WriteUInt(this.GChannelMask);
                output.WriteUInt(this.BChannelMask);
                output.WriteUInt(this.AChannelMask);
                output.WriteUInt(0x206E6957);
                output.WriteBytes(new byte[48]);
            }

            if (this.ColorTable.Length > 0)
            {
                for (var i = 0; i < this.ColorTable.Length; i++)
                {
                    var color = this.ColorTable[i];
                    output.WriteByte(color.B);
                    output.WriteByte(color.G);
                    output.WriteByte(color.R);
                    output.WriteByte(255);
                }

            }

            var stride = this.Stride;

            for (var y = this.Height - 1; y > -1; y--)
            {
                for (var x = 0; x < stride; x++)
                {
                    output.WriteByte(this.ScanData[y * stride + x]);
                }

            }

        }

    }

}
