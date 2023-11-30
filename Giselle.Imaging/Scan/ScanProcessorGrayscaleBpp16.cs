using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorGrayscaleBpp16 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor InstanceAGrayscale88 { get; } = new ScanProcessorGrayscaleBpp16();

        public ScanProcessorGrayscaleBpp16()
        {

        }

        protected override Argb32 DecodePixel(byte[] inputScan, int inputOffset)
        {
            var grayscale = inputScan[inputOffset + 0];
            var alpha = inputScan[inputOffset + 1];
            return new() { A = alpha, Grayscale = grayscale };
        }

        protected override void EncodePixel(byte[] outputScan, int outputOffset, Argb32 color)
        {
            outputScan[outputOffset + 0] = color.Grayscale;
            outputScan[outputOffset + 1] = color.A;
        }

    }

}
