using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Riff
{
    public class AniHeader
    {
        public int Frames { get; set; }
        public int Steps { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int BitCount { get; set; }
        public int Planes { get; set; }
        public int JIFRate { get; set; }
        public int Flags { get; set; }

        public AniHeader()
        {

        }

        public AniHeader(Stream input) : this()
        {
            this.Read(input);
        }

        public void Read(Stream input)
        {
            var processor = RiffChunk.CreateRiffDataProcessor(input);
            var sizeWithSelf = processor.ReadInt();
            this.Frames = processor.ReadInt();
            this.Steps = processor.ReadInt();
            this.CenterX = processor.ReadInt();
            this.CenterY = processor.ReadInt();
            this.BitCount = processor.ReadInt();
            this.Planes = processor.ReadInt();
            this.JIFRate = processor.ReadInt();
            this.Flags = processor.ReadInt();
        }

        public void Write(Stream output)
        {
            var processor = RiffChunk.CreateRiffDataProcessor(output);
            processor.WriteInt(0x24);
            processor.WriteInt(this.Frames);
            processor.WriteInt(this.Steps);
            processor.WriteInt(this.CenterX);
            processor.WriteInt(this.CenterY);
            processor.WriteInt(this.BitCount);
            processor.WriteInt(this.Planes);
            processor.WriteInt(this.JIFRate);
            processor.WriteInt(this.Flags);
        }

    }

}
