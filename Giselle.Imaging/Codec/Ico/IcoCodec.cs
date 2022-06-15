using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoCodec : ImageCodec
    {
        public const bool IsLittleEndian = true;

        public static IcoCodec Instance { get; } = new IcoCodec();

        public static DataProcessor CreateIcoProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = IsLittleEndian };

        public IcoCodec()
        {

        }

        public override int BytesForTest => 4;

        public override bool SupportMultiFrame => true;

        public override string PrimaryExtension => GetExtension(IcoImageType.Icon);

        public override IEnumerable<string> GetExtensions()
        {
            yield return GetExtension(IcoImageType.Icon);
            yield return GetExtension(IcoImageType.Cursor);
        }

        public string GetExtension(IcoImageType type)
        {
            if (type == IcoImageType.Icon) return "ico";
            else if (type == IcoImageType.Icon) return "cur";
            else return string.Empty;
        }

        public override bool Test(MemoryStream stream)
        {
            var processor = CreateIcoProcessor(stream);
            var reserved = processor.ReadShort();
            var type = (IcoImageType)processor.ReadShort();
            return reserved == 0 && TestType(type);
        }

        public static bool TestType(IcoImageType type)
        {
            return type == IcoImageType.Icon || type == IcoImageType.Cursor;
        }

        public override ImageArgb32Container Read(Stream input)
        {
            var processor = CreateIcoProcessor(input);
            var reserved = processor.ReadShort();

            if (reserved != 0)
            {
                throw new IOException($"Invalid data ({reserved}). Reserved must always be 0.");
            }

            var type = (IcoImageType)processor.ReadShort();

            if (TestType(type) == false)
            {
                throw new IOException($"Invalid type ({type}). Type must always be {IcoImageType.Icon} or {IcoImageType.Cursor}");
            }

            var numberOfImages = processor.ReadShort();
            var infos = new List<IcoImageInfo>();
            var sizes = new List<int>();
            var offsets = new List<int>();

            for (var i = 0; i < numberOfImages; i++)
            {
                var info = new IcoImageInfo(processor);
                infos.Add(info);

                var size = processor.ReadInt();
                sizes.Add(size);

                var offset = processor.ReadInt();
                offsets.Add(offset);
            }

            var container = new ImageArgb32Container()
            {
                PrimaryCodec = this,
                PrimaryOptions = new IcoSaveOptions() { Type = type },
            };

            for (var i = 0; i < infos.Count; i++)
            {
                var size = sizes[i];
                var offset = offsets[i];

                processor.SkipByRead(offset - processor.ReadLength);

                using (var ms = new MemoryStream(processor.ReadBytes(size)))
                {
                    try
                    {
                        var subContainer = PngCodec.Instance.Read(ms);
                        container.AddRange(subContainer);
                        continue;
                    }
                    catch (Exception)
                    {

                    }

                    ms.Position = 0L;
                    var imageProcessor = BmpCodec.CreateBmpProcessor(ms);
                    var headerSize = imageProcessor.ReadInt();
                    var width = imageProcessor.ReadInt();
                    var height = imageProcessor.ReadInt();
                    var frame = BmpCodec.Instance.Read(ms, width, height / 2);
                    container.Add(frame);
                }

            }

            return container;
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
