using System;

namespace Moton.CoAP.Exceptions
{
    public class CoapProtocolViolationException : Exception
    {
        public CoapProtocolViolationException(string message) : base(message)
        {
        }
    }
}