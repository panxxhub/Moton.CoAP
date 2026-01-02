namespace Moton.CoAP.Protocol.Options
{
    /// <summary>
    /// Helper methods for working with CoAP content formats.
    /// </summary>
    public static class CoapContentFormatHelper
    {
        /// <summary>
        /// Checks if the content format is CBOR-based.
        /// </summary>
        public static bool IsCborFormat(CoapMessageContentFormat format)
        {
            return format == CoapMessageContentFormat.ApplicationCbor ||
                   format == CoapMessageContentFormat.ApplicationSenmlCbor ||
                   format == CoapMessageContentFormat.ApplicationSensmlCbor ||
                   format == CoapMessageContentFormat.ApplicationCwt;
        }

        /// <summary>
        /// Checks if the content format is JSON-based.
        /// </summary>
        public static bool IsJsonFormat(CoapMessageContentFormat format)
        {
            return format == CoapMessageContentFormat.ApplicationJson ||
                   format == CoapMessageContentFormat.ApplicationSenmlJson ||
                   format == CoapMessageContentFormat.ApplicationSensmlJson ||
                   format == CoapMessageContentFormat.ApplicationCoapGroupJson;
        }

        /// <summary>
        /// Checks if the content format is XML-based.
        /// </summary>
        public static bool IsXmlFormat(CoapMessageContentFormat format)
        {
            return format == CoapMessageContentFormat.ApplicationXml ||
                   format == CoapMessageContentFormat.ApplicationSenmlXml ||
                   format == CoapMessageContentFormat.ApplicationSensmlXml;
        }

        /// <summary>
        /// Checks if the content format is text-based (human readable).
        /// </summary>
        public static bool IsTextBasedFormat(CoapMessageContentFormat format)
        {
            return format == CoapMessageContentFormat.TextPlain ||
                   IsJsonFormat(format) ||
                   IsXmlFormat(format) ||
                   format == CoapMessageContentFormat.ApplicationLinkFormat;
        }

        /// <summary>
        /// Checks if the content format is binary (not human readable).
        /// </summary>
        public static bool IsBinaryFormat(CoapMessageContentFormat format)
        {
            return !IsTextBasedFormat(format);
        }

        /// <summary>
        /// Gets the IANA registered media type string for the content format.
        /// </summary>
        public static string GetMediaType(CoapMessageContentFormat format)
        {
            switch (format)
            {
                case CoapMessageContentFormat.TextPlain:
                    return "text/plain; charset=utf-8";
                case CoapMessageContentFormat.ApplicationCoseEncrypt0:
                    return "application/cose; cose-type=\"cose-encrypt0\"";
                case CoapMessageContentFormat.ApplicationCoseMac0:
                    return "application/cose; cose-type=\"cose-mac0\"";
                case CoapMessageContentFormat.ApplicationCoseSign1:
                    return "application/cose; cose-type=\"cose-sign1\"";
                case CoapMessageContentFormat.ApplicationLinkFormat:
                    return "application/link-format";
                case CoapMessageContentFormat.ApplicationXml:
                    return "application/xml";
                case CoapMessageContentFormat.ApplicationOctetStream:
                    return "application/octet-stream";
                case CoapMessageContentFormat.ApplicationExi:
                    return "application/exi";
                case CoapMessageContentFormat.ApplicationJson:
                    return "application/json";
                case CoapMessageContentFormat.ApplicationCbor:
                    return "application/cbor";
                case CoapMessageContentFormat.ApplicationCwt:
                    return "application/cwt";
                case CoapMessageContentFormat.ApplicationSenmlJson:
                    return "application/senml+json";
                case CoapMessageContentFormat.ApplicationSensmlJson:
                    return "application/sensml+json";
                case CoapMessageContentFormat.ApplicationSenmlCbor:
                    return "application/senml+cbor";
                case CoapMessageContentFormat.ApplicationSensmlCbor:
                    return "application/sensml+cbor";
                case CoapMessageContentFormat.ApplicationSenmlExi:
                    return "application/senml-exi";
                case CoapMessageContentFormat.ApplicationSensmlExi:
                    return "application/sensml-exi";
                case CoapMessageContentFormat.ApplicationCoapGroupJson:
                    return "application/coap-group+json";
                case CoapMessageContentFormat.ApplicationSenmlXml:
                    return "application/senml+xml";
                case CoapMessageContentFormat.ApplicationSensmlXml:
                    return "application/sensml+xml";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// Tries to parse a media type string to a content format.
        /// </summary>
        public static bool TryParseMediaType(string mediaType, out CoapMessageContentFormat format)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
            {
                format = CoapMessageContentFormat.ApplicationOctetStream;
                return false;
            }

            // Normalize: lowercase and remove parameters for basic types
            var normalized = mediaType.ToLowerInvariant().Trim();
            
            // Handle charset parameter for text/plain
            if (normalized.StartsWith("text/plain"))
            {
                format = CoapMessageContentFormat.TextPlain;
                return true;
            }

            switch (normalized)
            {
                case "application/link-format":
                    format = CoapMessageContentFormat.ApplicationLinkFormat;
                    return true;
                case "application/xml":
                    format = CoapMessageContentFormat.ApplicationXml;
                    return true;
                case "application/octet-stream":
                    format = CoapMessageContentFormat.ApplicationOctetStream;
                    return true;
                case "application/exi":
                    format = CoapMessageContentFormat.ApplicationExi;
                    return true;
                case "application/json":
                    format = CoapMessageContentFormat.ApplicationJson;
                    return true;
                case "application/cbor":
                    format = CoapMessageContentFormat.ApplicationCbor;
                    return true;
                case "application/cwt":
                    format = CoapMessageContentFormat.ApplicationCwt;
                    return true;
                case "application/senml+json":
                    format = CoapMessageContentFormat.ApplicationSenmlJson;
                    return true;
                case "application/sensml+json":
                    format = CoapMessageContentFormat.ApplicationSensmlJson;
                    return true;
                case "application/senml+cbor":
                    format = CoapMessageContentFormat.ApplicationSenmlCbor;
                    return true;
                case "application/sensml+cbor":
                    format = CoapMessageContentFormat.ApplicationSensmlCbor;
                    return true;
                case "application/senml-exi":
                    format = CoapMessageContentFormat.ApplicationSenmlExi;
                    return true;
                case "application/sensml-exi":
                    format = CoapMessageContentFormat.ApplicationSensmlExi;
                    return true;
                case "application/coap-group+json":
                    format = CoapMessageContentFormat.ApplicationCoapGroupJson;
                    return true;
                case "application/senml+xml":
                    format = CoapMessageContentFormat.ApplicationSenmlXml;
                    return true;
                case "application/sensml+xml":
                    format = CoapMessageContentFormat.ApplicationSensmlXml;
                    return true;
                default:
                    format = CoapMessageContentFormat.ApplicationOctetStream;
                    return false;
            }
        }
    }
}
