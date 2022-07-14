using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec;
using Giselle.Imaging.Codec.ICC;
using Giselle.Imaging.Drawable;
using Giselle.Imaging.Physical;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging
{
    public class ImageArgb32Frame : IImageArgb32
    {
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public byte[] Scan { get; }
        public PhysicalDensity WidthResoulution { get; set; }
        public PhysicalDensity HeightResoulution { get; set; }
        public PhysicalDensity Resolution
        {
            get => PhysicalValueUtils.Max(new[] { this.WidthResoulution, this.HeightResoulution });
            set { this.WidthResoulution = value; this.HeightResoulution = value; }
        }
        private readonly Lazy<IEnumerable<Argb32>> _Colors;
        public IEnumerable<Argb32> Colors => this._Colors.Value;

        private readonly Lazy<IEnumerable<ColorWithPosition>> _ColorWithPositions;
        public IEnumerable<ColorWithPosition> ColorWithPositions => this._ColorWithPositions.Value;

        public ImageCodec PrimaryCodec { get; set; }
        public SaveOptions PrimaryOptions { get; set; }
        public ICCProfile ICCProfile { get; set; }


        public ImageArgb32Frame()
        {
            this.Resolution = new PhysicalDensity(96.0D, PhysicalUnit.Inch);
            this._Colors = new Lazy<IEnumerable<Argb32>>(() => new ImageEnumerable<Argb32>(this, s => s.Frame[s.X, s.Y]));
            this._ColorWithPositions = new Lazy<IEnumerable<ColorWithPosition>>(() => new ImageEnumerable<ColorWithPosition>(this, s => new ColorWithPosition(s.Frame, s.X, s.Y)));
        }

        public ImageArgb32Frame(int width, int height) : this()
        {
            this.Width = width;
            this.Height = height;
            this.Stride = ScanProcessor.GetStride4(width, this.PixelFormat.GetBitsPerPixel());
            this.Scan = new byte[height * this.Stride];
        }

        public ImageArgb32Frame(ScanData scanData, ScanProcessor scanProcessor) : this()
        {
            this.Width = scanData.Width;
            this.Height = scanData.Height;
            this.Stride = ScanProcessor.GetStride4(this.Width, this.PixelFormat.GetBitsPerPixel());
            this.Scan = new byte[this.Height * this.Stride];

            scanProcessor.Decode(scanData, this);
        }

        public PhysicalLength PrintWidth => new PhysicalLength(this.Width / this.WidthResoulution.Value, this.WidthResoulution.Unit);

        public PhysicalLength PrintHeight => new PhysicalLength(this.Height / this.HeightResoulution.Value, this.HeightResoulution.Unit);

        public PixelFormat PixelFormat => PixelFormat.Format32bppArgb8888;

        public int GetOffset(int x, int y) => (y * this.Stride) + (x * 4);

        public int GetOffset(PointI coord) => this.GetOffset(coord.X, coord.Y);

        public Argb32 this[int offset]
        {
            get
            {
                var b = this.Scan[offset + 0];
                var g = this.Scan[offset + 1];
                var r = this.Scan[offset + 2];
                var a = this.Scan[offset + 3];
                return new Argb32(a, r, g, b);
            }

            set
            {
                this.Scan[offset + 0] = value.B;
                this.Scan[offset + 1] = value.G;
                this.Scan[offset + 2] = value.R;
                this.Scan[offset + 3] = value.A;
            }

        }

        public Argb32 this[int x, int y]
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

        public Argb32 this[PointI coord]
        {
            get => this[coord.X, coord.Y];
            set => this[coord.X, coord.Y] = value;
        }

        public Argb32[] GetColorTable(PixelFormat format)
        {
            var colorTableLength = format.GetColorTableLength();

            if (colorTableLength > 0)
            {
                var usedColors = this.Colors.Distinct().ToArray();

                if (usedColors.Length > colorTableLength)
                {
                    throw new ArgumentException($"Image's Used Colors Kind({usedColors.Length}) Exceeds ColorTableLength({colorTableLength})");
                }

                return usedColors;
            }
            else
            {
                return new Argb32[0];
            }

        }

        public void Save(Stream output) => this.Save(output, this.PrimaryCodec, this.PrimaryOptions);

        public void Save(Stream output, ImageCodec codec) => this.Save(output, codec, null);

        public void Save(Stream output, ImageCodec codec, SaveOptions options)
        {
            codec.Write(output, this, options);
        }

        public class ImageEnumerable<T> : IEnumerable<T>
        {
            public ImageArgb32Frame Frame { get; }
            public Func<ImageEnumerator<T>, T> Selector { get; }

            public ImageEnumerable(ImageArgb32Frame frame, Func<ImageEnumerator<T>, T> seletor)
            {
                this.Frame = frame;
                this.Selector = seletor;
            }

            public IEnumerator<T> GetEnumerator() => new ImageEnumerator<T>(this.Frame, this.Selector);

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        }

        public class ImageEnumerator<T> : IEnumerator<T>
        {
            public ImageArgb32Frame Frame { get; }
            public Func<ImageEnumerator<T>, T> Selector { get; }

            public int Length { get; private set; }
            public int Index { get; private set; }
            public int X { get; private set; }
            public int Y { get; private set; }

            public ImageEnumerator(ImageArgb32Frame frame, Func<ImageEnumerator<T>, T> selector)
            {
                this.Frame = frame;
                this.Selector = selector;

                this.Length = frame.Width * frame.Height;
                this.Reset();
            }

            public T Current => this.Selector(this);

            object IEnumerator.Current => this.Current;

            public bool MoveNext()
            {
                var nextIndex = this.Index + 1;
                var width = this.Frame.Width;

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
