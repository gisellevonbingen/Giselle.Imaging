using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.ICC
{
    public struct ICCDateTime : IEquatable<ICCDateTime>
    {
        public const int Length = 12;

        public ushort Year { get; set; }
        public ushort Month { get; set; }
        public ushort Day { get; set; }
        public ushort Hours { get; set; }
        public ushort Minutes { get; set; }
        public ushort Seconds { get; set; }

        public ICCDateTime(DataProcessor processor) : this()
        {
            this.Read(processor);
        }

        public ICCDateTime(DateTime dateTime) : this()
        {
            this.Year = (ushort)dateTime.Year;
            this.Month = (ushort)dateTime.Month;
            this.Day = (ushort)dateTime.Day;
            this.Hours = (ushort)dateTime.Hour;
            this.Minutes = (ushort)dateTime.Minute;
            this.Seconds = (ushort)dateTime.Second;
        }

        public override bool Equals(object obj)
        {
            return obj is ICCDateTime other && this.Equals(other);
        }

        public bool Equals(ICCDateTime other)
        {
            return this.DateTime.Equals(other.DateTime);
        }

        public override int GetHashCode()
        {
            return this.DateTime.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.Year:D4}-{this.Month:D2}-{this.Day:D2} {this.Hours:D2}:{this.Minutes:D2}:{this.Seconds:D2}";
        }

        public DateTime DateTime => new DateTime(this.Year, this.Month, this.Day, this.Hours, this.Minutes, this.Seconds, DateTimeKind.Utc);

        public void Read(DataProcessor processor)
        {
            this.Year = processor.ReadUShort();
            this.Month = processor.ReadUShort();
            this.Day = processor.ReadUShort();
            this.Hours = processor.ReadUShort();
            this.Minutes = processor.ReadUShort();
            this.Seconds = processor.ReadUShort();
        }

        public void Write(DataProcessor processor)
        {
            processor.WriteUShort(this.Year);
            processor.WriteUShort(this.Month);
            processor.WriteUShort(this.Day);
            processor.WriteUShort(this.Hours);
            processor.WriteUShort(this.Minutes);
            processor.WriteUShort(this.Seconds);
        }

    }

}
