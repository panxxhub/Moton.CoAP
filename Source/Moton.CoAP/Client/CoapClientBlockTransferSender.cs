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
    /// <summary>
    /// Handles sending large payloads using Block1 option (RFC 7959).
    /// Block1 is used for request payload segmentation (client → server).
    /// </summary>
    public sealed class CoapClientBlockTransferSender
    {
        /// <summary>
        /// Default block size in bytes (1KB, SZX=6).
        /// </summary>
        public const int DefaultBlockSize = 1024;

        /// <summary>
        /// Minimum block size in bytes (16 bytes, SZX=0).
        /// </summary>
        public const int MinBlockSize = 16;

        /// <summary>
        /// Maximum block size in bytes (1KB, SZX=6).
        /// </summary>
        public const int MaxBlockSize = 1024;

        readonly CoapClient _client;
        readonly CoapNetLogger _logger;
        readonly int _blockSize;

        /// <summary>
        /// Creates a new Block1 sender.
        /// </summary>
        /// <param name="client">The CoAP client to use for sending.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="blockSize">The block size to use (16, 32, 64, 128, 256, 512, or 1024 bytes).</param>
        public CoapClientBlockTransferSender(
            CoapClient client,
            CoapNetLogger logger,
            int blockSize = DefaultBlockSize)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockSize = NormalizeBlockSize(blockSize);
        }

        /// <summary>
        /// Determines if the payload requires block-wise transfer.
        /// </summary>
        /// <param name="payload">The payload to check.</param>
        /// <returns>True if the payload exceeds the block size.</returns>
        public bool RequiresBlockTransfer(ArraySegment<byte> payload)
        {
            return payload.Count > _blockSize;
        }

        /// <summary>
        /// Determines if the payload requires block-wise transfer.
        /// </summary>
        /// <param name="payload">The payload to check.</param>
        /// <returns>True if the payload exceeds the block size.</returns>
        public bool RequiresBlockTransfer(byte[] payload)
        {
            return payload != null && payload.Length > _blockSize;
        }

        /// <summary>
        /// Sends a large payload using Block1 option.
        /// </summary>
        /// <param name="requestTemplate">The request message template (will be cloned for each block).</param>
        /// <param name="payload">The full payload to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The final response after all blocks are acknowledged.</returns>
        public async Task<CoapMessage> SendAsync(
            CoapMessage requestTemplate,
            ArraySegment<byte> payload,
            CancellationToken cancellationToken)
        {
            if (requestTemplate is null)
            {
                throw new ArgumentNullException(nameof(requestTemplate));
            }

            var totalBlocks = (payload.Count + _blockSize - 1) / _blockSize;
            _logger.Trace(nameof(CoapClientBlockTransferSender),
                "Starting Block1 transfer: {0} bytes in {1} blocks of {2} bytes.",
                payload.Count, totalBlocks, _blockSize);

            CoapMessage lastResponse = null;
            var currentBlockNumber = 0;
            var effectiveBlockSize = _blockSize;

            while (true)
            {
                var offset = currentBlockNumber * effectiveBlockSize;
                if (offset >= payload.Count)
                {
                    break;
                }

                var remainingBytes = payload.Count - offset;
                var hasMoreBlocks = remainingBytes > effectiveBlockSize;
                var blockPayloadSize = hasMoreBlocks ? effectiveBlockSize : remainingBytes;

                // Create block payload
                var blockPayload = new byte[blockPayloadSize];
                Array.Copy(payload.Array, payload.Offset + offset, blockPayload, 0, blockPayloadSize);

                // Create request message for this block
                var blockRequest = CloneRequestMessage(requestTemplate);
                blockRequest.Payload = new ArraySegment<byte>(blockPayload);

                // Add Block1 option
                var block1Value = new CoapBlockTransferOptionValue
                {
                    Number = (ushort)currentBlockNumber,
                    Size = (ushort)effectiveBlockSize,
                    HasFollowingBlocks = hasMoreBlocks
                };
                var block1OptionValue = CoapBlockTransferOptionValueEncoder.Encode(block1Value);

                // Remove any existing Block1 option and add new one
                blockRequest.Options.RemoveAll(o => o.Number == CoapMessageOptionNumber.Block1);
                blockRequest.Options.Add(new CoapMessageOption(
                    CoapMessageOptionNumber.Block1,
                    new CoapMessageOptionUintValue(block1OptionValue)));

                _logger.Trace(nameof(CoapClientBlockTransferSender),
                    "Sending Block1 {0}/{1}/{2} ({3} bytes).",
                    currentBlockNumber, hasMoreBlocks ? "M" : "_", effectiveBlockSize, blockPayloadSize);

                // Send and wait for response
                lastResponse = await _client.RequestAsync(blockRequest, cancellationToken).ConfigureAwait(false);

                // Check response
                if (!IsSuccessResponse(lastResponse))
                {
                    _logger.Warning(nameof(CoapClientBlockTransferSender),
                        "Block1 transfer failed at block {0}. Response code: {1}.{2}",
                        currentBlockNumber, lastResponse.Code.Class, lastResponse.Code.Detail);
                    return lastResponse;
                }

                // Check if server sent a Block1 option in response (may indicate different block size)
                var responseBlock1Option = lastResponse.Options
                    .FirstOrDefault(o => o.Number == CoapMessageOptionNumber.Block1);

                if (responseBlock1Option != null)
                {
                    var responseBlock1Value = CoapBlockTransferOptionValueDecoder.Decode(
                        ((CoapMessageOptionUintValue)responseBlock1Option.Value).Value);

                    // Server may request a smaller block size
                    if (responseBlock1Value.Size < effectiveBlockSize)
                    {
                        _logger.Trace(nameof(CoapClientBlockTransferSender),
                            "Server requested smaller block size: {0} -> {1}.",
                            effectiveBlockSize, responseBlock1Value.Size);
                        effectiveBlockSize = responseBlock1Value.Size;
                    }
                }

                if (!hasMoreBlocks)
                {
                    _logger.Trace(nameof(CoapClientBlockTransferSender),
                        "Block1 transfer completed. Total {0} bytes sent in {1} blocks.",
                        payload.Count, currentBlockNumber + 1);
                    break;
                }

                currentBlockNumber++;
            }

            return lastResponse;
        }

        /// <summary>
        /// Normalizes the block size to a valid RFC 7959 value.
        /// Valid sizes: 16, 32, 64, 128, 256, 512, 1024 (SZX 0-6).
        /// </summary>
        static int NormalizeBlockSize(int blockSize)
        {
            if (blockSize <= 16) return 16;
            if (blockSize <= 32) return 32;
            if (blockSize <= 64) return 64;
            if (blockSize <= 128) return 128;
            if (blockSize <= 256) return 256;
            if (blockSize <= 512) return 512;
            return 1024;
        }

        /// <summary>
        /// Creates a shallow clone of the request message with a new options list.
        /// </summary>
        static CoapMessage CloneRequestMessage(CoapMessage original)
        {
            return new CoapMessage
            {
                Type = original.Type,
                Code = original.Code,
                Token = original.Token,
                Options = new List<CoapMessageOption>(original.Options)
            };
        }

        /// <summary>
        /// Checks if the response indicates success (2.xx class).
        /// </summary>
        static bool IsSuccessResponse(CoapMessage response)
        {
            return response.Code.Class == 2;
        }
    }
}
