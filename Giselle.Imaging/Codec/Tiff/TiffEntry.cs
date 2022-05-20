using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffEntry
    {
        public TiffTagId TagId { get; set; }
        public TiffValue Value { get; set; }

        public TiffEntry()
        {

        }

        public TiffEntry(TiffRawEntry raw, DataProcessor processor)
        {
            this.TagId = raw.TagId;
            this.Value = raw.ValueType.ValueGenerator();

            if (raw.IsOffset == true)
            {
                var skipping = raw.ValueOrOffset - processor.ReadLength;

                if (skipping < 0)
                {
                    throw new IOException($"Entry Value Offset Matched : Require={raw.ValueOrOffset:X8}, Reading={processor.ReadLength:X8}");
                }
                else
                {
                    processor.SkipByRead(skipping);
                }

                this.Value.Read(raw, processor);
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    var entryProcessor = TiffCodec.CreateTiffProcessor(ms, processor);
                    entryProcessor.WriteInt(raw.ValueOrOffset);

                    ms.Position = 0L;
                    this.Value.Read(raw, entryProcessor);
                }

            }

        }

        public override string ToString()
        {
            return $"id:{this.TagId}, Value:{this.Value}";
        }

    }

}
