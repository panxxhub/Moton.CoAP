namespace Moton.CoAP.Client
{
    public class CoapObserveOptions
    {
        public CoapObserveRequest Request { get; set; } = null!;

        public ICoapResponseHandler ResponseHandler { get; set; } = null!;
    }
}