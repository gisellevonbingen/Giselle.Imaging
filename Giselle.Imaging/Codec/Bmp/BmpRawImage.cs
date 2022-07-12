using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Giselle.Imaging.IO;
using Giselle.Imaging.Physical;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Bmp
{
    public class BmpRawImage
    {
        public const int InfoSize_WindowsV3 = 40;
        public const int InfoSize_WindowsV4 = InfoSize_WindowsV3 + BitFieldsLength; // 108
        public const int InfoSize_WindowsV5 = InfoSize_WindowsV4 + 16; //124
        public const int InfoSize_AVIBMP = 68;

        public const int BitFieldsLength = 68;

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

        public BmpRawImage(Stream input) : this()
        {
            this.Read(input);
        }

        public BmpRawImage(ImageArgb32Frame frame, BmpSaveOptions options) : this()
        {
            this.Encode(frame, options);
        }

        public int InfoSize => InfoSize_WindowsV3 + (this.CompressionMethod == BmpCompressionMethod.BitFields ? BitFieldsLength : 0);

        public int Stride => ScanProcessor.GetStride4(this.Width, (short)this.BitsPerPixel);

        public void Read(Stream input)
        {
            var processor = BmpCodec.CreateBmpProcessor(input);
            var signature = processor.ReadBytes(BmpCodec.SignatureLength);

            if (BmpCodec.Signatures.Any(s => signature.StartsWith(s)) == false)
            {
                throw new IOException();
            }

            // Bitmap File Header
            var fileSize = processor.ReadInt();
            this.Reserved1 = processor.ReadShort(); // Depends on application
            this.Reserved2 = processor.ReadShort(); // Depends on application
            var scanDataOffset = processor.ReadInt();

            // Bitmap Info Header
            var infoSize = processor.ReadInt();
            this.Width = processor.ReadInt();
            this.Height = processor.ReadInt();

            this.ReadInfoBeforeGap1(processor);

            var gap1Offset = processor.ReadLength;
            var gap1Length = scanDataOffset - gap1Offset;

            // Read Gap1
            if (gap1Length > 0)
            {
                processor.SkipByRead(gap1Length);
            }

            this.ReadScanData(processor);

            var gap2Offset = processor.ReadLength;
            var gap2Length = fileSize - gap2Offset;

            // Read Gap2
            if (gap2Length > 0)
            {
                processor.SkipByRead(gap2Length);
            }

        }

        public void ReadInfoBeforeGap1(DataProcessor processor, int? overrideColorsUsed = null)
        {
            // Bitmap Info Header
            var planes = processor.ReadShort(); // Must be 1
            this.BitsPerPixel = (BmpBitsPerPixel)processor.ReadShort();
            this.CompressionMethod = (BmpCompressionMethod)processor.ReadInt();
            var compressedImageSize = processor.ReadInt(); // Dummy 0 can be when unused
            this.WidthPixelsPerMeter = processor.ReadInt();  // Pixels per meter
            this.HeightPixelsPerMeter = processor.ReadInt(); // Pixels per meter
            var readingColorsUsed = processor.ReadInt(); // 0 or 2^bitsPerPixel
            var colorsUsed = overrideColorsUsed ?? readingColorsUsed;
            var importantColors = processor.ReadInt(); // Generally ingored

            if (this.CompressionMethod == BmpCompressionMethod.BitFields)
            {
                this.RChannelMask = processor.ReadUInt();
                this.GChannelMask = processor.ReadUInt();
                this.BChannelMask = processor.ReadUInt();
                this.AChannelMask = processor.ReadUInt();
            }

            // Color Table
            this.ColorTable = new Argb32[colorsUsed];

            if (colorsUsed > 0)
            {
                var bgr = new byte[4];

                for (var i = 0; i < colorsUsed; i++)
                {
                    processor.ReadBytes(bgr);
                    this.ColorTable[i] = new Argb32(bgr[2], bgr[1], bgr[0]);
                }

            }

        }

        public virtual void ReadScanData(DataProcessor processor)
        {
            this.ScanData = processor.ReadBytes(this.Height * this.Stride);
        }

        public virtual ImageArgb32Frame Decode()
        {
            ScanProcessor scanProcessor;

            if (this.CompressionMethod == BmpCompressionMethod.BitFields)
            {
                scanProcessor = ScanProcessor.CreateScanProcessor((int)this.BitsPerPixel, this.AChannelMask, this.RChannelMask, this.GChannelMask, this.BChannelMask);
            }
            else
            {
                scanProcessor = ScanProcessor.GetScanProcessor(this.BitsPerPixel.ToPixelFormat());
            }

            var scanData = new ScanData(this.Width, this.Height, (int)this.BitsPerPixel) { Stride = this.Stride, Scan = this.ScanData, ColorTable = this.ColorTable, CoordTransformer = this.GetCoordTransformer() };
            var image = new ImageArgb32Frame(scanData, scanProcessor)
            {
                PrimaryCodec = BmpCodec.Instance,
                PrimaryOptions = new BmpSaveOptions() { BitsPerPixel = this.BitsPerPixel },
                WidthResoulution = new PhysicalDensity(this.WidthPixelsPerMeter, PhysicalUnit.Meter),
                HeightResoulution = new PhysicalDensity(this.HeightPixelsPerMeter, PhysicalUnit.Meter),
            };
            return image;
        }

        public virtual void Encode(ImageArgb32Frame frame, BmpSaveOptions options)
        {
            this.Width = frame.Width;
            this.Height = frame.Height;
            this.BitsPerPixel = options.BitsPerPixel;

            var colorTable = new Argb32[0];

            if (options.BitsPerPixel != BmpBitsPerPixel.Undefined)
            {
                colorTable = frame.GetColorTable(options.BitsPerPixel.ToPixelFormat());
            }
            else
            {
                var usedColors = frame.Colors.Distinct().ToArray();
                var noAlpha = usedColors.All(c => c.A == byte.MaxValue);

                if (noAlpha == false)
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

            this.CompressionMethod = options.Compression ?? (this.BitsPerPixel == BmpBitsPerPixel.Bpp32Argb ? BmpCompressionMethod.BitFields : BmpCompressionMethod.Rgb);
            this.WidthPixelsPerMeter = (int)frame.WidthResoulution.GetConvertValue(PhysicalUnit.Meter);
            this.HeightPixelsPerMeter = (int)frame.HeightResoulution.GetConvertValue(PhysicalUnit.Meter);

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

            var stride = ScanProcessor.GetStride4(frame.Width, (int)this.BitsPerPixel);
            var scanData = new ScanData(frame.Width, frame.Height, (int)this.BitsPerPixel) { Stride = stride, Scan = new byte[frame.Height * stride], ColorTable = this.ColorTable, CoordTransformer = this.GetCoordTransformer() };
            scanProcessor.Write(scanData, frame.Scan);
            this.ScanData = scanData.Scan;
        }

        public void Write(Stream output)
        {
            var processor = BmpCodec.CreateBmpProcessor(output);

            var infoSize = this.InfoSize;
            var colorTable = this.ColorTable;
            var colorTableSize = colorTable.Length * 4;
            var scanOffset = 14 + infoSize + colorTableSize;

            // File Header
            processor.WriteBytes(BmpCodec.SignatureBM); // 2
            processor.WriteInt(scanOffset + this.ScanData.Length); // 6
            processor.WriteShort(0); // 8
            processor.WriteShort(0); // 10
            processor.WriteInt(scanOffset); // 14

            // Bitmap Info Header
            processor.WriteInt(infoSize);
            processor.WriteInt(this.Width);
            processor.WriteInt(this.Height);

            this.WriteInfoBeforeGap1(processor);
            this.WriteScanData(processor);
        }

        public virtual int CompressionSize => (this.CompressionMethod == BmpCompressionMethod.BitFields ? BitFieldsLength : 0) + (this.ColorTable.Length * 4) + this.ScanData.Length;

        public CoordTransformer GetCoordTransformer() => new CoordTransformerFlip(false, true);

        public void WriteInfoBeforeGap1(DataProcessor processor)
        {
            var colorTable = this.ColorTable;

            processor.WriteShort(1); // Planes, Must be 1
            processor.WriteShort((short)this.BitsPerPixel);
            processor.WriteInt((int)this.CompressionMethod);
            processor.WriteInt(this.CompressionSize);
            processor.WriteInt(this.WidthPixelsPerMeter);
            processor.WriteInt(this.HeightPixelsPerMeter);
            processor.WriteInt(colorTable.Length);
            processor.WriteInt(colorTable.Length);

            // BitFields
            if (this.CompressionMethod == BmpCompressionMethod.BitFields)
            {
                processor.WriteUInt(this.RChannelMask);
                processor.WriteUInt(this.GChannelMask);
                processor.WriteUInt(this.BChannelMask);
                processor.WriteUInt(this.AChannelMask);
                processor.WriteUInt(0x206E6957);
                processor.WriteBytes(new byte[48]);
            }

            if (colorTable.Length > 0)
            {
                var bgra = new byte[4];
                bgra[3] = byte.MaxValue;

                for (var i = 0; i < colorTable.Length; i++)
                {
                    var color = colorTable[i];
                    bgra[0] = color.B;
                    bgra[1] = color.G;
                    bgra[2] = color.R;
                    processor.WriteBytes(bgra);
                }

            }

        }

        public virtual void WriteScanData(DataProcessor processor)
        {
            processor.WriteBytes(this.ScanData);
        }

    }

}