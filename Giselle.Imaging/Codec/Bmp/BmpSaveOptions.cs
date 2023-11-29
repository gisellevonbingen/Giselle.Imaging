using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Bmp
{
    public class BmpSaveOptions : SaveOptions<BmpSaveOptions>
    {
        public BmpBitsPerPixel BitsPerPixel { get; set; }
        public BmpCompressionMethod? Compression { get; set; }

        public BmpSaveOptions()
        {
            this.BitsPerPixel = BmpBitsPerPixel.Undefined;
            this.Compression = null;
        }

        public BmpSaveOptions(BmpSaveOptions other) : base(other)
        {
            this.BitsPerPixel = other.BitsPerPixel;
            this.Compression = other.Compression;
        }

        public override BmpSaveOptions Clone() => new(this);
    }

}
