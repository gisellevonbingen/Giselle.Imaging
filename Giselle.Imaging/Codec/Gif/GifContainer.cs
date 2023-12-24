using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.IO;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifContainer
    {
        public List<GifFrame> Frames { get; } = new List<GifFrame>();
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte BackgroundColorIndex { get; set; } = 0;
        public byte PixelAspectRatio { get; set; } = 0;
        public Argb32[] GlobalColorTable { get; set; } = Array.Empty<Argb32>();

        public string Comment { get; set; } = string.Empty;
        public string ApplicationIdentifier { get; set; } = string.Empty;
        public string ApplicationAuthenticationCode { get; set; } = string.Empty;
        public ushort Repetitions { get; set; } = 0;

        public GifContainer()
        {

        }

        public GifContainer(Stream input) : this()
        {
            this.Read(input);
        }

        public void Read(Stream input)
        {
            this.Frames.Clear();

            var processor = GifCodec.CreateGifProcessor(input);
            var signature = processor.ReadBytes(3);
            var version = processor.ReadBytes(3);

            if (GifCodec.Instance.Test(signature) == false)
            {
                throw new IOException();
            }

            this.Width = processor.ReadUShort();
            this.Height = processor.ReadUShort();

            var gctInfo = new BitVector32(processor.ReadByte());
            var gctPresent = gctInfo[0x80];
            var gctSize = gctInfo[BitVector32.CreateSection(0x07)] + 1;

            this.BackgroundColorIndex = processor.ReadByte();
            this.PixelAspectRatio = processor.ReadByte();
            this.GlobalColorTable = ReadColorTable(processor, gctPresent, gctSize);

            var transparentColorFlag = false;
            var userInputFlag = false;
            var transparentColorIndex = new byte?();
            var disposalMethod = GifDisposalMethod.NoSpecified;
            var frameDelay = ushort.MinValue;

            while (true)
            {
                var blockCode0 = (GifBlockCode)processor.ReadByte();

                if (blockCode0 == GifBlockCode.ExtensionIntroducer)
                {
                    var blockCode1 = (GifBlockCode)processor.ReadByte();
                    var subBlocks = new List<MemoryStream>();

                    while (true)
                    {
                        var subBlockLength = processor.ReadByte();

                        if (subBlockLength == 0)
                        {
                            break;
                        }

                        var subBlockBytes = processor.ReadBytes(subBlockLength);
                        subBlocks.Add(new MemoryStream(subBlockBytes));
                    }

                    if (blockCode1 == GifBlockCode.GraphicControlExtension)
                    {
                        var gceProcessor = GifCodec.CreateGifProcessor(subBlocks[0]);
                        var bitFields = gceProcessor.ReadByte();
                        transparentColorFlag = (bitFields & 0x01) > 0;
                        userInputFlag = (bitFields & 0x02) > 0;
                        disposalMethod = (GifDisposalMethod)((bitFields & 0x1C) >> 2);
                        frameDelay = gceProcessor.ReadUShort();
                        transparentColorIndex = gceProcessor.ReadByte();
                    }
                    else if (blockCode1 == GifBlockCode.Comment)
                    {
                        this.Comment = Encoding.ASCII.GetString(subBlocks[0].ToArray());
                    }
                    else if (blockCode1 == GifBlockCode.ApplicationExtension)
                    {
                        var applicationProcessor = GifCodec.CreateGifProcessor(subBlocks[0]);
                        this.ApplicationIdentifier = Encoding.ASCII.GetString(applicationProcessor.ReadBytes(8));
                        this.ApplicationAuthenticationCode = Encoding.ASCII.GetString(applicationProcessor.ReadBytes(3));

                        var dataProcessor = GifCodec.CreateGifProcessor(subBlocks[1]);

                        if (this.ApplicationIdentifier.Equals("NETSCAPE"))
                        {
                            // Netscape
                            var dataSubBlockIndex = dataProcessor.ReadByte(); // Alwasy 1
                            this.Repetitions = dataProcessor.ReadUShort();
                        }

                    }
                    else
                    {

                    }

                }
                else if (blockCode0 == GifBlockCode.ImageDescriptor)
                {
                    var frame = new GifFrame()
                    {
                        TransparentColorIndex = transparentColorFlag ? transparentColorIndex : null,
                        DisposalMethod = disposalMethod,
                        FrameDelay = frameDelay,
                    };
                    frame.X = processor.ReadUShort();
                    frame.Y = processor.ReadUShort();
                    frame.Width = processor.ReadUShort();
                    frame.Height = processor.ReadUShort();

                    var lctInfo = new BitVector32(processor.ReadByte());
                    var lctPresent = lctInfo[0x80];
                    var lctSize = lctInfo[BitVector32.CreateSection(0x07)] + 1;
                    frame.LocalColorTable = ReadColorTable(processor, lctPresent, lctSize);
                    frame.MinimumLZWCodeSize = processor.ReadByte();

                    while (true)
                    {
                        var dataLength = processor.ReadByte();

                        if (dataLength == 0)
                        {
                            break;
                        }

                        var data = processor.ReadBytes(dataLength);
                        frame.CompressedScanData.Write(data);
                    }

                    this.Frames.Add(frame);
                }
                else if (blockCode0 == GifBlockCode.Trailer)
                {
                    break;
                }

            }

        }

        public ImageArgb32Container Decode()
        {
            var container = new ImageArgb32Container()
            {
                PrimaryCodec = GifCodec.Instance,
                PrimaryOptions = new GifSaveOptions()
                {
                    Repetitions = this.Repetitions,
                },
            };

            for (var i = 0; i < this.Frames.Count; i++)
            {
                var prev = i == 0 ? null : container[i - 1];
                var frame = this.Frames[i].Decode(this, prev);
                container.Add(frame);
            }

            return container;
        }

        public static Argb32[] ReadColorTable(DataProcessor processor, bool present, int depth)
        {
            if (present == true)
            {
                var table = new Argb32[1 << depth];
                var buffer = new byte[3];

                for (var i = 0; i < table.Length; i++)
                {
                    processor.ReadBytes(buffer);
                    table[i] = new Argb32()
                    {
                        A = byte.MaxValue,
                        R = buffer[0],
                        G = buffer[1],
                        B = buffer[2],
                    };

                }

                return table;
            }
            else
            {
                return Array.Empty<Argb32>();
            }

        }

    }

}
