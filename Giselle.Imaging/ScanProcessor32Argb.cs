using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanProcessor32Argb : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor Instance { get; } = new ScanProcessor32Argb();

        public ScanProcessor32Argb()
        {

        }

        protected override void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset)
        {
            formatScan[formatOffset + 0] = inputScan[inputOffset + 0];
            formatScan[formatOffset + 1] = inputScan[inputOffset + 1];
            formatScan[formatOffset + 2] = inputScan[inputOffset + 2];
            formatScan[formatOffset + 3] = inputScan[inputOffset + 3];
        }

        protected override void WritePixel(byte[] outputScan, int outputOffset, byte[] formatScan, int formatOffset)
        {
            outputScan[outputOffset + 0] = formatScan[formatOffset + 0];
            outputScan[outputOffset + 1] = formatScan[formatOffset + 1];
            outputScan[outputOffset + 2] = formatScan[formatOffset + 2];
            outputScan[outputOffset + 3] = formatScan[formatOffset + 3];
        }

    }

}
