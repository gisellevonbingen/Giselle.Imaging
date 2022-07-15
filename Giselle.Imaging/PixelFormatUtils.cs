using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class PixelFormatUtils
    {
        public static IEnumerable<PixelFormat> GetIndexedPixelFormats()
        {
            yield return PixelFormat.Format1bppIndexed;
            yield return PixelFormat.Format2bppIndexed;
            yield return PixelFormat.Format4bppIndexed;
            yield return PixelFormat.Format8bppIndexed;
        }

        public static PixelFormat GetPreferredIndexedFormat(int colorCount) => GetPreferredIndexedFormat(colorCount, GetIndexedPixelFormats());

        public static PixelFormat GetPreferredIndexedFormat(int colorCount, IEnumerable<PixelFormat> pool)
        {
            var lastCount = 0;
            var lastPixelFormat = PixelFormat.Undefined;

            foreach (var e in pool)
            {
                var colorTableLength = e.GetColorTableLength();

                if (colorTableLength >= colorCount)
                {
                    if (lastCount == 0 || colorTableLength < lastCount)
                    {
                        lastPixelFormat = e;
                        lastCount = colorTableLength;
                    }

                }

            }

            return lastPixelFormat;
        }

    }

}
