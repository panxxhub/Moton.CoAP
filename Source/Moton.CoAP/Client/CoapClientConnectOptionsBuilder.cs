using Moton.CoAP.Protocol;
using Moton.CoAP.Transport;
using System;

namespace Moton.CoAP.Client
{
    public class CoapClientConnectOptionsBuilder
    {
        readonly CoapClientConnectOptions _options = new CoapClientConnectOptions
        {
            TransportLayerFactory = () => new UdpCoapTransportLayer() // This is the protocols default transport.
        };

        public CoapClientConnectOptionsBuilder WithHost(string value)
        {
            _options.Host = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        public CoapClientConnectOptionsBuilder WithPort(int value)
        {
            _options.Port = value;
            return this;
        }

        public CoapClientConnectOptionsBuilder WithEncryptedPort()
        {
            return WithPort(CoapDefaultPort.Encrypted);
        }

        public CoapClientConnectOptionsBuilder WithUnencryptedPort()
        {
            return WithPort(CoapDefaultPort.Unencrypted);
        }

        public CoapClientConnectOptionsBuilder WithTcpTransportLayer()
        {
            _options.TransportLayerFactory = () => new TcpCoapTransportLayer();
            return this;
        }

        public CoapClientConnectOptionsBuilder WithTransportLayer(Func<ICoapTransportLayer> transportLayerFactory)
        {
            _options.TransportLayerFactory = transportLayerFactory ?? throw new ArgumentNullException(nameof(transportLayerFactory));
            return this;
        }

        public CoapClientConnectOptionsBuilder WithUdpTransportLayer()
        {
            _options.TransportLayerFactory = () => new UdpCoapTransportLayer();
            return this;
        }

        /// <summary>
        /// Sets the preferred block size for Block1/Block2 transfers.
        /// </summary>
        /// <param name="value">Block size in bytes (16, 32, 64, 128, 256, 512, or 1024).</param>
        public CoapClientConnectOptionsBuilder WithPreferredBlockSize(int value)
        {
            _options.PreferredBlockSize = value;
            return this;
        }

        /// <summary>
        /// Enables automatic block-wise transfer for large payloads.
        /// </summary>
        public CoapClientConnectOptionsBuilder WithBlockTransferEnabled()
        {
            _options.EnableBlockTransfer = true;
            return this;
        }

        /// <summary>
        /// Disables automatic block-wise transfer for large payloads.
        /// </summary>
        public CoapClientConnectOptionsBuilder WithBlockTransferDisabled()
        {
            _options.EnableBlockTransfer = false;
            return this;
        }

        /// <summary>
        /// Sets the communication timeout for requests.
        /// </summary>
        /// <param name="timeout">The timeout duration.</param>
        public CoapClientConnectOptionsBuilder WithCommunicationTimeout(TimeSpan timeout)
        {
            _options.CommunicationTimeout = timeout;
            return this;
        }

        public CoapClientConnectOptions Build()
        {
            if (_options.TransportLayerFactory == null)
            {
                throw new CoapClientConfigurationInvalidException("Transport layer is not set.", null!);
            }

            return _options;
        }
    }
}