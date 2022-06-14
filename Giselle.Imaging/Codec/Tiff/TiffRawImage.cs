using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffRawImage : RawImage<TiffEncodeOptions>
    {
        public List<TiffImageFileDirectory> Directories { get; } = new List<TiffImageFileDirectory>();
        public Dictionary<long, byte[]> Strips { get; } = new Dictionary<long, byte[]>();

        public TiffRawImage()
        {

        }

        public void Read(DataProcessor processor)
        {
            this.Directories.Clear();
            var ifdOffset = processor.ReadInt();

            var strips = new Dictionary<long, byte[]>();

            while (true)
            {
                if (ifdOffset == 0)
                {
                    break;
                }

                var o = processor.ReadLength;
                strips[o] = processor.ReadBytes(ifdOffset - o);

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

            foreach (var directory in this.Directories)
            {
                var offsets = directory.Entries.FirstOrDefault(e => e.TagId == TiffTagId.StripOffsets)?.Value.AsNumbers().AsSigneds.ToArray();
                var counts = directory.Entries.FirstOrDefault(e => e.TagId == TiffTagId.StripByteCounts)?.Value.AsNumbers().AsSigneds.ToArray();

                if (offsets != null && counts != null)
                {
                    if (offsets.Length != counts.Length)
                    {
                        // throw _;
                    }

                    for (var i = 0; i < offsets.Length; i++)
                    {
                        var offset = offsets[i];
                        var count = counts[i];
                        var o = processor.ReadLength;

                        if (strips.TryGetValue(offset, out var prev) == true)
                        {
                            var take = new byte[count];
                            Array.Copy(prev, 0, take, 0, take.Length);
                            var remain = new byte[prev.Length - count];
                            Array.Copy(prev, count, remain, 0, remain.Length);
                            strips[offset] = take;
                            strips[offset + count] = remain;
                            continue;
                        }
                        else if (o < offset)
                        {
                            continue;
                        }
                        else if (o > offset)
                        {
                            processor.SkipByRead(o - offset);
                        }

                        strips[o] = processor.ReadBytes(count);
                    }

                }

            }

            this.Strips.Clear();

            foreach (var pair in strips.OrderBy(p => p.Key))
            {
                if (pair.Value.Length > 0)
                {
                    this.Strips[pair.Key] = pair.Value;
                }

            }

        }

        public override ImageArgb32 Decode()
        {
            foreach (var directory in this.Directories)
            {
                return directory.Decode(this);
            }

            throw new NotImplementedException();
        }

        public override void Encode(ImageArgb32 image, TiffEncodeOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
