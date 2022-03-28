using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Tiff
{
    public class TiffEntry
    {
        public TiffTagId TagId { get; set; }
        public TiffValueType ValueType { get; set; }
        public int ValueCount { get; set; }
        public int ValueOrOffset { get; set; }

        public TiffEntry()
        {

        }

        public bool IsOffset => this.ValueType.DefaultOffset == true || this.ValueCount > 1;

        public void Read(DataProcessor processor)
        {
            this.TagId = (TiffTagId)processor.ReadUShort();
            this.ValueType = processor.ReadShort().ToTiffEntryType();
            this.ValueCount = processor.ReadInt();

            if (this.IsOffset == true)
            {
                this.ValueOrOffset = processor.ReadInt();
            }
            else
            {
                var type = this.CastValueTypeInteger();
                var o1 = processor.ReadLength;
                this.ValueOrOffset = type.ReadAsSigned(processor);
                var o2 = processor.ReadLength;
                processor.SkipByRead(4 - (o2 - o1));
            }

        }

        public override string ToString()
        {
            return $"id:{this.TagId}, ValueType:{this.ValueType}, ValueCount:{this.ValueCount:D4}, ValueOrOffset:{this.ValueOrOffset:X8}";
        }

    }

}
