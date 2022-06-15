using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.Codec.Png;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoSaveOptions : SaveOptions
    {
        public IcoImageType Type { get; set; } = IcoImageType.Icon;
    }

}
