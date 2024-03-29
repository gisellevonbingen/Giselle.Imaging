﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Giselle.Imaging.Scan;
using Giselle.Imaging.Utils;
using Streams.IO;

namespace Giselle.Imaging.Codec.ICC
{
    public class ICCProfile
    {
        public const string Extension = "icc";
        public const int FileSignature = 0x61637370;
        public const bool IsLittleEndian = false;
        public const byte ASCIISuffix = 0x20;
        public const int IDLength = 16;
        public const int ReservedsLength = 28;

        public static ICCProfileWriteOptions DefaultOptions { get; set; } = new ICCProfileWriteOptions();

        public static DataProcessor CreateProcessor(Stream stream) => new(stream) { IsLittleEndian = IsLittleEndian };

        public string PreferredCMMType { get; set; } = string.Empty;
        public Version Version { get; set; } = default;
        public ICCProfileClass Class { get; set; } = default;
        public ICCColorSpaceType ColorSpace { get; set; } = default;
        public string PSC { get; set; } = string.Empty;
        public ICCDateTime FirstCreated { get; set; } = default;

        public string PrimaryPlatform { get; set; } = string.Empty;
        public ICCProfileFlags Flags { get; set; } = default;
        public string DeviceManufacturer { get; set; } = string.Empty;
        public string DeviceModel { get; set; } = string.Empty;
        public ICCDeviceAttributes DeviceAttributes { get; set; } = default;

        public ICCRenderingIndent RenderingIntent { get; set; } = default;
        public ICCXYZ PSCIlluminant { get; set; } = default;
        public string Creator { get; set; } = string.Empty;
        public byte[] ID { get; set; } = new byte[IDLength];
        public byte[] Reserveds { get; set; } = new byte[ReservedsLength];

        public List<ICCTag> Tags { get; } = new List<ICCTag>();

        public ICCProfile()
        {

        }

        public ICCProfile(ICCProfile other)
        {
            this.PreferredCMMType = other.PreferredCMMType;
            this.Version = other.Version;
            this.Class = other.Class;
            this.ColorSpace = other.ColorSpace;
            this.PSC = other.PSC;
            this.FirstCreated = other.FirstCreated;
            this.PrimaryPlatform = other.PrimaryPlatform;
            this.Flags = other.Flags;
            this.DeviceManufacturer = other.DeviceManufacturer;
            this.DeviceModel = other.DeviceModel;
            this.DeviceAttributes = other.DeviceAttributes;
            this.RenderingIntent = other.RenderingIntent;
            this.PSCIlluminant = other.PSCIlluminant;
            this.Creator = other.Creator;
            this.ID = other.ID;
            this.Reserveds = other.Reserveds.ToArray();

            this.Tags = new List<ICCTag>(other.Tags.Select(t => t.Clone()));
        }

        public ICCProfile(Stream stream) : this()
        {
            this.Read(stream);
        }

        public ICCProfile Clone() => new(this);

        public void Read(Stream stream)
        {
            var processor = CreateProcessor(stream);
            var isLittleEndian = processor.IsLittleEndian;
            var lengthWidthSelf = processor.ReadInt();
            this.PreferredCMMType = processor.ReadInt().ToASCIIString(isLittleEndian);
            this.Version = processor.ReadVersion();
            this.Class = (ICCProfileClass)processor.ReadInt();
            this.ColorSpace = (ICCColorSpaceType)processor.ReadInt();
            this.PSC = processor.ReadInt().ToASCIIString(isLittleEndian);
            this.FirstCreated = new ICCDateTime(processor);

            var fileSignature = processor.ReadInt();

            if (fileSignature != FileSignature)
            {
                throw new IOException($"File Signature Mismatched : Reading={fileSignature:X8}, Require={FileSignature:X8}");
            }

            this.PrimaryPlatform = processor.ReadInt().ToASCIIString(isLittleEndian);
            this.Flags = (ICCProfileFlags)processor.ReadInt();
            this.DeviceManufacturer = processor.ReadInt().ToASCIIString(isLittleEndian);
            this.DeviceModel = processor.ReadInt().ToASCIIString(isLittleEndian);
            this.DeviceAttributes = (ICCDeviceAttributes)processor.ReadLong();
            this.RenderingIntent = (ICCRenderingIndent)processor.ReadInt();
            this.PSCIlluminant = new ICCXYZ(processor);
            this.Creator = processor.ReadInt().ToASCIIString(isLittleEndian);
            this.ID = processor.ReadBytes(IDLength);
            this.Reserveds = processor.ReadBytes(ReservedsLength);

            var tagCount = processor.ReadInt();
            var rawTags = new List<ICCRawTag>();

            for (var i = 0; i < tagCount; i++)
            {
                rawTags.Add(new ICCRawTag(processor));
            }

            this.Tags.Clear();

            for (var i = 0; i < rawTags.Count; i++)
            {
                var rawTag = rawTags[i];
                byte[] data = null;
                var skipping = rawTag.DataElementOffset - processor.ReadLength;

                if (i > 0 && rawTags[i - 1].DataElementOffset == rawTag.DataElementOffset)
                {
                    data = this.Tags[i - 1].Data.ToArray();
                }
                else if (skipping < 0)
                {
                    throw new IOException($"Invalid Tag DataElementOffset : Reading={processor.ReadLength:X8}, Require={rawTag.DataElementOffset:X8}");
                }
                else
                {
                    processor.SkipByRead(skipping);
                }

                if (data == null)
                {
                    data = processor.ReadBytes(rawTag.DataElementSize);
                }

                this.Tags.Add(new ICCTag() { Signature = rawTag.Signature, Data = data });
            }

            processor.SkipByRead(lengthWidthSelf - processor.ReadLength);
        }

        public void Write(Stream stream) => this.Write(stream, DefaultOptions);

        public void Write(Stream stream, ICCProfileWriteOptions options)
        {
            var processor = CreateProcessor(stream);
            var isLittleEndian = processor.IsLittleEndian;
            var tagInfoLength = 12 * this.Tags.Count;

            var rawTags = new List<ICCRawTag>();
            var tagValuesCursor = 128 + 4 + tagInfoLength;
            var tagDataElementOffset = tagValuesCursor;

            for (var i = 0; i < this.Tags.Count; i++)
            {
                var tag = this.Tags[i];
                rawTags.Add(new ICCRawTag() { Signature = tag.Signature, DataElementSize = tag.Data.Length, DataElementOffset = tagDataElementOffset });

                var withPadLength = ScanProcessor.ApplyPadding(tag.Data.Length, 4);

                if (i + 1 < this.Tags.Count)
                {
                    if (options.Compact == true && this.Tags[i + 1].Data.SequenceEqual(tag.Data) == true)
                    {
                        continue;
                    }
                    else
                    {
                        tagDataElementOffset += withPadLength;
                    }

                }

                tagValuesCursor += withPadLength;
            }

            processor.WriteInt(tagValuesCursor);
            processor.WriteInt(this.PreferredCMMType.ToASCIIInt32(isLittleEndian));
            processor.WriteVersion(this.Version);
            processor.WriteInt((int)this.Class);
            processor.WriteInt((int)this.ColorSpace);
            processor.WriteInt(this.PSC.ToASCIIInt32(isLittleEndian));
            this.FirstCreated.Write(processor);
            processor.WriteInt(FileSignature);
            processor.WriteInt(this.PrimaryPlatform.ToASCIIInt32(isLittleEndian));
            processor.WriteInt((int)this.Flags);
            processor.WriteInt(this.DeviceManufacturer.ToASCIIInt32(isLittleEndian));
            processor.WriteInt(this.DeviceModel.ToASCIIInt32(isLittleEndian));
            processor.WriteLong((long)this.DeviceAttributes);
            processor.WriteInt((int)this.RenderingIntent);
            this.PSCIlluminant.Write(processor);
            processor.WriteInt(this.Creator.ToASCIIInt32(isLittleEndian));
            processor.WriteBytes(this.ID.TakeFixSize(0, IDLength).ToArray());
            processor.WriteBytes(this.Reserveds.TakeFixSize(0, ReservedsLength).ToArray());

            processor.WriteInt(rawTags.Count);

            for (var i = 0; i < rawTags.Count; i++)
            {
                rawTags[i].Write(processor);
            }

            for (var i = 0; i < this.Tags.Count; i++)
            {
                var tag = this.Tags[i];
                var rawTag = rawTags[i];

                if (i > 0 && options.Compact == true && rawTags[i - 1].DataElementOffset == rawTag.DataElementOffset)
                {
                    continue;
                }

                processor.WriteBytes(tag.Data);

                var pad = ScanProcessor.ApplyPadding(rawTag.DataElementSize, 4) - rawTag.DataElementSize;

                for (var j = 0; j < pad; j++)
                {
                    processor.WriteByte(0);
                }

            }

        }

    }

}
