using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tga
{
    public struct TgaTimeSpan : IEquatable<TgaTimeSpan>
    {
        public const int Length = 6;

        public ushort Hours { get; set; }
        public ushort Minutes { get; set; }
        public ushort Seconds { get; set; }

        public TgaTimeSpan(DataProcessor processor) : this()
        {
            this.Read(processor);
        }

        public TgaTimeSpan(TimeSpan span) : this()
        {
            this.Hours = (ushort)span.TotalHours;
            this.Minutes = (ushort)span.Minutes;
            this.Seconds = (ushort)span.Seconds;
        }

        public override bool Equals(object obj)
        {
            return obj is TgaTimeSpan other && this.Equals(other);
        }

        public bool Equals(TgaTimeSpan other)
        {
            return this.Span.Equals(other.Span);
        }

        public override int GetHashCode()
        {
            return this.Span.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.Hours}:{this.Minutes:D2}:{this.Seconds:D2}";
        }

        public TimeSpan Span => TimeSpan.FromHours(this.Hours).Add(new TimeSpan(0, this.Minutes, this.Seconds));

        public void Read(DataProcessor processor)
        {
            this.Hours = processor.ReadUShort();
            this.Minutes = processor.ReadUShort();
            this.Seconds = processor.ReadUShort();
        }

        public void Write(DataProcessor processor)
        {
            processor.WriteUShort(this.Hours);
            processor.WriteUShort(this.Minutes);
            processor.WriteUShort(this.Seconds);
        }

    }

}
