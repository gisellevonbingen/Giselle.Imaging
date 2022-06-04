using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffRawImage : RawImage<TiffEncodeOptions>
    {
        public List<TiffImageFileDirectory> Directories { get; } = new List<TiffImageFileDirectory>();

        public TiffRawImage()
        {

        }

        public void Read(DataProcessor processor)
        {
            this.Directories.Clear();
            var ifdOffset = processor.ReadInt();

            while (true)
            {
                if (ifdOffset == 0)
                {
                    break;
                }

                var o = processor.ReadLength;
                var payload = processor.ReadBytes(ifdOffset - o);

                var entryCount = processor.ReadShort();
                var rawEntries = new List<TiffRawEntry>();

                for (var i = 0; i < entryCount; i++)
                {
                    var entry = new TiffRawEntry();
                    entry.ReadInfo(processor);
                    rawEntries.Add(entry);
                }

                ifdOffset = processor.ReadInt();

                var directory = new TiffImageFileDirectory();
                this.Directories.Add(directory);

                foreach (var rawEntry in rawEntries)
                {
                    var entry = new TiffEntry(rawEntry, processor);
                    directory.Entries.Add(entry);
                }

            }

        }

        public override ImageArgb32 Decode()
        {
            foreach (var directory in this.Directories)
            {
                directory.Decode();
            }

            throw new NotImplementedException();
        }

        public override void Encode(ImageArgb32 image, TiffEncodeOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
