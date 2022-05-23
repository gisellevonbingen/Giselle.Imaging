using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.ICC
{
    public enum ICCProfileClass : int
    {
        /// <summary>
        /// scnr
        /// </summary>
        InputDevice = 0x73636E72,
        /// <summary>
        /// mntr
        /// </summary>
        DisplayDevice = 0x6D6E7472,
        /// <summary>
        /// prtr
        /// </summary>
        OutputDevice = 0x70727472,
        /// <summary>
        /// link
        /// </summary>
        DeviceLink = 0x6C696E6B,
        /// <summary>
        /// spac
        /// </summary>
        ColorSpace = 0x73706163,
        /// <summary>
        /// abst
        /// </summary>
        Abstract = 0x61627374,
        /// <summary>
        /// nmcl
        /// </summary>
        NamedColor = 0x6E6D636C,
    }

}
