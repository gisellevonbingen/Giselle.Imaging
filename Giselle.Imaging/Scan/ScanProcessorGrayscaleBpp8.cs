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

        protected override Argb32 DecodePixel(byte[] inputScan, int inputOffset)
        {
            return new() { A = byte.MaxValue, Grayscale = inputScan[inputOffset] };
        }

        protected override void EncodePixel(byte[] outputScan, int outputOffset, Argb32 color)
        {
            outputScan[outputOffset] = color.Grayscale;
        }

    }

}
