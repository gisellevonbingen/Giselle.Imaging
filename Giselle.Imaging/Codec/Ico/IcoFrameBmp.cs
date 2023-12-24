using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoFrameBmp : IcoFrame
    {
        public IcoBmpRawImage Frame { get; set; }

        public override int Width => this.Frame.Width;
        public override int Height => this.Frame.Height;
        public override int BitsPerPixel => (int)this.Frame.BitsPerPixel;

        public IcoFrameBmp()
        {

        }

        public override void Read(Stream input, IcoImageInfo info)
        {
            var processor = BmpCodec.CreateBmpProcessor(input);
            var infoSize = processor.ReadInt();
            var width = processor.ReadInt();
            var height = processor.ReadInt() / 2;
            var frame = new IcoBmpRawImage
            {
                Width = width,
                Height = height,
            };
            frame.ReadInfoBeforeGap1(processor, info.UsedColors > 0 ? new byte?(info.UsedColors) : null);
            frame.ReadScanData(processor);
            this.Frame = frame;
        }

        public override ImageArgb32Frame Decode() => this.Frame.Decode();

        public override void Encod(ImageArgb32Frame frame)
        {
            throw new NotSupportedException();
        }

        public override void Write(Stream output, IcoImageInfo info)
        {
            var frame = this.Frame;
            info.UsedColors = (byte)frame.ColorTable.Length;

            var processor = BmpCodec.CreateBmpProcessor(output);
            processor.WriteInt(frame.InfoSize);
            processor.WriteInt(frame.Width);
            processor.WriteInt(frame.Height * 2);

            frame.WriteInfoBeforeGap1(processor);
            frame.WriteScanData(processor);
        }

    }

}
