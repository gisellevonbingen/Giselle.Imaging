using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoRawFrameBmp : IcoRawFrame
    {
        public BmpRawImage And { get; set; }
        public BmpRawImage Xor { get; set; }

        public override int Width => this.And.Width;
        public override int Height => this.And.Height;
        public override int BitsPerPixel => (int)this.And.BitsPerPixel;

        public IcoRawFrameBmp()
        {

        }

        public override void ReadFrame(Stream input, IcoImageInfo info)
        {
            var processor = BmpCodec.CreateBmpProcessor(input);
            var infoSize = processor.ReadInt();
            var width = processor.ReadInt();
            var height = processor.ReadInt() / 2;
            var and = new BmpRawImage
            {
                Width = width,
                Height = height,
            };
            and.ReadInfoBeforeGap1(processor, info.UsedColors);
            and.ReadScanData(processor);
            this.And = and;

            var xor = new BmpRawImage() { Width = width, Height = height, BitsPerPixel = BmpBitsPerPixel.Bpp1Indexed };
            xor.ColorTable = new Argb32[2] { Argb32.Black.DeriveA(0), Argb32.White.DeriveA(0), };
            xor.ReadScanData(processor);
            this.Xor = xor;
        }

        public override ImageArgb32Frame Decode()
        {
            var andFrame = this.And.Decode();
            var xorFrame = this.Xor.Decode();

            for (var x = 0; x < andFrame.Width; x++)
            {
                for (var y = 0; y < andFrame.Height; y++)
                {
                    andFrame[x, y] ^= xorFrame[x, y];
                }

            }

            return andFrame;
        }

        public override void EncodeFrame(ImageArgb32Frame frame)
        {
            var options = new BmpSaveOptions() { Compression = BmpCompressionMethod.Rgb };
            this.And = new BmpRawImage(frame, options);
            this.Xor = new BmpRawImage(new ImageArgb32Frame(frame.Width, frame.Height), options);
        }

        public override void Write(Stream output, IcoImageInfo info)
        {
            var and = this.And;
            var xor = this.Xor;
            info.UsedColors = (byte)and.ColorTable.Length;

            var processor = BmpCodec.CreateBmpProcessor(output);
            processor.WriteInt(and.InfoSize);
            processor.WriteInt(and.Width);
            processor.WriteInt(and.Height * 2);

            and.WriteInfoBeforeGap1(processor);
            and.WriteScanData(processor);
            xor.WriteScanData(processor);
        }

    }

}
