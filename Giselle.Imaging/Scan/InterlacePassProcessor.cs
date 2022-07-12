using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;
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

        public PointI GetCoord(PointI indexCoord)
        {
            var scanData = this.ScanData;

            if (scanData.InterlacePasses.Length == 0)
            {
                return indexCoord;
            }
            else
            {
                var pass = scanData.InterlacePasses[this.PassIndex];
                var x = pass.OffsetX + pass.IntervalX * indexCoord.X;
                var y = pass.OffsetY + pass.IntervalY * indexCoord.Y;
                return new PointI(x, y);
            }

        }

        public PointI GetEncodeCoord(PointI indexCoord) => this.ScanData.GetEncodeCoord(this.GetCoord(indexCoord));

        public PointI GetDecodeCoord(PointI indexCoord) => this.ScanData.GetDecodeCoord(this.GetCoord(indexCoord));

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
