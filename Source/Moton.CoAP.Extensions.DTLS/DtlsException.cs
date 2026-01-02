using System;
using Moton.CoAP.Exceptions;

namespace Moton.CoAP.Extensions.DTLS
{
    public sealed class DtlsException : CoapCommunicationException
    {
        public DtlsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public byte ReceivedAlert { get; set; }
    }
}
