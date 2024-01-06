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

        public byte[] Decompress()
        {
            this.CompressedScanData.Position = 0L;
            using var input = new BitStream(this.CompressedScanData, true, true);
            using var lzwStream = new LZWStream(input, CompressionMode.Decompress, new GifLZWProcessor(this.MinimumLZWCodeSize, MaximumLZWCodeSize), true);
            using var buffer = new MemoryStream();
            lzwStream.CopyTo(buffer);
            return buffer.ToArray();
        }

        public ScanData CreateScanData(GifContainer container, ImageArgb32Frame prev)
        {
            var scan = new ScanData(this.Width, this.Height, BitsPerPixel)
            {
                Stride = this.Width,
                ColorTable = this.LocalColorTable.Length > 0 ? this.LocalColorTable : container.GlobalColorTable,
                CoordTransformer = new GifCoordTransformer(prev, this),
                ColorTransformer = new GifColorTransformer(prev, this),
            };

            if (this.Interlace == true)
            {
                scan.InterlaceBlockWidth = 8;
                scan.InterlaceBlockHeight = 8;
                scan.InterlacePasses = new[]
                {
                    new InterlacePass(0, 0, 1, 8),
                    new InterlacePass(0, 4, 1, 8),
                    new InterlacePass(0, 2, 1, 4),
                    new InterlacePass(0, 1, 1, 2)
                };
            }

            return scan;
        }

        public ImageArgb32Frame Decode(GifContainer container, ImageArgb32Frame prev)
        {
            var frame = prev == null ? new ImageArgb32Frame(container.Width, container.Height) : new ImageArgb32Frame(prev);
            frame.PrimaryCodec = GifCodec.Instance;
            frame.PrimaryOptions = new GifFrameSaveOptions() { UserInput = this.UserInput, Delay = this.FrameDelay, Interlace = this.Interlace };

            var scanData = this.CreateScanData(container, prev);
            scanData.Scan = this.Decompress();
            ScanProcessorIndexed.Instance.Decode(scanData, frame);

            return frame;
        }

        public void Encode(GifContainer container, ImageArgb32Frame prev, ImageArgb32Frame frame)
        {
            var scanData = this.CreateScanData(container, prev);
            scanData.Scan = new byte[this.Height * ScanProcessor.GetBytesPerWidth(this.Width, BitsPerPixel)];
            ScanProcessorIndexed.Instance.Encode(scanData, frame);


            this.CompressedScanData.Position = 0L;
            using var output = new BitStream(this.CompressedScanData, true, true);
            using var lzwStream = new LZWStream(output, CompressionMode.Compress, new GifLZWProcessor(this.MinimumLZWCodeSize, MaximumLZWCodeSize), true);
            lzwStream.Write(scanData.Scan);

            //var buffer = new byte[0x2F];

            //using (var ms = new MemoryStream(scanData.Scan))
            //{
            //    while (true)
            //    {
            //        var len = ms.Read(buffer);

            //        if (len <= 0)
            //        {
            //            break;
            //        }

            //        this.CompressedScanData.Write(buffer, 0, len);
            //        lzwStream.WriteClearCode();
            //    }

            //}

        }

    }

}
