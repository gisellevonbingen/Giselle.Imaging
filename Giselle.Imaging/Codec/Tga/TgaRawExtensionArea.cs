using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Giselle.Imaging.IO;
using Giselle.Imaging.Text;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaRawExtensionArea
    {
        public const int Length = 495;
        public const int AuthorNameLength = 40;
        public const int AuthorCommentsLength = 4;
        public const int AuthorCommentLineLength = 80;
        public const int JobIDLength = 40;
        public const int SoftwareIDLength = 40;
        public const int KeyColorLength = 4;

        public string AuthorName { get; set; } = string.Empty;
        public string[] AuthorComments { get; set; } = Enumerable.Repeat(string.Empty, AuthorCommentsLength).ToArray();
        public TgaDateTime DateTimeStamp { get; set; } = new TgaDateTime();
        public string JobID { get; set; } = string.Empty;
        public TgaTimeSpan JobTime { get; set; } = new TgaTimeSpan();
        public string SoftwareID { get; set; } = string.Empty;
        public TgaSoftwareVersion SoftwareVersion { get; set; } = new TgaSoftwareVersion();
        public byte[] KeyColor { get; set; } = new byte[KeyColorLength];
        public ushort PixelAspectRatioNumerator { get; set; } = 0;
        public ushort PixelAspectRatioDenominator { get; set; } = 0;
        public ushort GammaValueRatioNumerator { get; set; } = 0;
        public ushort GammaValueRatioDenominator { get; set; } = 0;
        public int ColorCorrectionOffset { get; set; } = 0;
        public int PostageStampOffset { get; set; } = 0;
        public int ScanLineOffset { get; set; } = 0;
        public byte AttributesType { get; set; } = 0;

        public TgaRawExtensionArea()
        {

        }

        public TgaRawExtensionArea(DataProcessor input) : this()
        {
            this.Read(input);
        }

        public void Read(DataProcessor input)
        {
            var encoding = TgaCodec.Encoding;
            var lengthWithSelf = input.ReadUShort();
            this.AuthorName = encoding.GetStringUntil0(input.ReadBytes(AuthorNameLength + 1));
            this.AuthorComments = EnumerableUtils.Repeat(AuthorCommentsLength, i => encoding.GetStringUntil0(input.ReadBytes(AuthorCommentLineLength + 1))).ToArray();
            this.DateTimeStamp = new TgaDateTime(input);
            this.JobID = encoding.GetStringUntil0(input.ReadBytes(JobIDLength + 1));
            this.JobTime = new TgaTimeSpan(input);
            this.SoftwareID = encoding.GetStringUntil0(input.ReadBytes(SoftwareIDLength + 1));
            this.SoftwareVersion = new TgaSoftwareVersion(input);
            this.KeyColor = input.ReadBytes(KeyColorLength);
            this.PixelAspectRatioNumerator = input.ReadUShort();
            this.PixelAspectRatioDenominator = input.ReadUShort();
            this.GammaValueRatioNumerator = input.ReadUShort();
            this.GammaValueRatioDenominator = input.ReadUShort();
            this.ColorCorrectionOffset = input.ReadInt();
            this.PostageStampOffset = input.ReadInt();
            this.ScanLineOffset = input.ReadInt();
            this.AttributesType = input.ReadByte();
        }

        public void Write(DataProcessor output)
        {
            var encoding = TgaCodec.Encoding;
            output.WriteUShort(Length);
            output.WriteBytes(encoding.GetBytesWith0(this.AuthorName, AuthorNameLength));
            Array.ForEach(this.AuthorComments, s => output.WriteBytes(encoding.GetBytesWith0(s, AuthorCommentLineLength)));
            this.DateTimeStamp.Write(output);
            output.WriteBytes(encoding.GetBytesWith0(this.JobID, JobIDLength));
            this.JobTime.Write(output);
            output.WriteBytes(encoding.GetBytesWith0(this.SoftwareID, SoftwareIDLength));
            this.SoftwareVersion.Write(output);
            output.WriteBytes(this.KeyColor);
            output.WriteUShort(this.PixelAspectRatioNumerator);
            output.WriteUShort(this.PixelAspectRatioDenominator);
            output.WriteUShort(this.GammaValueRatioNumerator);
            output.WriteUShort(this.GammaValueRatioDenominator);
            output.WriteInt(this.ColorCorrectionOffset);
            output.WriteInt(this.PostageStampOffset);
            output.WriteInt(this.ScanLineOffset);
            output.WriteByte(this.AttributesType);
        }

    }

}
