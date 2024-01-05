using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Streams.IO;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifContainer
    {
        public const int ApplicationIdentifierLength = 8;
        public const int ApplicationAuthenticationCodeLength = 3;
        public const string ApplicationIdentifierNetscape = "NETSCAPE";
        public const string ApplicationAuthenticationCodeNetscapce = "2.0";

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

            var logicalScreenPackedFields = new GifLogicalScreenPackedFields() { Raw = processor.ReadByte() };

            this.BackgroundColorIndex = processor.ReadByte();
            this.PixelAspectRatio = processor.ReadByte();
            this.GlobalColorTable = ReadColorTable(processor, logicalScreenPackedFields.GlobalColorTableFlag, logicalScreenPackedFields.GlobalColorTableSize);

            var graphicControl = GifGraphicControlPackedFields.Empty;
            var transparentColorIndex = byte.MinValue;
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
                        graphicControl.Raw = gceProcessor.ReadByte();
                        frameDelay = gceProcessor.ReadUShort();
                        transparentColorIndex = gceProcessor.ReadByte();
                    }
                    else if (blockCode1 == GifBlockCode.Comment)
                    {
                        this.Comment = GifCodec.Encoding.GetString(subBlocks[0].ToArray());
                    }
                    else if (blockCode1 == GifBlockCode.ApplicationExtension)
                    {
                        var applicationProcessor = GifCodec.CreateGifProcessor(subBlocks[0]);
                        this.ApplicationIdentifier = GifCodec.Encoding.GetString(applicationProcessor.ReadBytes(ApplicationIdentifierLength));
                        this.ApplicationAuthenticationCode = GifCodec.Encoding.GetString(applicationProcessor.ReadBytes(ApplicationAuthenticationCodeLength));

                        var dataProcessor = GifCodec.CreateGifProcessor(subBlocks[1]);

                        if (this.ApplicationIdentifier.Equals(ApplicationIdentifierNetscape) && this.ApplicationAuthenticationCode.Equals(ApplicationAuthenticationCodeNetscapce))
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
                        TransparentColorIndex = graphicControl.TransparentColorFlag ? transparentColorIndex : null,
                        DisposalMethod = graphicControl.DisposalMethod,
                        UserInput = graphicControl.UserInputFlag,
                        FrameDelay = frameDelay,
                    };
                    frame.X = processor.ReadUShort();
                    frame.Y = processor.ReadUShort();
                    frame.Width = processor.ReadUShort();
                    frame.Height = processor.ReadUShort();

                    var imagePackedFields = new GifImagePackedFields() { Raw = processor.ReadByte() };
                    frame.LocalColorTable = ReadColorTable(processor, imagePackedFields.LocalColorTableFlag, imagePackedFields.LocalColorTableSize);
                    frame.Interlace = imagePackedFields.Interlace;
                    frame.SortFlag = imagePackedFields.SortFlag;
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

        public void Write(Stream output)
        {
            var processor = GifCodec.CreateGifProcessor(output);
            processor.WriteBytes(GifCodec.Signature);
            processor.WriteBytes(GifCodec.EncoderVersion);

            processor.WriteUShort(this.Width);
            processor.WriteUShort(this.Height);

            var gct = this.GlobalColorTable;
            var logicalScreenPackedFields = new GifLogicalScreenPackedFields()
            {
                GlobalColorTableFlag = gct.Length > 0,
                ColorResolution = 8,
                SortFlag = false,
                GlobalColorTableSize = gct.Length,
            }.Normalized;
            processor.WriteByte(logicalScreenPackedFields.Raw);

            processor.WriteByte(this.BackgroundColorIndex);
            processor.WriteByte(this.PixelAspectRatio);
            WriteColorTable(processor, logicalScreenPackedFields.GlobalColorTableFlag, logicalScreenPackedFields.GlobalColorTableSize, gct);


            this.WriteBlock(processor, GifBlockCode.ExtensionIntroducer, p =>
            {
                this.WriteSubBlocks(p, GifBlockCode.ApplicationExtension,
                    p2 =>
                    {
                        p2.WriteBytes(GifCodec.Encoding.GetBytes(ApplicationIdentifierNetscape));
                        p2.WriteBytes(GifCodec.Encoding.GetBytes(ApplicationAuthenticationCodeNetscapce));
                    },
                    p2 =>
                    {
                        p2.WriteByte(1);
                        p2.WriteUShort(this.Repetitions);
                    });
            });

            foreach (var frame in this.Frames)
            {
                this.WriteBlock(processor, GifBlockCode.ExtensionIntroducer, p =>
                {
                    this.WriteSubBlocks(p, GifBlockCode.GraphicControlExtension, p2 =>
                    {
                        p2.WriteByte(new GifGraphicControlPackedFields()
                        {
                            TransparentColorFlag = frame.TransparentColorIndex.HasValue,
                            DisposalMethod = frame.DisposalMethod,
                            UserInputFlag = frame.UserInput,
                        }.Raw);
                        p2.WriteUShort(frame.FrameDelay);
                        p2.WriteByte(frame.TransparentColorIndex ?? 0);
                    });
                });

                this.WriteBlock(processor, GifBlockCode.ImageDescriptor, p =>
                {
                    p.WriteUShort(frame.X);
                    p.WriteUShort(frame.Y);
                    p.WriteUShort(frame.Width);
                    p.WriteUShort(frame.Height);

                    var imagePackedFields = new GifImagePackedFields()
                    {
                        LocalColorTableFlag = frame.LocalColorTable.Length > 0,
                        Interlace = frame.Interlace,
                        SortFlag = frame.SortFlag,
                        LocalColorTableSize = frame.LocalColorTable.Length,
                    };

                    p.WriteByte(imagePackedFields.Raw);
                    WriteColorTable(processor, imagePackedFields.LocalColorTableFlag, imagePackedFields.LocalColorTableSize, frame.LocalColorTable);
                    p.WriteByte(frame.MinimumLZWCodeSize);

                    var buffer = new byte[byte.MaxValue];
                    frame.CompressedScanData.Position = 0L;

                    while (true)
                    {
                        var length = frame.CompressedScanData.Read(buffer);

                        if (length == 0)
                        {
                            break;
                        }

                        p.WriteByte((byte)length);
                        p.Write(buffer, 0, length);
                    }

                    processor.WriteByte(0);
                });
            }


            if (string.IsNullOrEmpty(this.Comment) == false)
            {
                this.WriteBlock(processor, GifBlockCode.ExtensionIntroducer, p =>
                {
                    this.WriteSubBlocks(p, GifBlockCode.Comment, p2 => p2.WriteBytes(GifCodec.Encoding.GetBytes(this.Comment)));
                });
            }

            this.WriteBlock(processor, GifBlockCode.Trailer, p => { });
        }

        public void WriteBlock(DataProcessor processor, GifBlockCode code, Action<DataProcessor> callback)
        {
            processor.WriteByte((byte)code);
            callback(processor);
        }

        public void WriteSubBlocks(DataProcessor processor, GifBlockCode code, params Action<DataProcessor>[] callbacks)
        {
            this.WriteBlock(processor, code, p =>
            {
                foreach (var callback in callbacks)
                {
                    using var ms = new MemoryStream();
                    callback(GifCodec.CreateGifProcessor(ms));

                    var subBlock = ms.ToArray().Take(byte.MaxValue).ToArray();
                    p.WriteByte((byte)subBlock.Length);
                    p.WriteBytes(subBlock);
                }

                p.WriteByte(0);
            });
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

        public static Argb32[] ReadColorTable(DataProcessor processor, bool present, int size)
        {
            if (present == true)
            {
                var table = new Argb32[size];
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

        public static void WriteColorTable(DataProcessor processor, bool present, int normalizedSize, Argb32[] table)
        {
            if (present == true && table != null)
            {
                var buffer = new byte[3];

                foreach (var color in table.TakeFixSize(0, normalizedSize, Argb32.Black))
                {
                    buffer[0] = color.R;
                    buffer[1] = color.G;
                    buffer[2] = color.B;
                    processor.WriteBytes(buffer);
                }

            }

        }

    }

}
