using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
using Giselle.Imaging.Utils;

namespace Giselle.Imaging.Codec.ICC
{
    public class ICCRawTag
    {
        public int Signature { get; set; }
        public int DataElementOffset { get; set; }
        public int DataElementSize { get; set; }

        public ICCRawTag()
        {

        }

        public ICCRawTag(DataProcessor processor) : this()
        {
            this.Read(processor);
        }

        public void Read(DataProcessor processor)
        {
            this.Signature = processor.ReadInt();
            this.DataElementOffset = processor.ReadInt();
            this.DataElementSize = processor.ReadInt();
        }

        public void Write(DataProcessor processor)
        {
            processor.WriteInt(this.Signature);
            processor.WriteInt(this.DataElementOffset);
            processor.WriteInt(this.DataElementSize);
        }

        public override string ToString()
        {
            return $"Signature: \"{this.Signature.ToASCIIString(ICCProfile.IsLittleEndian)}\", DataElementOffset: {this.DataElementOffset:X8}, DataElementSize: {this.DataElementSize:X8}";
        }

    }

}
