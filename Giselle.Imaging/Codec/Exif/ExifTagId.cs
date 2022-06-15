using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Exif
{
    public enum ExifTagId : ushort
    {
        NewSubfileType = 0x00FE,
        OldSubfileType = 0x00FF,
        ImageWidth = 0x0100,
        ImageLength = 0x0101,
        BitsPerSample = 0x0102,
        Compression = 0x0103,

        PhotometricInterpretation = 0x0106,
        Threshholding = 0x0107,
        CellWidth = 0x0108,
        CellLength = 0x0109,
        FillOrder = 0x010A,
        ImageDescription = 0x010E,
        Make = 0x010F,
        Model = 0x0110,
        StripOffsets = 0x0111,
        Orientation = 0x0112,
        SamplesPerPixel = 0x0115,
        RowsPerStrip = 0x0116,
        StripByteCounts = 0x0117,
        MinSampleValue = 0x0118,
        MaxSampleValue = 0x0119,
        XResolution = 0x011A,
        YResolution = 0x011B,
        PlanarConfiguration = 0x011C,

        FreeOffsets = 0x0120,
        FreeByteCounts = 0x0121,
        GrayResponseUnit = 0x0122,
        GrayResponseCurve = 0x0123,

        ResolutionUnit = 0x0128,

        TransferFunction = 0x012D,

        Software = 0x0131,
        DateTime = 0x0132,

        Artist = 0x013B,
        HostComputer = 0x013C,
        Predictor = 0x013D,
        WhitePoint = 0x013E,
        PrimaryChromaticities = 0x013F,
        ColorMap = 0x0140,
        HalftoneHints = 0x0141,
        TileWidth = 0x0142,
        TileLength = 0x0143,
        TileOffsets = 0x0144,
        TileByteCounts = 0x0145,
        InkSet = 0x014C,
        InkNames = 0x014D,
        NumberOfInks = 0x014E,

        DotRange = 0x0150,
        TargetPrinter = 0x0151,
        ExtraSamples = 0x0152,
        SampleFormat = 0x0153,
        SMinSampleValue = 0x0154,
        SMaxSampleValue = 0x0155,
        TransferRange = 0x0156,

        ReferenceBlackWhite = 0x0214,


        Copyright = 0x8298,
    }

}
