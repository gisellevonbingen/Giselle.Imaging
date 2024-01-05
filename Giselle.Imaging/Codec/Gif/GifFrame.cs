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
        public const int MaximumLZWCodeSize = 12;
        public const int BitsPerPixel = 8;

        public bool UserInput { get; set; } = false;
        public byte? TransparentColorIndex { get; set; } = null;
        public GifDisposalMethod DisposalMethod { get; set; } = GifDisposalMethod.NoSpecified;
        public ushort FrameDelay { get; set; } = 0;

        public ushort X { get; set; } = 0;
        public ushort Y { get; set; } = 0;
        public ushort Width { get; set; } = 0;
        public ushort Height { get; set; } = 0;
        public Argb32[] LocalColorTable { get; set; } = Array.Empty<Argb32>();
        public bool Interlace { get; set; } = false;
        public bool SortFlag { get; set; } = false;
        public byte MinimumLZWCodeSize { get; set; } = 0;
        public MemoryStream CompressedScanData { get; set; } = new MemoryStream();

        public GifFrame()
        {

        }

        public ImageArgb32Frame Decode(GifContainer container, ImageArgb32Frame prev)
        {
            this.CompressedScanData.Position = 0L;
            using var input = new BitStream(this.CompressedScanData, true);
            using var lzwStream = new LZWStream(input, CompressionMode.Decompress, new GifLZWProcessor(this.MinimumLZWCodeSize, MaximumLZWCodeSize));
            using var buffer = new MemoryStream();
            lzwStream.CopyTo(buffer);

            var frame = prev == null ? new ImageArgb32Frame(container.Width, container.Height) : new ImageArgb32Frame(prev);
            frame.PrimaryCodec = GifCodec.Instance;
            frame.PrimaryOptions = new GifFrameSaveOptions() { Delay = this.FrameDelay };

            ScanProcessorIndexed.Instance.Decode(new ScanData(this.Width, this.Height, BitsPerPixel)
            {
                Scan = buffer.ToArray(),
                Stride = this.Width,
                ColorTable = this.LocalColorTable.Length > 0 ? this.LocalColorTable : container.GlobalColorTable,
                CoordTransformer = new GifCoordTransformer(prev, this),
                ColorTransformer = new GifColorTransformer(prev, this),
            }, frame);

            return frame;
        }

        public void Encode(GifContainer container, ImageArgb32Frame prev, ImageArgb32Frame frame)
        {
            var scan = new ScanData(this.Width, this.Height, BitsPerPixel)
            {
                Scan = new byte[this.Height * ScanProcessor.GetBytesPerWidth(this.Width, BitsPerPixel)],
                Stride = this.Width,
                ColorTable = this.LocalColorTable.Length > 0 ? this.LocalColorTable : container.GlobalColorTable,
                CoordTransformer = new GifCoordTransformer(prev, this),
                ColorTransformer = new GifColorTransformer(prev, this),
            };

            ScanProcessorIndexed.Instance.Encode(scan, frame);

            this.CompressedScanData.Position = 0L;
            using var output = new BitStream(this.CompressedScanData, true);
            using var lzwStream = new LZWStream(output, CompressionMode.Compress, new GifLZWProcessor(this.MinimumLZWCodeSize, MaximumLZWCodeSize));
            lzwStream.Write(scan.Scan);
        }

    }

}
