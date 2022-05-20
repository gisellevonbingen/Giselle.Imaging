using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public interface ITiffValue
    {
        TiffValueType Type { get; }

        void Read(TiffRawEntry entry, DataProcessor processor);

        void Write(TiffRawEntry entry, DataProcessor processor);
    }

    public abstract class TiffValue : ITiffValue
    {
        public abstract TiffValueType Type { get; }

        public TiffValue()
        {

        }

        public abstract void Read(TiffRawEntry entry, DataProcessor processor);

        public abstract void Write(TiffRawEntry entry, DataProcessor processor);
    }

}
