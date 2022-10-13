using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Ico;
using Giselle.Imaging.Codec.Riff;
using Giselle.Imaging.Drawable;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Ani
{
    public class AniContainer
    {
        public static Encoding Encoding => Encoding.ASCII;

        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Copyright { get; set; } = string.Empty;

        public PointI Center { get; set; } = new PointI();
        public int Flags { get; set; } = 0;

        public List<AniStep> Steps { get; } = new List<AniStep>();
        public List<IcoContainer> Frames { get; } = new List<IcoContainer>();

        public AniContainer()
        {

        }

        public AniContainer(Stream input) : this()
        {
            this.Read(input);
        }

        public void Clear()
        {
            this.Name = string.Empty;
            this.Artist = string.Empty;
            this.Copyright = string.Empty;

            this.Center = new PointI();
            this.Flags = 0;

            this.Steps.Clear();
            this.Frames.Clear();
        }

        public void Read(Stream input)
        {
            this.Clear();

            using (var rs = new RiffInputStream(input, true))
            {
                for (RiffChunkHeader header; (header = rs.ReadNextChunk()) != null;)
                {
                    var formType = rs.CurrentPath.FormType;
                    var typeKey = header.TypeKey;
                    var processor = RiffChunk.CreateRiffDataProcessor(rs);

                    if (formType == KnownRiffTypeKeys.Info)
                    {
                        var text = processor.ReadStringUntil0(Encoding);

                        if (typeKey == KnownRiffTypeKeys.InfoName)
                        {
                            this.Name = text;
                        }
                        else if (typeKey == KnownRiffTypeKeys.InfoArtist)
                        {
                            this.Artist = text;
                        }
                        else if (typeKey == KnownRiffTypeKeys.InfoCopyright)
                        {
                            this.Copyright = text;
                        }
                        else
                        {

                        }

                    }

                    else if (typeKey == KnownRiffTypeKeys.AniHeader)
                    {
                        var aniHeader = new AniHeader(rs);

                        this.Center = new PointI(aniHeader.CenterX, aniHeader.CenterY);
                        this.Flags = aniHeader.Flags;

                        for (var i = 0; i < aniHeader.Steps; i++)
                        {
                            this.Steps.Add(new AniStep() { JIFRate = aniHeader.JIFRate, Frame = i });
                        }

                    }
                    else if (typeKey == KnownRiffTypeKeys.Rate)
                    {
                        for (var i = 0; i < this.Steps.Count; i++)
                        {
                            this.Steps[i].JIFRate = processor.ReadInt();
                        }

                    }
                    else if (typeKey == KnownRiffTypeKeys.Sequence)
                    {
                        for (var i = 0; i < this.Steps.Count; i++)
                        {
                            this.Steps[i].Frame = processor.ReadInt();
                        }

                    }
                    else if (typeKey == KnownRiffTypeKeys.Icon)
                    {
                        var iconFile = new IcoContainer(rs);
                        this.Frames.Add(iconFile);
                    }
                    else
                    {

                    }

                }

            }

        }

        public void Write(Stream output)
        {
            var riff = new RiffChunkFile() { FormType = KnownRiffTypeKeys.Acon };

            var info = new RiffChunkList() { FormType = KnownRiffTypeKeys.Info };
            this.AddInfo(info, KnownRiffTypeKeys.InfoName, this.Name);
            this.AddInfo(info, KnownRiffTypeKeys.InfoArtist, this.Artist);
            this.AddInfo(info, KnownRiffTypeKeys.InfoCopyright, this.Copyright);

            if (info.Children.Count > 0)
            {
                riff.Children.Add(info);
            }

            var jifMax = this.Steps.Max(s => new int?(s.JIFRate)) ?? 0;
            var jifUnified = this.Steps.All(s => s.JIFRate == jifMax);
            var stepsSequential = this.IsStepsSequential();

            using (var ms = new MemoryStream())
            {
                new AniHeader()
                {
                    Frames = this.Frames.Count,
                    Steps = this.Steps.Count,
                    CenterX = this.Center.X,
                    CenterY = this.Center.Y,
                    BitCount = this.Frames.Max(f => f.Frames.Max(f2 => new int?(f2.BitsPerPixel))) ?? 0,
                    Planes = 1,
                    JIFRate = jifMax,
                    Flags = this.Flags,
                }.Write(ms);
                riff.Children.Add(new RiffChunkElement(KnownRiffTypeKeys.AniHeader) { Data = ms.ToArray() });
            }

            if (jifUnified == false)
            {
                using (var ms = new MemoryStream())
                {
                    var procesor = RiffChunk.CreateRiffDataProcessor(ms);

                    foreach (var step in this.Steps)
                    {
                        procesor.WriteInt(step.JIFRate);
                    }

                    riff.Children.Add(new RiffChunkElement(KnownRiffTypeKeys.Rate) { Data = ms.ToArray() });
                }

            }

            if (stepsSequential == false)
            {
                using (var ms = new MemoryStream())
                {
                    var procesor = RiffChunk.CreateRiffDataProcessor(ms);

                    foreach (var step in this.Steps)
                    {
                        procesor.WriteInt(step.Frame);
                    }

                    riff.Children.Add(new RiffChunkElement(KnownRiffTypeKeys.Sequence) { Data = ms.ToArray() });
                }

            }

            {
                var frame = new RiffChunkList() { FormType = KnownRiffTypeKeys.Frame };
                riff.Children.Add(frame);

                foreach (var icon in this.Frames)
                {
                    using (var ms = new MemoryStream())
                    {
                        icon.Write(ms);
                        frame.Children.Add(new RiffChunkElement(KnownRiffTypeKeys.Icon) { Data = ms.ToArray() });
                    }

                }

            }

            RiffChunk.WriteChunk(output, riff);
        }

        protected void AddInfo(RiffChunkAbstractList list, int typeKey, string text)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                using (var ms = new MemoryStream())
                {
                    var processor = RiffChunk.CreateRiffDataProcessor(ms);
                    processor.WriteStringWith0(Encoding, text);
                    list.Children.Add(new RiffChunkElement(typeKey) { Data = ms.ToArray() });
                }

            }

        }

        protected bool IsStepsSequential()
        {
            for (var i = 0; i < this.Steps.Count; i++)
            {
                if (this.Steps[i].Frame != i)
                {
                    return false;
                }

            }

            return true;
        }

        public int FrameKinds => this.Frames.Max(f => new int?(f.Frames.Count)) ?? 0;

        public ImageArgb32Container Decode(int frameKind)
        {
            var container = new ImageArgb32Container() { PrimaryCodec = AniCodec.Instance, };

            foreach (var frame in this.Frames)
            {
                container.Add(frame.Frames[frameKind].Decode());
            }

            return container;
        }

    }

}
