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

        protected override void ReadPixel(byte[] inputScan, int inputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var grayscale = inputScan[inputOffset + 0];
            var alpha = inputScan[inputOffset + 1];
            frame[coord] = new Argb32() { A = alpha, Grayscale = grayscale };
        }

        protected override void WritePixel(byte[] outputScan, int outputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var color = frame[coord];
            outputScan[outputOffset + 0] = color.Grayscale;
            outputScan[outputOffset + 1] = color.A;
        }

    }

}
