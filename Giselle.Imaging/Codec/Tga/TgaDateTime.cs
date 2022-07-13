using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tga
{
    public struct TgaDateTime : IEquatable<TgaDateTime>
    {
        public const int Length = 12;

        public ushort Month { get; set; }
        public ushort Day { get; set; }
        public ushort Year { get; set; }
        public ushort Hour { get; set; }
        public ushort Minute { get; set; }
        public ushort Second { get; set; }

        public TgaDateTime(DataProcessor processor) : this()
        {
            this.Read(processor);
        }

        public TgaDateTime(DateTime dateTime) : this()
        {
            this.Month = (ushort)dateTime.Month;
            this.Day = (ushort)dateTime.Day;
            this.Year = (ushort)dateTime.Year;
            this.Hour = (ushort)dateTime.Hour;
            this.Minute = (ushort)dateTime.Minute;
            this.Second = (ushort)dateTime.Second;
        }

        public override bool Equals(object obj)
        {
            return obj is TgaDateTime other && this.Equals(other);
        }

        public bool Equals(TgaDateTime other)
        {
            return this.DateTime.Equals(other.DateTime);
        }

        public override int GetHashCode()
        {
            return this.DateTime.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.Year:D4}-{this.Month:D2}-{this.Day:D2} {this.Hour:D2}:{this.Minute:D2}:{this.Second:D2}";
        }

        public DateTime DateTime => new DateTime(this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second, DateTimeKind.Utc);

        public void Read(DataProcessor processor)
        {
            this.Month = processor.ReadUShort();
            this.Day = processor.ReadUShort();
            this.Year = processor.ReadUShort();
            this.Hour = processor.ReadUShort();
            this.Minute = processor.ReadUShort();
            this.Second = processor.ReadUShort();
        }

        public void Write(DataProcessor processor)
        {
            processor.WriteUShort(this.Month);
            processor.WriteUShort(this.Day);
            processor.WriteUShort(this.Year);
            processor.WriteUShort(this.Hour);
            processor.WriteUShort(this.Minute);
            processor.WriteUShort(this.Second);
        }

    }

}
