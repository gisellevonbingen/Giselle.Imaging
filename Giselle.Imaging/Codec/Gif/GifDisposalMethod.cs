using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Gif
{
    public enum GifDisposalMethod : byte
    {
        NoSpecified = 0,
        DoNotDispose = 1,
        RestoreToBackgroundColor = 2,
        RestoreToPrevious = 3,
    }

}
