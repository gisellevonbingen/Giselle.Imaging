﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec;

namespace Giselle.Imaging
{
    public interface IImageArgb32
    {
        int Width { get; }
        int Height { get; }

        ImageCodec PrimaryCodec { get; }

        void Save(Stream output);

        void Save(Stream output, ImageCodec codec);

        void Save(Stream output, ImageCodec codec, ISaveOptions options);

    }

}
