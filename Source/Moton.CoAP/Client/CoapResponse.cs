namespace Moton.CoAP.Client
{
    public sealed class CoapResponse
    {
        public CoapResponseStatusCode StatusCode
        {
            get; set;
        }

        public CoapResponseOptions Options
        {
            get; set;
        } = new CoapResponseOptions();

        public byte[]? Payload
        {
            get; set;
        }
    }
}

