﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorMaskBpp8 : ScanProcessorInt32Mask
    {
        public ScanProcessorMaskBpp8()
        {

        }

        protected override void ReadPixel(byte[] inputScan, int inputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var merged = inputScan[inputOffset + 0];
            frame[coord] = this.ReadPixel(merged);
        }

        protected override void WritePixel(byte[] outputScan, int outputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var color = frame[coord];
            var merged = this.WritePixel(color);
            outputScan[outputOffset + 0] = (byte)(merged & 0xFF);
        }

    }

}
