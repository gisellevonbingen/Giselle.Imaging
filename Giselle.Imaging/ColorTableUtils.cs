using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class ColorTableUtils
    {
        public static (Argb32[] RgbTable, byte[] AlphaTable) SplitColorTable(Argb32[] colorTable)
        {
            var rgbTable = new Argb32[colorTable.Length];
            var alphaTable = new byte[colorTable.Length];

            for (var i = 0; i < colorTable.Length; i++)
            {
                var argb = colorTable[i];
                rgbTable[i] = argb.DeriveA(byte.MaxValue);
                alphaTable[i] = argb.A;
            }

            return (rgbTable, alphaTable);
        }

        public static Argb32[] MergeColorTable(Argb32[] rgbTable, byte[] alphaTable)
        {
            var colorTable = new Argb32[rgbTable.Length];

            for (var i = 0; i < colorTable.Length; i++)
            {
                var rgb = rgbTable[i];
                var a = i < alphaTable.Length ? alphaTable[i] : byte.MaxValue;
                colorTable[i] = rgb.DeriveA(a);
            }

            return colorTable;
        }

        public static bool RequireWriteAlphaTable(byte[] alphaTable) => alphaTable.Aggregate(byte.MaxValue, Math.Min) < byte.MaxValue;

    }

}
