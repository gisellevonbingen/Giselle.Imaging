﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ImageArgb32
    {
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public byte[] Scan { get; }
        public double WidthResoulution { get; set; }
        public double HeightResoulution { get; set; }
        public double Resolution
        {
            get => Math.Max(this.WidthResoulution, this.HeightResoulution);
            set { this.WidthResoulution = value; this.HeightResoulution = value; }
        }
        private readonly Lazy<IEnumerable<Argb32>> _Colors;
        public IEnumerable<Argb32> Colors => this._Colors.Value;

        private readonly Lazy<IEnumerable<ColorWithPosition>> _ColorWithPositions;
        public IEnumerable<ColorWithPosition> ColorWithPositions => this._ColorWithPositions.Value;


        public ImageArgb32()
        {
            this.Resolution = 96.0D;
            this._Colors = new Lazy<IEnumerable<Argb32>>(() => new ImageEnumerable<Argb32>(this, s => s.Image[s.X, s.Y]));
            this._ColorWithPositions = new Lazy<IEnumerable<ColorWithPosition>>(() => new ImageEnumerable<ColorWithPosition>(this, s => new ColorWithPosition(s.Image, s.X, s.Y)));
        }

        public ImageArgb32(int width, int height) : this()
        {
            this.Width = width;
            this.Height = height;
            this.Stride = ScanProcessor.GetStridePadding4(width, this.PixelFormat.GetBitsPerPixel());
            this.Scan = new byte[height * this.Stride];
        }

        public ImageArgb32(ScanData scanData, ScanProcessor scanProcessor) : this()
        {
            this.Width = scanData.Width;
            this.Height = scanData.Height;
            this.Stride = scanProcessor.GetFormatStride(this.Width);
            this.Scan = new byte[this.Height * this.Stride];

            scanProcessor.Read(scanData, this.Scan);
        }

        public PixelFormat PixelFormat => PixelFormat.Format32bppArgb8888;

        public int GetOffset(int x, int y) => (y * this.Stride) + (x * 4);

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

        public class ImageEnumerable<T> : IEnumerable<T>
        {
            public ImageArgb32 Image { get; }
            public Func<ImageEnumerator<T>, T> Selector { get; }

            public ImageEnumerable(ImageArgb32 image, Func<ImageEnumerator<T>, T> seletor)
            {
                this.Image = image;
                this.Selector = seletor;
            }

            public IEnumerator<T> GetEnumerator() => new ImageEnumerator<T>(this.Image, this.Selector);

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        }

        public class ImageEnumerator<T> : IEnumerator<T>
        {
            public ImageArgb32 Image { get; }
            public Func<ImageEnumerator<T>, T> Selector { get; }

            public int Length { get; private set; }
            public int Index { get; private set; }
            public int X { get; private set; }
            public int Y { get; private set; }

            public ImageEnumerator(ImageArgb32 image, Func<ImageEnumerator<T>, T> selector)
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