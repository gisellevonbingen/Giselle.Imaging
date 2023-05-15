using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Streams.IO;

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

        public TgaRawHeader(DataProcessor input)
        {
            this.Read(input);
        }

        public void Read(DataProcessor input)
        {
            this.IDLength = input.ReadByte();
            this.ColorMapType = input.ReadByte();
            var imageType = input.ReadByte();
            this.ImageType = (TgaImageType)(imageType & 0x07);
            this.Compression = (imageType & 0x08) == 0x08;

            // ColorMap Spec
            this.ColorMapFirstEntryOffset = input.ReadUShort();
            this.ColorMapLength = input.ReadUShort();
            this.ColorMapEntryBitDepth = input.ReadByte();

            // Image Spec
            this.OriginX = input.ReadUShort();
            this.OriginY = input.ReadUShort();
            this.Width = input.ReadUShort();
            this.Height = input.ReadUShort();
            this.PixelDepth = input.ReadByte();
            this.Descriptor = new BitVector32(input.ReadByte());
        }

        public void Write(DataProcessor output)
        {
            output.WriteByte(this.IDLength);
            output.WriteByte(this.ColorMapType);
            output.WriteByte((byte)(((byte)this.ImageType & 0x07) | (this.Compression ? 0x08 : 0x00)));

            // ColorMap Spec
            output.WriteUShort(this.ColorMapFirstEntryOffset);
            output.WriteUShort(this.ColorMapLength);
            output.WriteByte(this.ColorMapEntryBitDepth);

            // Image Spec
            output.WriteUShort(this.OriginX);
            output.WriteUShort(this.OriginY);
            output.WriteUShort(this.Width);
            output.WriteUShort(this.Height);
            output.WriteByte(this.PixelDepth);
            output.WriteByte((byte)this.Descriptor.Data);
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
