using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formats.Riff;

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

        public override int BytesForTest => 12;

        public override IEnumerable<string> GetExtensions()
        {
            yield return "ani";
        }

        protected override bool TestAsStream(Stream stream)
        {
            using var s = new RiffInputStream(stream, true);
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

        public override PixelFormat GetPreferredPixelFormat(ImageArgb32Frame frame)
        {
            throw new NotSupportedException();
        }

    }

}
