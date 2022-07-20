using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Exif
{
    public interface IExifValueIntegers : IExifValue
    {
        int AsSigned { get; set; }

        IEnumerable<int> AsSigneds { get; set; }

        uint AsUnsigned { get; set; }

        IEnumerable<uint> AsUnsigneds { get; set; }
    }

    public abstract class ExifValueIntegers<T> : ExifValueNumbers<T>, IExifValueIntegers where T : IConvertible
    {
        protected abstract int CastToSigned(T value);

        protected abstract uint CastToUnsigned(T value);

        protected abstract T CastToValue(int i);

        protected abstract T CastToValue(uint i);

        public int AsSigned { get => this.AsSigneds.FirstOrDefault(); set => this.AsSigneds = new[] { value }; }

        public IEnumerable<int> AsSigneds { get => this.Values.Select(this.CastToSigned); set => this.Values = value.Select(this.CastToValue).ToArray(); }

        public uint AsUnsigned { get => this.AsUnsigneds.FirstOrDefault(); set => this.AsUnsigneds = new[] { value }; }

        public IEnumerable<uint> AsUnsigneds { get => this.Values.Select(this.CastToUnsigned); set => this.Values = value.Select(this.CastToValue).ToArray(); }
    }

}
