using System;

namespace Moton.CoAP.Client
{
    public class CoapClientConfigurationInvalidException : Exception
    {
        public CoapClientConfigurationInvalidException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
