using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffRawEntry
    {
        public TiffTagId TagId { get; set; }
        public TiffValueType ValueType { get; set; }
        public int ValueCount { get; set; }
        public int ValueOrOffset { get; set; }

        public TiffRawEntry()
        {

        }

        public bool IsOffset => this.ValueType.DefaultOffset == true || this.ValueCount > 1;

        public void ReadInfo(DataProcessor processor)
        {
            this.TagId = (TiffTagId)processor.ReadUShort();
            this.ValueType = processor.ReadShort().ToTiffEntryType();
            this.ValueCount = processor.ReadInt();
            this.ValueOrOffset = processor.ReadInt();
        }

        public override string ToString()
        {
            return $"Id:{this.TagId}, ValueType:{this.ValueType}, ValueCount:{this.ValueCount:D4}, ValueOrOffset:{this.ValueOrOffset:X8}";
        }

    }

}
