﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Png;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoFramePng : IcoFrame
    {
        public PngRawImage Frame { get; set; }

        public override int Width => this.Frame.Width;
        public override int Height => this.Frame.Height;
        public override int BitsPerPixel => this.Frame.PixelFormat.GetBitsPerPixel();

        public IcoFramePng()
        {

        }

        public override void Read(Stream input, IcoImageInfo info) => this.Frame = new PngRawImage(input);

        public override ImageArgb32Frame Decode() => this.Frame.Decode();

        public override void Encod(ImageArgb32Frame frame) => this.Frame = new PngRawImage(frame, new PngSaveOptions() { PixelFormat = PngPixelFormat.Bpp32TruecolorWithAlpha });

        public override void Write(Stream output, IcoImageInfo info) => this.Frame.Write(output);

    }

}
