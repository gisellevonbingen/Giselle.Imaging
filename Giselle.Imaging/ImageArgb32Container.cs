using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ImageArgb32Container : IList<ImageArgb32Frame>
    {
        private readonly List<ImageArgb32Frame> _Frames;

        public ImageArgb32Container()
        {
            this._Frames = new List<ImageArgb32Frame>();
        }

        public int Count => this._Frames.Count;

        public bool IsReadOnly => false;

        public ImageArgb32Frame this[int index] { get => this._Frames[index]; set => this._Frames[index] = value; }

        public void Add(ImageArgb32Frame frame) => this._Frames.Add(frame);

        public void Clear() => this._Frames.Clear();

        public bool Contains(ImageArgb32Frame frame) => this._Frames.Contains(frame);

        public void CopyTo(ImageArgb32Frame[] array, int arrayIndex) => this._Frames.CopyTo(array, arrayIndex);

        public IEnumerator<ImageArgb32Frame> GetEnumerator() => this._Frames.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Remove(ImageArgb32Frame frame) => this._Frames.Remove(frame);

        public int IndexOf(ImageArgb32Frame frame) => this._Frames.IndexOf(frame);

        public void Insert(int index, ImageArgb32Frame frame) => this._Frames.Insert(index, frame);

        public void RemoveAt(int index) => this._Frames.RemoveAt(index);
    }

}
