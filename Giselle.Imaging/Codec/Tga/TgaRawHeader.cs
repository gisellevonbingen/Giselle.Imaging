using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaRawHeader
    {
        public const int Length = 18;
        public const int ColorMapSpecificationLength = 5;
        public const int ImageSpecificationLength = 10;

        public static BitVector32.Section DescriptorAlphaChannelSection { get; private set; } = BitVector32.CreateSection(0x0F);
        public const int DescriptorFlipXBitMask = 0b00010000;
        public const int DescriptorFlipYBitMask = 0b00100000;

        public byte IDLength { get; set; }
        public byte ColorMapType { get; set; }
        public TgaImageType ImageType { get; set; }
        public bool Compression { get; set; }

        public ushort ColorMapFirstEntryOffset { get; set; }
        public ushort ColorMapLength { get; set; }
        public byte ColorMapEntryBitDepth { get; set; }

        public ushort OriginX { get; set; }
        public ushort OriginY { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte PixelDepth { get; set; }

        private BitVector32 _Descriptor;
        public BitVector32 Descriptor { get => this._Descriptor; set => this._Descriptor = value; }

        public TgaRawHeader()
        {

        }

        public TgaRawHeader(Stream input)
        {
            this.Read(input);
        }

        public void Read(Stream input)
        {
            var processor = TgaCodec.CreateTgaProcessor(input);
            this.IDLength = processor.ReadByte();
            this.ColorMapType = processor.ReadByte();
            var imageType = processor.ReadByte();
            this.ImageType = (TgaImageType)(imageType & 0x07);
            this.Compression = (imageType & 0x08) == 0x08;

            // ColorMap Spec
            this.ColorMapFirstEntryOffset = processor.ReadUShort();
            this.ColorMapLength = processor.ReadUShort();
            this.ColorMapEntryBitDepth = processor.ReadByte();

            // Image Spec
            this.OriginX = processor.ReadUShort();
            this.OriginY = processor.ReadUShort();
            this.Width = processor.ReadUShort();
            this.Height = processor.ReadUShort();
            this.PixelDepth = processor.ReadByte();
            this.Descriptor = new BitVector32(processor.ReadByte());
        }

        public void Write(Stream output)
        {
            var processor = TgaCodec.CreateTgaProcessor(output);
            processor.WriteByte(this.IDLength);
            processor.WriteByte(this.ColorMapType);
            processor.WriteByte((byte)(((byte)this.ImageType & 0x07) |(this.Compression ? 0x08 : 0x00)));

            // ColorMap Spec
            processor.WriteUShort(this.ColorMapFirstEntryOffset);
            processor.WriteUShort(this.ColorMapLength);
            processor.WriteByte(this.ColorMapEntryBitDepth);

            // Image Spec
            processor.WriteUShort(this.OriginX);
            processor.WriteUShort(this.OriginY);
            processor.WriteUShort(this.Width);
            processor.WriteUShort(this.Height);
            processor.WriteByte(this.PixelDepth);
            processor.WriteByte((byte)this.Descriptor.Data);
        }

        public TgaColorMapTypeKind ColorMapTypeKind
        {
            get
            {
                var type = this.ColorMapType;

                if (type == 0) return TgaColorMapTypeKind.NoColorMap;
                else if (type == 1) return TgaColorMapTypeKind.Present;
                else if (type < 128) return TgaColorMapTypeKind.ReservedByTruevision;
                else return TgaColorMapTypeKind.AvailableForDeveloper;
            }

        }

        public byte AlphaChannelBits { get => (byte)this._Descriptor[DescriptorAlphaChannelSection]; set => this._Descriptor[DescriptorAlphaChannelSection] = value; }

        public bool FlipX { get => this._Descriptor[DescriptorFlipXBitMask]; set => this._Descriptor[DescriptorFlipXBitMask] = value; }

        public bool FlipY { get => this._Descriptor[DescriptorFlipYBitMask]; set => this._Descriptor[DescriptorFlipYBitMask] = value; }

    }

}
