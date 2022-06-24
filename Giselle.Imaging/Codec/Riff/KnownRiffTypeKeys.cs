using System;
using System.Collections.Generic;
using System.Text;

namespace Giselle.Imaging.Codec.Riff
{
    public static class KnownRiffTypeKeys
    {
        public const int Riff = 0x46464952;
        public const int List = 0x5453494C;

        public const int Info = 0x4F464E49;
        public const int InfoName = 0x4D414E49;
        public const int InfoCopyright = 0x504F4349;
        public const int InfoArtist = 0x54524149;

        /// <summary>
        /// Windows NT Animated Cursor RIFF Files
        /// </summary>
        public const int Acon = 0x4E4F4341;

        public const int AniHeader = 0x68696E61;
        public const int Rate = 0x65746172;
        public const int Sequence = 0x20716573;
        public const int Frame = 0x6D617266;
        public const int Icon = 0x6E6F6369;
    }

}
