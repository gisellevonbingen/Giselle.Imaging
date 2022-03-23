using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Checksum
{
    [Serializable]
    public class CRCException : Exception
    {
        public CRCException() { }
        public CRCException(string message) : base(message) { }
        public CRCException(string message, Exception inner) : base(message, inner) { }
        protected CRCException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

}
