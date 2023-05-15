using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoImageInfo
    {
        public const int Size = 16;

        public byte Width { get; set; }
        public byte Height { get; set; }
        public byte UsedColors { get; set; }
        public byte Reserved { get; set; }
        public short MagicNumber1 { get; set; }
        public short MagicNumber2 { get; set; }
        public int DataSize { get; set; }
        public int DataOffset { get; set; }

        public IcoImageInfo()
        {

        }

        public IcoImageInfo(DataProcessor processor)
        {
            this.Read(processor);
        }

        public void Read(DataProcessor processor)
        {
            this.Width = processor.ReadByte();
            this.Height = processor.ReadByte();
            this.UsedColors = processor.ReadByte();
            this.Reserved = processor.ReadByte();
            this.MagicNumber1 = processor.ReadShort();
            this.MagicNumber2 = processor.ReadShort();
            this.DataSize = processor.ReadInt();
            this.DataOffset = processor.ReadInt();
        }

        public void Write(DataProcessor processor)
        {
            processor.WriteByte(this.Width);
            processor.WriteByte(this.Height);
            processor.WriteByte(this.UsedColors);
            processor.WriteByte(this.Reserved);
            processor.WriteShort(this.MagicNumber1);
            processor.WriteShort(this.MagicNumber2);
            processor.WriteInt(this.DataSize);
            processor.WriteInt(this.DataOffset);
        }

        public short IconColorPlanes { get => this.MagicNumber1; set => this.MagicNumber1 = value; }
        public short IconBitsPerPixel { get => this.MagicNumber2; set => this.MagicNumber2 = value; }

        public short CursorHotspotLeft { get => this.MagicNumber1; set => this.MagicNumber1 = value; }
        public short CursorHotspotTop { get => this.MagicNumber2; set => this.MagicNumber2 = value; }
    }

}
