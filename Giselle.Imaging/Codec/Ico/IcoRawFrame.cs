﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Ico
{
    public abstract class IcoRawFrame
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract int BitsPerPixel { get; }

        public IcoRawFrame()
        {

        }

        public abstract void ReadFrame(Stream input, IcoImageInfo info);

        public abstract ImageArgb32Frame Decode();

        public abstract void EncodeFrame(ImageArgb32Frame frame);

        public abstract void Write(Stream output, IcoImageInfo info);
    }

}
