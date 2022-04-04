using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Scan
{
    public class InterlacePassProcessor
    {
        public ScanData ScanData { get; private set; }
        public int PassIndex { get; private set; }
        public InterlacePassInformation PassInfo { get; private set; }

        public InterlacePassProcessor(ScanData scanData)
        {
            this.ScanData = scanData;
            this.PassIndex = -1;
        }

        public (int X, int Y) GetPosition(int xi, int yi)
        {
            var scanData = this.ScanData;

            if (scanData.InterlacePasses.Length == 0)
            {
                return (xi, yi);
            }
            else
            {
                var pass = scanData.InterlacePasses[this.PassIndex];
                var x = pass.OffsetX + pass.IntervalX * xi;
                var y = pass.OffsetY + pass.IntervalY * yi;
                return (x, y);
            }

        }

        public bool NextPass()
        {
            var scanData = this.ScanData;

            if (scanData.InterlacePasses.Length == 0)
            {
                if (this.PassIndex >= 0)
                {
                    return false;
                }
                else
                {
                    this.PassIndex++;
                    this.PassInfo = new InterlacePassInformation() { PixelsX = scanData.Width, PixelsY = scanData.Height, Stride = scanData.Stride };
                    return true;
                }

            }
            else
            {
                if (this.PassIndex + 1 >= scanData.InterlacePasses.Length)
                {
                    return false;
                }
                else
                {
                    this.PassIndex++;
                    this.PassInfo = scanData.GetInterlacePassInformation(this.PassIndex);
                    return true;
                }

            }

        }

    }

}
