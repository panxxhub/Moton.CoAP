using Moton.CoAP.Internal;
using Moton.CoAP.Logging;
using Moton.CoAP.Protocol;
using Moton.CoAP.Protocol.BlockTransfer;
using Moton.CoAP.Protocol.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Moton.CoAP.Client
{
    public sealed class CoapClientBlockTransferReceiver
    {
        readonly CoapMessage _requestMessage;
        readonly CoapMessage _firstResponseMessage;
        readonly CoapClient _client;
        readonly CoapNetLogger _logger;

        /// <summary>
        /// Event raised after each block is received.
        /// </summary>
        public event EventHandler<CoapBlockTransferProgress>? BlockReceived;

        public CoapClientBlockTransferReceiver(CoapMessage requestMessage, CoapMessage firstResponseMessage, CoapClient client, CoapNetLogger logger)
        {
            _requestMessage = requestMessage ?? throw new ArgumentNullException(nameof(requestMessage));
            _firstResponseMessage = firstResponseMessage ?? throw new ArgumentNullException(nameof(firstResponseMessage));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static bool IsBlockTransfer(CoapMessage responseMessage)
        {
            if (responseMessage is null)
            {
                throw new ArgumentNullException(nameof(responseMessage));
            }

            return responseMessage.Options.Any(o => o.Number == CoapMessageOptionNumber.Block2);
        }

        public async Task<ArraySegment<byte>> ReceiveFullPayload(CancellationToken cancellationToken)
        {
            var receivedBlock2Option = _firstResponseMessage.Options.First(o => o.Number == CoapMessageOptionNumber.Block2);
            var receivedBlock2OptionValue = CoapBlockTransferOptionValueDecoder.Decode(((CoapMessageOptionUintValue)receivedBlock2Option.Value).Value);
            _logger.Trace(nameof(CoapClientBlockTransferReceiver), "Received Block2 {0}.", FormatBlock2OptionValue(receivedBlock2OptionValue));

            var requestMessage = new CoapMessage
            {
                Type = CoapMessageType.Confirmable,
                Code = _requestMessage.Code,
                Token = _requestMessage.Token,
                Options = new List<CoapMessageOption>(_requestMessage.Options)
            };

            var requestBlock2Option = new CoapMessageOption(CoapMessageOptionNumber.Block2, new CoapMessageOptionUintValue(0));
            requestMessage.Options.Add(requestBlock2Option);

            // Create a buffer which is pre sized to at least 4 blocks.
            using (var buffer = new MemoryBuffer(receivedBlock2OptionValue.Size * 4))
            {
                buffer.Write(_firstResponseMessage.Payload);
                
                // Raise progress for first block
                RaiseBlockReceived(0, receivedBlock2OptionValue.Size, (int)buffer.Position);

                while (receivedBlock2OptionValue.HasFollowingBlocks)
                {
                    // Patch Block2 so that we get the next chunk.
                    receivedBlock2OptionValue.Number++;

                    requestBlock2Option.Value = new CoapMessageOptionUintValue(CoapBlockTransferOptionValueEncoder.Encode(receivedBlock2OptionValue));

                    var response = await _client.RequestAsync(requestMessage, cancellationToken).ConfigureAwait(false);
                    receivedBlock2Option = response.Options.First(o => o.Number == CoapMessageOptionNumber.Block2);
                    receivedBlock2OptionValue = CoapBlockTransferOptionValueDecoder.Decode(((CoapMessageOptionUintValue)receivedBlock2Option.Value).Value);

                    _logger.Trace(nameof(CoapClientBlockTransferReceiver), "Received Block2 {0}.", FormatBlock2OptionValue(receivedBlock2OptionValue));

                    buffer.Write(response.Payload);
                    
                    // Raise progress for each block
                    RaiseBlockReceived(receivedBlock2OptionValue.Number, receivedBlock2OptionValue.Size, (int)buffer.Position);
                }

                return buffer.GetBuffer();
            }
        }

        static string FormatBlock2OptionValue(CoapBlockTransferOptionValue value)
        {
            return $"{value.Number}/{(value.HasFollowingBlocks ? 'M' : '_')}/{value.Size}";
        }

        void RaiseBlockReceived(int blockNumber, int blockSize, int bytesReceived)
        {
            // For Block2, we don't know total size until the last block (when HasFollowingBlocks is false).
            // We estimate based on current progress.
            BlockReceived?.Invoke(this, new CoapBlockTransferProgress
            {
                Direction = CoapBlockTransferDirection.Download,
                BlockNumber = blockNumber,
                TotalBlocks = blockNumber + 1, // Estimate - updated when transfer completes
                BlockSize = blockSize,
                BytesTransferred = bytesReceived,
                TotalBytes = bytesReceived // Estimate - we don't know total until done
            });
        }
    }
}
