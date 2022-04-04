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
            var pass = this.ScanData.InterlacePasses[this.PassIndex];
            var x = pass.OffsetX + pass.IntervalX * xi;
            var y = pass.OffsetY + pass.IntervalY * yi;
            return (x, y);
        }

        public bool NextPass()
        {
            if (this.PassIndex + 1 >= this.ScanData.InterlacePasses.Length)
            {
                return false;
            }
            else
            {
                this.PassIndex++;
                this.PassInfo = this.ScanData.GetInterlacePassInformation(this.PassIndex);
                return true;
            }

        }

    }

}
