using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Exif
{
    public interface IExifValueIntegers : IExifValue
    {
        int AsSigned { get; }

        IEnumerable<int> AsSigneds { get; }

        uint AsUnsigned { get; }

        IEnumerable<uint> AsUnsigneds { get; }
    }

    public abstract class ExifValueIntegers<T> : ExifValueNumbers<T>, IExifValueIntegers where T : IConvertible
    {
        public int AsSigned => this.AsSigneds.FirstOrDefault();

        public IEnumerable<int> AsSigneds => this.Values.Select(v => v.ToInt32(null));

        public uint AsUnsigned => this.AsUnsigneds.FirstOrDefault();

        public IEnumerable<uint> AsUnsigneds => this.Values.Select(v => v.ToUInt32(null));
    }

}
