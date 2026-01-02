using System;
using System.IO;

namespace Moton.CoAP.Exceptions
{
    public class CoapCommunicationException : IOException
    {
        public CoapCommunicationException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
