namespace Moton.CoAP.Extensions.DTLS
{
    public sealed class PreSharedKey : IDtlsCredentials
    {
        public byte[] Identity
        {
            get; set;
        } = null!;

        public byte[] Key
        {
            get; set;
        } = null!;
    }
}
