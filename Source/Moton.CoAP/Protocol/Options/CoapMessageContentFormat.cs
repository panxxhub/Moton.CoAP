namespace Moton.CoAP.Protocol.Options
{
    /// <summary>
    /// CoAP Content-Format registry values (IANA).
    /// See: https://www.iana.org/assignments/core-parameters/core-parameters.xhtml#content-formats
    /// </summary>
    public enum CoapMessageContentFormat
    {
        /// <summary>
        /// text/plain; charset=utf-8 (RFC 7252)
        /// </summary>
        TextPlain = 0,

        /// <summary>
        /// application/cose; cose-type="cose-encrypt0" (RFC 8152)
        /// </summary>
        ApplicationCoseEncrypt0 = 16,

        /// <summary>
        /// application/cose; cose-type="cose-mac0" (RFC 8152)
        /// </summary>
        ApplicationCoseMac0 = 17,

        /// <summary>
        /// application/cose; cose-type="cose-sign1" (RFC 8152)
        /// </summary>
        ApplicationCoseSign1 = 18,

        /// <summary>
        /// application/link-format (RFC 6690)
        /// </summary>
        ApplicationLinkFormat = 40,

        /// <summary>
        /// application/xml (RFC 3023)
        /// </summary>
        ApplicationXml = 41,

        /// <summary>
        /// application/octet-stream (RFC 2045)
        /// </summary>
        ApplicationOctetStream = 42,

        /// <summary>
        /// application/exi (Efficient XML Interchange)
        /// </summary>
        ApplicationExi = 47,

        /// <summary>
        /// application/json (RFC 7159)
        /// </summary>
        ApplicationJson = 50,

        /// <summary>
        /// application/cbor (RFC 8949, IANA CoAP Content-Format 60)
        /// </summary>
        ApplicationCbor = 60,

        /// <summary>
        /// application/cwt (RFC 8392)
        /// </summary>
        ApplicationCwt = 61,

        /// <summary>
        /// application/senml+json (RFC 8428)
        /// </summary>
        ApplicationSenmlJson = 110,

        /// <summary>
        /// application/sensml+json (RFC 8428)
        /// </summary>
        ApplicationSensmlJson = 111,

        /// <summary>
        /// application/senml+cbor (RFC 8428)
        /// </summary>
        ApplicationSenmlCbor = 112,

        /// <summary>
        /// application/sensml+cbor (RFC 8428)
        /// </summary>
        ApplicationSensmlCbor = 113,

        /// <summary>
        /// application/senml-exi (RFC 8428)
        /// </summary>
        ApplicationSenmlExi = 114,

        /// <summary>
        /// application/sensml-exi (RFC 8428)
        /// </summary>
        ApplicationSensmlExi = 115,

        /// <summary>
        /// application/coap-group+json (RFC 7390)
        /// </summary>
        ApplicationCoapGroupJson = 256,

        /// <summary>
        /// application/senml+xml (RFC 8428)
        /// </summary>
        ApplicationSenmlXml = 310,

        /// <summary>
        /// application/sensml+xml (RFC 8428)
        /// </summary>
        ApplicationSensmlXml = 311
    }
}
