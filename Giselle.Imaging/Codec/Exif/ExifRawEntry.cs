using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifRawEntry
    {
        public const int InfoSize = 2 + 2 + 4 + 4;
        public ExifTagId TagId { get; set; }
        public ExifValueType ValueType { get; set; }
        public int ValueCount { get; set; }
        public int ValueOrOffset { get; set; }

        public ExifRawEntry()
        {

        }

        public ExifRawEntry(KeyValuePair<ExifTagId, ExifValue> pair)
        {
            this.TagId = pair.Key;
            this.ValueType = pair.Value.Type;
            this.ValueCount = pair.Value.RawValueCount;
        }

        public bool IsOffset => this.ValueType.DefaultOffset == true || this.ValuesSize > 4;

        public int ValuesSize => this.ValueType.ElementSize * this.ValueCount;

        public void ReadInfo(DataProcessor processor)
        {
            this.TagId = (ExifTagId)processor.ReadUShort();
            this.ValueType = processor.ReadShort().ToExifEntryType();
            this.ValueCount = processor.ReadInt();
            this.ValueOrOffset = processor.ReadInt();
        }

        public void WriteInfo(DataProcessor processor)
        {
            processor.WriteUShort((ushort)this.TagId);
            processor.WriteShort(this.ValueType.Id);
            processor.WriteInt(this.ValueCount);
            processor.WriteInt(this.ValueOrOffset);
        }

        public override string ToString()
        {
            return $"Id: \"{this.TagId}\", ValueType: \"{this.ValueType}\" , ValueCount: {this.ValueCount:D4}, ValueOrOffset: {this.ValueOrOffset:X8}";
        }

    }

}
