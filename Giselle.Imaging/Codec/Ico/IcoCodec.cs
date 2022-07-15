using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.IO;
using Giselle.Imaging.Scan;

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

        public override int BytesForTest => 6;

        public override bool SupportMultiFrame => true;

        public override IEnumerable<string> GetExtensions()
        {
            yield return GetExtension(IcoImageType.Icon);
            yield return GetExtension(IcoImageType.Cursor);
        }

        public string GetExtension(IcoImageType type)
        {
            if (type == IcoImageType.Icon) return "ico";
            else if (type == IcoImageType.Cursor) return "cur";
            else return string.Empty;
        }

        protected override bool TestAsStream(Stream stream)
        {
            var processor = CreateIcoProcessor(stream);
            var reserved = processor.ReadShort();
            var type = (IcoImageType)processor.ReadShort();
            var frameCount = processor.ReadShort();
            return reserved == 0 && TestType(type) && frameCount > 0;
        }

        public static bool TestType(IcoImageType type)
        {
            return type == IcoImageType.Icon || type == IcoImageType.Cursor;
        }

        public override ImageArgb32Container Read(Stream input)
        {
            var rawContainer = new IcoContainer(input);
            return rawContainer.Decode();
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions options)
        {
            throw new NotSupportedException();
        }

        public override PixelFormat GetPreferredPixelFormat(ImageArgb32Frame frame)
        {
            throw new NotSupportedException();
        }

    }

}
