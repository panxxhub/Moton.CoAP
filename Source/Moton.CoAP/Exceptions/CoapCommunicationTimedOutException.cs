using System;

namespace Moton.CoAP.Exceptions
{
    public class CoapCommunicationTimedOutException : CoapCommunicationException
    {
        public CoapCommunicationTimedOutException()
            : base("CoAP communication timed out.", null)
        {
        }

        public CoapCommunicationTimedOutException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
