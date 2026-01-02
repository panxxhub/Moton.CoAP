using Moton.CoAP.Protocol;
using Moton.CoAP.Transport;
using System;

namespace Moton.CoAP.Client
{
    public class CoapClientConnectOptions
    {
        public string Host
        {
            get; set;
        }

        public int Port { get; set; } = CoapDefaultPort.Unencrypted;

        public TimeSpan CommunicationTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public Func<ICoapTransportLayer> TransportLayerFactory { get; set; } = () => new UdpCoapTransportLayer();

        /// <summary>
        /// Maximum block size for Block1/Block2 transfers.
        /// Valid values: 16, 32, 64, 128, 256, 512, 1024 bytes.
        /// Default: 1024 bytes (SZX=6).
        /// </summary>
        public int PreferredBlockSize { get; set; } = 1024;

        /// <summary>
        /// Enable automatic block-wise transfer for large payloads (Block1) and responses (Block2).
        /// When enabled, the client will automatically segment large payloads.
        /// Default: true.
        /// </summary>
        public bool EnableBlockTransfer { get; set; } = true;
    }
}
