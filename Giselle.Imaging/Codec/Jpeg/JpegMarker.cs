using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Jpeg
{
    public enum JpegMarker : ushort
    {
        /// <summary>
        /// Start of Image
        /// </summary>
        SOI = 0xFFD8,
        /// <summary>
        /// End of Image
        /// </summary>
        EOI = 0xFFD9,

        /// <summary>
        /// Start of Frame (Baseline DCT)<br/>
        /// Non-Differential, Huffman Coding
        /// </summary>
        SOF_0 = 0xFFC0,
        /// <summary>
        /// Start of Frame (Extended Sequential DCT)<br/>
        /// Non-Differential, Huffman Coding
        /// </summary>
        SOF_1 = 0xFFC1,
        /// <summary>
        /// Start of Frame (Progressive DCT)<br/>
        /// Non-Differential, Huffman Coding
        /// </summary>
        SOF_2 = 0xFFC2,
        /// <summary>
        /// Start of Frame (Lossless (Sequential))<br/>
        /// Non-Differential, Huffman Coding
        /// </summary>
        SOF_3 = 0xFFC3,
        /// <summary>
        /// Start of Frame (Differential Sequential DCT)<br/>
        /// Differential, Huffman Coding
        /// </summary>
        SOF_5 = 0xFFC5,
        /// <summary>
        /// Start of Frame (Differential Progressive DCT)<br/>
        /// Differential, Huffman Coding
        /// </summary>
        SOF_6 = 0xFFC6,
        /// <summary>
        /// Start of Frame (Differential Lossless (Sequential))<br/>
        /// Differential, Huffman Coding
        /// </summary>
        SOF_7 = 0xFFC7,
        /// <summary>
        /// Reserved for JPEG Extensions
        /// </summary>
        JPG = 0xFFC8,
        /// <summary>
        /// Start of Frame (Extended sequential DCT)<br/>
        /// Non-Differential, Arithmetic Coding
        /// </summary>
        SOF_9 = 0xFFC9,
        /// <summary>
        /// Start of Frame (Progressive DCT)<br/>
        /// Non-Differential, Arithmetic Coding
        /// </summary>
        SOF_A = 0xFFCA,
        /// <summary>
        /// Start of Frame (Lossless (Sequential))<br/>
        /// Non-Differential, Arithmetic Coding
        /// </summary>
        SOF_B = 0xFFCB,
        /// <summary>
        /// Start of Frame (Differential sequential DCT)<br/>
        /// Differential, Arithmetic Coding
        /// </summary>
        SOF_D = 0xFFCD,
        /// <summary>
        /// Start of Frame (Differential progressive DCT)<br/>
        /// Differential, Arithmetic Coding
        /// </summary>
        SOF_E = 0xFFCE,
        /// <summary>
        /// Start of Frame (Differential Lossless (Sequential))<br/>
        /// Differential, Arithmetic Coding
        /// </summary>
        SOF_F = 0xFFCF,

        /// <summary>
        /// Define Huffman table(s)
        /// </summary>
        DHT = 0xFFC4,
        /// <summary>
        /// Start of Scan
        /// </summary>
        SOS = 0xFFDA,
        /// <summary>
        /// Define Quantization Table(s)
        /// </summary>
        DQT = 0xFFDB,

        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_0 = 0xFFE0,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_1 = 0xFFE1,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_2 = 0xFFE2,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_3 = 0xFFE3,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_4 = 0xFFE4,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_5 = 0xFFE5,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_6 = 0xFFE6,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_7 = 0xFFE7,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_8 = 0xFFE8,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_9 = 0xFFE9,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_A = 0xFFEA,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_B = 0xFFEB,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_C = 0xFFEC,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_D = 0xFFED,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_E = 0xFFEE,
        /// <summary>
        /// Reserved for Application Segments
        /// </summary>
        APP_F = 0xFFEF,
    }

}
