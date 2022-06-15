using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public abstract class ExifValueArray<T> : ExifValue
    {
        private T[] _Values = new T[0];
        public T[] Values
        {
            get => this._Values;
            set => this._Values = value ?? new T[0];
        }

        public T Value
        {
            get => this.Values[0];
            set => this.Values = new T[] { value };
        }

        public ExifValueArray()
        {

        }

        public override void Read(ExifRawEntry entry, DataProcessor processor)
        {
            var count = entry.ValueCount;
            this.Values = new T[count];

            for (var i = 0; i < count; i++)
            {
                this.Values[i] = this.ReadElement(processor);
            }

        }

        public override void Write(ExifRawEntry entry, DataProcessor processor)
        {
            var count = this.Values.Length;
            entry.ValueCount = count;

            for (var i = 0; i < count; i++)
            {
                this.WriteElement(this.Values[i], processor);
            }

        }

        public abstract T ReadElement(int raw);

        public abstract T ReadElement(DataProcessor processor);

        public abstract int WriteElement(T element);

        public abstract void WriteElement(T element, DataProcessor processor);

        public override string ToString()
        {
            return $"[{string.Join(", ", this.Values)}]";
        }

    }

}
