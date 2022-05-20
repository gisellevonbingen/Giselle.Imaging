using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffValueASCII : TiffValue
    {
        private string _Value = string.Empty;
        public string Value { get => this._Value; set => this._Value = value ?? string.Empty; }

        public override TiffValueType Type => TiffValueType.ASCII;

        public override void Read(TiffRawEntry entry, DataProcessor processor)
        {
            var bytes = processor.ReadBytes(entry.ValueCount);

            if (bytes[entry.ValueCount - 1] != '\0')
            {
                throw new IOException("ASCII fianl bytes is not '\\0'");
            }

            this.Value = Encoding.ASCII.GetString(bytes, 0, bytes.Length - 1);
        }

        public override void Write(TiffRawEntry entry, DataProcessor processor)
        {
            var bytes = Encoding.ASCII.GetBytes(this.Value + '\0');
            processor.WriteBytes(bytes);
            entry.ValueCount = bytes.Length;
        }

        public override string ToString()
        {
            return this.Value;
        }

    }

}
