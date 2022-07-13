using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaExtensionArea
    {
        public string AuthorName { get; set; } = string.Empty;
        public string[] AuthorComments { get; set; } = Enumerable.Repeat(string.Empty, TgaRawExtensionArea.AuthorCommentsLength).ToArray();
        public DateTime? DateTimeStamp { get; set; } = new DateTime?();
        public string JobID { get; set; } = string.Empty;
        public TimeSpan JobTime { get; set; } = new TimeSpan();
        public string SoftwareID { get; set; } = string.Empty;
        public TgaSoftwareVersion SoftwareVersion { get; set; } = new TgaSoftwareVersion();
        public Argb32 KeyColor { get; set; } = Argb32.Transparent;
        public ushort PixelAspectRatioNumerator { get; set; } = 0;
        public ushort PixelAspectRatioDenominator { get; set; } = 0;
        public ushort GammaValueRatioNumerator { get; set; } = 0;
        public ushort GammaValueRatioDenominator { get; set; } = 0;

        public float PixelAspectRatio => (float)this.PixelAspectRatioNumerator / (float)this.PixelAspectRatioDenominator;
        public float GammaValueRatio => (float)this.GammaValueRatioNumerator / (float)this.GammaValueRatioDenominator;

        public TgaExtensionArea()
        {

        }

        public TgaExtensionArea(TgaRawExtensionArea raw)
        {
            this.AuthorName = raw.AuthorName;
            this.AuthorComments = raw.AuthorComments;
            this.DateTimeStamp = raw.DateTimeStamp.DateTimeNullable;
            this.JobID = raw.JobID;
            this.JobTime = raw.JobTime.Span;
            this.SoftwareID = raw.SoftwareID;
            this.SoftwareVersion = raw.SoftwareVersion;
            this.KeyColor = new Argb32(raw.KeyColor[3], raw.KeyColor[2], raw.KeyColor[1], raw.KeyColor[0]);
            this.PixelAspectRatioNumerator = raw.PixelAspectRatioNumerator;
            this.PixelAspectRatioDenominator = raw.PixelAspectRatioDenominator;
            this.GammaValueRatioNumerator = raw.GammaValueRatioNumerator;
            this.GammaValueRatioDenominator = raw.GammaValueRatioDenominator;
        }

        public TgaRawExtensionArea Raw => new TgaRawExtensionArea()
        {
            AuthorName = this.AuthorName,
            AuthorComments = this.AuthorComments,
            DateTimeStamp = TgaDateTime.FromDateTime(this.DateTimeStamp),
            JobID = this.JobID,
            JobTime = new TgaTimeSpan(this.JobTime),
            SoftwareID = this.SoftwareID,
            SoftwareVersion = this.SoftwareVersion,
            KeyColor = new byte[TgaRawExtensionArea.KeyColorLength] { this.KeyColor[3], this.KeyColor[2], this.KeyColor[1], this.KeyColor[0] },
            PixelAspectRatioNumerator = this.PixelAspectRatioNumerator,
            PixelAspectRatioDenominator = this.PixelAspectRatioDenominator,
            GammaValueRatioNumerator = this.GammaValueRatioNumerator,
            GammaValueRatioDenominator = this.GammaValueRatioDenominator,
        };

    }

}
