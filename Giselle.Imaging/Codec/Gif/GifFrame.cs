using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Scan;
using Streams.IO;
using Streams.LZW;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifFrame
    {
        public byte? TransparentColorIndex { get; set; } = null;
        public GifDisposalMethod DisposalMethod { get; set; } = GifDisposalMethod.NoSpecified;
        public ushort FrameDelay { get; set; } = 0;

        public ushort X { get; set; } = 0;
        public ushort Y { get; set; } = 0;
        public ushort Width { get; set; } = 0;
        public ushort Height { get; set; } = 0;
        public Argb32[] LocalColorTable { get; set; } = Array.Empty<Argb32>();
        public byte MinimumLZWCodeSize { get; set; } = 0;
        public MemoryStream CompressedScanData { get; set; } = new MemoryStream();

        public GifFrame()
        {

        }

        public ImageArgb32Frame Decode(GifContainer container, ImageArgb32Frame prev)
        {
            var lzwProcessor = new GifLZWProcessor(this.MinimumLZWCodeSize, 12);

            this.CompressedScanData.Position = 0L;
            using var input = new BitStream(this.CompressedScanData, true);
            using var lzwStream = new LZWStream(input, CompressionMode.Decompress, lzwProcessor);
            using var buffer = new MemoryStream();
            lzwStream.CopyTo(buffer);

            var frame = prev == null ? new ImageArgb32Frame(container.Width, container.Height) : new ImageArgb32Frame(prev);
            frame.PrimaryCodec = GifCodec.Instance;
            frame.PrimaryOptions = new GifFrameSaveOptions() { Delay = this.FrameDelay };

            var colorTable = this.LocalColorTable.Length > 0 ? this.LocalColorTable : container.GlobalColorTable;
            ScanProcessorIndexed.Instance.Decode(new ScanData(this.Width, this.Height, 8)
            {
                Scan = buffer.ToArray(),
                Stride = this.Width,
                ColorTable = colorTable,
                CoordTransformer = new GifDecodeCoordTransformer(prev, this),
                ColorTransformer = new GifDecodeColorTransformer(prev, this),
            }, frame);

            return frame;
        }

        public void Encode(ImageArgb32Frame frame, ImageArgb32Frame prev)
        {

        }

    }

}
