﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Ico;
using Giselle.Imaging.Codec.Riff;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Ani
{
    public class AniCodec : ImageCodec
    {
        public static AniCodec Instance { get; } = new AniCodec();

        public const int ACONFormTypeKey = 0x4E4F4341;

        public AniCodec()
        {

        }

        public override bool SupportMultiFrame => true;

        public override string PrimaryExtension => "ani";

        public override int BytesForTest => 12;

        public override IEnumerable<string> GetExtensions()
        {
            yield return this.PrimaryExtension;
        }

        protected override bool TestAsStream(Stream stream)
        {
            var s = new RiffInputStream(stream);
            return s.FormType == ACONFormTypeKey;
        }

        public override ImageArgb32Container Read(Stream input)
        {
            var rawContainer = new AniContainer(input);
            return rawContainer.Decode(0);
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions options)
        {
            throw new NotSupportedException();
        }

    }

}
