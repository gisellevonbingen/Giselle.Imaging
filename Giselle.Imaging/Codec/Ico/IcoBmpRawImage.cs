using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.IO;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoBmpRawImage : BmpRawImage
    {
        public byte[] AndScanData { get; set; } = new byte[0];

        public IcoBmpRawImage()
        {

        }

        public IcoBmpRawImage(Stream input) : base(input)
        {

        }

        public IcoBmpRawImage(ImageArgb32Frame frame, BmpSaveOptions options) : base(frame, options)
        {

        }

        public BmpBitsPerPixel AndBitsPerPixel => BmpBitsPerPixel.Bpp1Indexed;

        public int AndStride => ScanProcessor.GetStride4(this.Width, (int)this.AndBitsPerPixel);

        public override int CompressionSize => base.CompressionSize + this.AndScanData.Length;

        public override ImageArgb32Frame Decode()
        {
            var xorFrame = base.Decode();
            xorFrame.PrimaryOptions.CastOrDefault<BmpSaveOptions>().BitsPerPixel = BmpBitsPerPixel.Bpp32Argb;

            var table = new Argb32[2] { Argb32.Black, Argb32.White, };
            var scanData = new ScanData(this.Width, this.Height, (int)this.AndBitsPerPixel) { Stride = this.AndStride, Scan = this.AndScanData, ColorTable = table, CoordTransformer = this.GetCoordTransformer() };
            var scanProcessor = ScanProcessor.GetScanProcessor(this.AndBitsPerPixel.ToPixelFormat());
            var andFrame = new ImageArgb32Frame(scanData, scanProcessor);
            var hasAlpha = this.BitsPerPixel == BmpBitsPerPixel.Bpp32Argb;

            for (var y = 0; y < this.Height; y++)
            {
                for (var x = 0; x < this.Width; x++)
                {
                    var and = andFrame[x, y];
                    var a = hasAlpha ? byte.MinValue : and.R;
                    var xor = xorFrame[x, y];

                    var color = new Argb32(255, 0, 0, 0) & and.DeriveA(a) ^ xor;
                    xorFrame[x, y] = color;
                }

            }

            return xorFrame;
        }

        public override void ReadScanData(DataProcessor processor)
        {
            base.ReadScanData(processor);

            this.AndScanData = processor.ReadBytes(this.Height * this.AndStride);
        }

        public override void WriteScanData(DataProcessor processor)
        {
            base.WriteScanData(processor);

            processor.WriteBytes(this.AndScanData);
        }

    }

}
