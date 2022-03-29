using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public class Pipe : IDisposable
    {
        private readonly List<byte> Buffer;
        private readonly AutoResetEvent ResetEvent;
        public Stream Reader { get; }
        public Stream Writer { get; }

        public Pipe()
        {
            this.Buffer = new List<byte>();
            this.ResetEvent = new AutoResetEvent(false);
            this.Reader = new PipeReadStream(this.Buffer, this.ResetEvent);
            this.Writer = new PipeWriteStream(this.Buffer, this.ResetEvent);
        }

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            this.ResetEvent.Dispose();
        }

        ~Pipe() => this.Dispose(false);

    }

}
