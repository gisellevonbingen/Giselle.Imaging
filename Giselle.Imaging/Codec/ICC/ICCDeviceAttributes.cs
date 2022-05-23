using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.ICC
{
    [Flags]
    public enum ICCDeviceAttributes : long
    {
        /// <summary>
        /// Reflective (0) or Transparency (1)
        /// </summary>
        Transparency = 1,
        /// <summary>
        /// Glossy (0) or Matte (1)
        /// </summary>
        Matte = 2,
        /// <summary>
        /// Media polarity, Positive (0) or Negative (1) 
        /// </summary>
        MediaPolarityNegative = 4,
        /// <summary>
        /// Colour Media (0) or Black & White Media (1)
        /// </summary>
        BlackWhiteMedia = 8,
    }

}
