using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class Image32Argb
    {
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public PixelFormat Format => PixelFormat.Format32bppArgb;
        public byte[] Scan { get; }
        public double WidthResoulution { get; set; }
        public double HeightResoulution { get; set; }
        public double Resolution
        {
            get => Math.Max(this.WidthResoulution, this.HeightResoulution);
            set { this.WidthResoulution = value; this.HeightResoulution = value; }
        }
        private readonly Lazy<IEnumerable<Color>> _Colors;
        public IEnumerable<Color> Colors => this._Colors.Value;

        private readonly Lazy<IEnumerable<ColorWithPosition>> _ColorWithPositions;
        public IEnumerable<ColorWithPosition> ColorWithPositions => this._ColorWithPositions.Value;


        public Image32Argb()
        {
            this.Resolution = 96.0D;
            this._Colors = new Lazy<IEnumerable<Color>>(() => new ImageEnumerable<Color>(this, s => s.Image[s.X, s.Y]));
            this._ColorWithPositions = new Lazy<IEnumerable<ColorWithPosition>>(() => new ImageEnumerable<ColorWithPosition>(this, s => new ColorWithPosition(s.Image, s.X, s.Y)));
        }

        public Image32Argb(int width, int height) : this(width, height, ScanProcessor.GetStride(width, PixelFormat.Format32bppArgb.GetBitsPerPixel()), null)
        {

        }

        public Image32Argb(int width, int height, int stride, byte[] scan) : this()
        {
            this.Width = width;
            this.Height = height;
            this.Stride = stride;
            this.Scan = scan ?? new byte[height * stride];
        }

        public Image32Argb(ScanData scanData) : this()
        {
            ScanProcessor processor;

            if (scanData.UseBitFields == true)
            {
                processor = ScanProcessor.CreateScanProcessor(scanData.Format.GetBitsPerPixel(), scanData.AMask, scanData.RMask, scanData.GMask, scanData.BMask);
            }
            else
            {
                processor = ScanProcessor.GetScanProcessor(scanData.Format);
            }

            this.Width = scanData.Width;
            this.Height = scanData.Height;
            this.Stride = processor.GetFormatStride(this.Width);
            this.Scan = new byte[this.Height * this.Stride];
            this.Resolution = scanData.Resolution;

            processor.Read(scanData, this.Scan);
        }

        public int GetOffset(int x, int y) => (y * this.Stride) + (x * 4);

        public Color this[int offset]
        {
            get
            {
                var b = this.Scan[offset + 0];
                var g = this.Scan[offset + 1];
                var r = this.Scan[offset + 2];
                var a = this.Scan[offset + 3];
                return Color.FromArgb(a, r, g, b);
            }

            set
            {
                this.Scan[offset + 0] = value.B;
                this.Scan[offset + 1] = value.G;
                this.Scan[offset + 2] = value.R;
                this.Scan[offset + 3] = value.A;
            }

        }

        public Color this[int x, int y]
        {
            get
            {
                var offset = this.GetOffset(x, y);
                return this[offset];
            }

            set
            {
                var offset = this.GetOffset(x, y);
                this[offset] = value;
            }

        }

        public Bitmap ToBitmap()
        {
            unsafe
            {
                fixed (byte* scan0 = this.Scan)
                {
                    return new Bitmap(this.Width, this.Height, this.Stride, this.Format, (IntPtr)scan0);
                }

            }

        }

        public ScanData Encode(PixelFormat format)
        {
            return this.Encode(format, null);
        }

        public ScanData Encode(PixelFormat format, Color[] colorTable)
        {
            var colorTableLength = format.GetColorTableLength();

            if (colorTableLength > 0)
            {
                if (colorTable == null)
                {
                    colorTable = this.Colors.Distinct().ToArray();
                }

                if (colorTable.Length > colorTableLength)
                {
                    throw new ArgumentException($"image's used colors kind({colorTable.Length}) exceeds ColorTableLength({colorTableLength})");
                }

            }

            var processor = ScanProcessor.GetScanProcessor(format);
            var stride = ScanProcessor.GetStride(this.Width, format.GetBitsPerPixel());
            var scan = new byte[this.Height * stride];
            var scanData = new ScanData(this.Width, this.Height, stride, format, scan, colorTable)
            {
                WidthResoulution = this.WidthResoulution,
                HeightResoulution = this.HeightResoulution,
            };
            processor.Write(scanData, this.Scan);

            return scanData;
        }

        public class ImageEnumerable<T> : IEnumerable<T>
        {
            public Image32Argb Image { get; }
            public Func<ImageEnumerator<T>, T> Selector { get; }

            public ImageEnumerable(Image32Argb image, Func<ImageEnumerator<T>, T> seletor)
            {
                this.Image = image;
                this.Selector = seletor;
            }

            public IEnumerator<T> GetEnumerator() => new ImageEnumerator<T>(this.Image, this.Selector);

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        }

        public class ImageEnumerator<T> : IEnumerator<T>
        {
            public Image32Argb Image { get; }
            public Func<ImageEnumerator<T>, T> Selector { get; }

            public int Length { get; private set; }
            public int Index { get; private set; }
            public int X { get; private set; }
            public int Y { get; private set; }

            public ImageEnumerator(Image32Argb image, Func<ImageEnumerator<T>, T> selector)
            {
                this.Image = image;
                this.Selector = selector;

                this.Length = image.Width * image.Height;
                this.Reset();
            }

            public T Current => this.Selector(this);

            object IEnumerator.Current => this.Current;

            public bool MoveNext()
            {
                var nextIndex = this.Index + 1;
                var width = this.Image.Width;

                this.Index = nextIndex;
                this.X = nextIndex % width;
                this.Y = nextIndex / width;

                return nextIndex < this.Length;
            }

            public void Reset()
            {
                this.Index = -1;
                this.X = -1;
                this.Y = -1;
            }

            public void Dispose()
            {

            }

        }

    }

}
