using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorGrayscaleBpp8 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor InstanceGrayscale8 { get; } = new ScanProcessorGrayscaleBpp8();

        public ScanProcessorGrayscaleBpp8()
        {

        }

        protected override void DecodePixel(byte[] inputScan, int inputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var grayscale = inputScan[inputOffset];
            frame[coord] = new Argb32() { A = byte.MaxValue, Grayscale = grayscale };
        }

        protected override void EncodePixel(byte[] outputScan, int outputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var color = frame[coord];
            outputScan[outputOffset] = color.Grayscale;
        }

    }

}
