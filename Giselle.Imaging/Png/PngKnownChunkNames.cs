using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Png
{
    public static class PngKnownChunkNames
    {
        // Critical chunks
        /// <summary>
        /// Image Header
        /// </summary>
        public const string IHDR = "IHDR";
        /// <summary>
        /// Palette Table
        /// </summary>
        public const string PLTE = "PLTE";
        /// <summary>
        /// Image Data
        /// </summary>
        public const string IDAT = "IDAT";
        /// <summary>
        /// Image Trailer
        /// </summary>
        public const string IEND = "IEND";

        // Ancillary chunks
        public const string cHRM = "cHRM";
        public const string gAMA = "gAMA";
        public const string iCCP = "iCCP";
        public const string sBIT = "sBIT";
        public const string sRGB = "sRGB";
        public const string bKGD = "bKGD";
        public const string hIST = "hIST";
        public const string tRNS = "tRNS";
        public const string pHYs = "pHYs";
        public const string sPLT = "sPLT";
        public const string tIME = "tIME";
        public const string iTXt = "iTXt";
        public const string tEXt = "tEXt";
        public const string zTXt = "zTXt";
    }

}
