using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

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

        public static TgaDateTime FromDateTime(DateTime dateTime) => new(dateTime);

        public static TgaDateTime FromDateTime(DateTime? dateTime) => dateTime.HasValue ? new TgaDateTime(dateTime.Value) : new TgaDateTime();

        public TgaDateTime(DateTime dateTime) : this()
        {
            this.Month = (ushort)dateTime.Month;
            this.Day = (ushort)dateTime.Day;
            this.Year = (ushort)dateTime.Year;
            this.Hour = (ushort)dateTime.Hour;
            this.Minute = (ushort)dateTime.Minute;
            this.Second = (ushort)dateTime.Second;
        }

        public override bool Equals(object obj) => obj is TgaDateTime other && this.Equals(other);

        public bool Equals(TgaDateTime other) => this.DateTime.Equals(other.DateTime);

        public override int GetHashCode() => this.DateTime.GetHashCode();

        public override string ToString() => $"{this.Year:D4}-{this.Month:D2}-{this.Day:D2} {this.Hour:D2}:{this.Minute:D2}:{this.Second:D2}";

        public DateTime DateTime => new(this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second, DateTimeKind.Utc);
        public DateTime? DateTimeNullable { get { try { return this.DateTime; } catch { return null; } } }

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

        public static bool operator ==(TgaDateTime left, TgaDateTime right) => left.Equals(right);

        public static bool operator !=(TgaDateTime left, TgaDateTime right) => !(left == right);
    }

}
