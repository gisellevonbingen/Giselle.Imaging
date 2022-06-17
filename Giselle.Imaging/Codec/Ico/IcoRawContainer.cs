using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.Drawable;
using Giselle.Imaging.Utils;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoRawContainer
    {
        public IcoImageType Type { get; set; } = IcoImageType.Icon;
        public List<IcoRawFrame> Frames { get; } = new List<IcoRawFrame>();
        public List<PointI> CursorHotspots { get; } = new List<PointI>();

        public IcoRawContainer()
        {

        }

        public IcoRawContainer(Stream input) : this()
        {
            this.Read(input);
        }

        public IcoRawContainer(ImageArgb32Container container, IcoSaveOptions options) : this()
        {
            this.Encode(container, options);
        }

        public void Read(Stream input)
        {
            var processor = IcoCodec.CreateIcoProcessor(input);
            var reserved = processor.ReadShort();

            if (reserved != 0)
            {
                throw new IOException($"Invalid data ({reserved}). Reserved must always be 0.");
            }

            var type = (IcoImageType)processor.ReadShort();
            this.Type = type;

            if (IcoCodec.TestType(type) == false)
            {
                throw new IOException($"Invalid type ({type}). Type must always be {IcoImageType.Icon} or {IcoImageType.Cursor}");
            }

            var numberOfImages = processor.ReadShort();
            var infos = new List<IcoImageInfo>();

            for (var i = 0; i < numberOfImages; i++)
            {
                var info = new IcoImageInfo(processor);
                infos.Add(info);
            }

            this.Frames.Clear();
            this.CursorHotspots.Clear();

            for (var i = 0; i < infos.Count; i++)
            {
                var info = infos[i];

                processor.SkipByRead(info.DataOffset - processor.ReadLength);

                using (var ms = new MemoryStream(processor.ReadBytes(info.DataSize)))
                {
                    IcoRawFrame frame;
                    var pngCodec = PngCodec.Instance;

                    if (pngCodec.Test(ms) == true)
                    {
                        frame = new IcoRawFramePng();
                    }
                    else
                    {
                        frame = new IcoRawFrameBmp();
                    }

                    frame.ReadFrame(ms, info);
                    this.Frames.Add(frame);

                    if (type == IcoImageType.Cursor)
                    {
                        this.CursorHotspots.Add(new PointI() { X = info.CursorHotspotLeft, Y = info.CursorHotspotTop });
                    }

                }

            }

        }

        public ImageArgb32Container Decode()
        {
            var container = new ImageArgb32Container()
            {
                PrimaryCodec = IcoCodec.Instance,
                PrimaryOptions = new IcoSaveOptions() { Type = this.Type, CursorHotspots = this.CursorHotspots.ToArray() },
            };
            container.AddRange(this.Frames.Select(f => f.Decode()));
            return container;
        }

        public void Encode(ImageArgb32Container container, IcoSaveOptions options)
        {
            this.Type = options.Type;
            this.Frames.Clear();
            this.CursorHotspots.Clear();

            for (var i = 0; i < container.Count; i++)
            {
                var frame = container[i];
                IcoRawFrame rawFrame;

                if (options.Type == IcoImageType.Icon)
                {
                    rawFrame = new IcoRawFramePng();
                }
                else if (options.Type == IcoImageType.Cursor)
                {
                    rawFrame = new IcoRawFrameBmp();
                    this.CursorHotspots.Add(options.CursorHotspots.TryGet(i, out var cursor) ? cursor : default);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Unknown type: {options.Type}");
                }

                rawFrame.EncodeFrame(frame);
                this.Frames.Add(rawFrame);
            }

        }

        public void Write(Stream output)
        {
            var processor = IcoCodec.CreateIcoProcessor(output);
            processor.WriteShort(0);
            processor.WriteShort((short)this.Type);

            var count = (short)this.Frames.Count;
            processor.WriteShort(count);

            var streamsOffset = processor.WriteLength + IcoImageInfo.Size * count;
            var infos = new List<IcoImageInfo>();
            var streams = new List<MemoryStream>();

            for (var i = 0; i < count; i++)
            {
                var frame = this.Frames[i];
                var info = new IcoImageInfo() { Width = (byte)frame.Width, Height = (byte)frame.Height };

                if (this.Type == IcoImageType.Icon)
                {
                    if (frame is IcoRawFramePng)
                    {

                    }
                    else if (frame is IcoRawFrameBmp)
                    {
                        info.IconColorPlanes = 0;
                        info.IconBitsPerPixel = (short)frame.BitsPerPixel;
                    }

                }
                else if (this.Type == IcoImageType.Cursor)
                {
                    this.CursorHotspots.TryGet(i, out PointI hotspot);
                    info.CursorHotspotLeft = (short)hotspot.X;
                    info.CursorHotspotTop = (short)hotspot.Y;
                    this.CursorHotspots.Add(new PointI() { X = info.CursorHotspotLeft, Y = info.CursorHotspotTop });
                }

                var frameStream = new MemoryStream();
                frame.Write(frameStream, info);
                frameStream.Position = 0L;
                info.DataOffset = (int)streamsOffset;
                info.DataSize = (int)frameStream.Length;
                streamsOffset += frameStream.Length;
                infos.Add(info);
                streams.Add(frameStream);
            }

            for (var i = 0; i < count; i++)
            {
                infos[i].Write(processor);
            }

            for (var i = 0; i < count; i++)
            {
                processor.Write(streams[i].GetBuffer(), 0, (int)streams[i].Length);
            }

        }

    }

}
