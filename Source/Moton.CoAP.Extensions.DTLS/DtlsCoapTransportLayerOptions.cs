namespace Moton.CoAP.Extensions.DTLS
{
    public sealed class DtlsCoapTransportLayerOptions
    {
        public IDtlsCredentials Credentials
        {
            get;
            set;
        } = null!;

        public DtlsVersion DtlsVersion
        {
            get;
            set;
        } = DtlsVersion.V1_2;
    }
}
