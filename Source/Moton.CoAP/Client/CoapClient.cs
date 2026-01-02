using System;
using System.Threading;
using System.Threading.Tasks;
using Moton.CoAP.Internal;
using Moton.CoAP.Logging;
using Moton.CoAP.LowLevelClient;
using Moton.CoAP.MessageDispatcher;
using Moton.CoAP.Protocol;
using Moton.CoAP.Protocol.Observe;
using Moton.CoAP.Protocol.Options;

namespace Moton.CoAP.Client
{
    public sealed class CoapClient : ICoapClient
    {
        readonly CoapNetLogger _logger;
        readonly LowLevelCoapClient _lowLevelClient;
        readonly CoapMessageDispatcher _messageDispatcher = new CoapMessageDispatcher();
        readonly CoapMessageIdProvider _messageIdProvider = new CoapMessageIdProvider();
        readonly CoapMessageTokenProvider _messageTokenProvider = new CoapMessageTokenProvider();
        readonly CoapMessageToResponseConverter _messageToResponseConverter = new CoapMessageToResponseConverter();
        readonly CoapClientObservationManager _observationManager;
        readonly CoapRequestToMessageConverter _requestToMessageConverter = new CoapRequestToMessageConverter();
        
        CancellationTokenSource? _cancellationToken;

        CoapClientConnectOptions? _connectOptions;

        /// <summary>
        /// Event raised when a block transfer progresses (Block1 upload or Block2 download).
        /// </summary>
        public event CoapBlockTransferProgressHandler? BlockTransferProgress;

        public CoapClient(CoapNetLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _lowLevelClient = new LowLevelCoapClient(_logger);
            _observationManager =
                new CoapClientObservationManager(_messageToResponseConverter, _lowLevelClient, _logger);
        }

        public async Task ConnectAsync(CoapClientConnectOptions options, CancellationToken cancellationToken)
        {
            _connectOptions = options ?? throw new ArgumentNullException(nameof(options));

            await _lowLevelClient.ConnectAsync(options, cancellationToken).ConfigureAwait(false);

            _cancellationToken = new CancellationTokenSource();
            ParallelTask.StartLongRunning(() => ReceiveMessages(_cancellationToken.Token), _cancellationToken.Token);
        }

        public async Task<CoapResponse> RequestAsync(CoapRequest request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestMessage = _requestToMessageConverter.Convert(request);

            // Check if Block1 is needed for large payloads
            var blockSize = _connectOptions?.PreferredBlockSize ?? CoapClientBlockTransferSender.DefaultBlockSize;
            var enableBlockTransfer = _connectOptions?.EnableBlockTransfer ?? true;

            CoapMessage responseMessage;

            if (enableBlockTransfer && request.Payload.Count > blockSize)
            {
                // Use Block1 for large payloads
                var sender = new CoapClientBlockTransferSender(this, _logger, blockSize);
                sender.BlockSent += OnBlock1Progress;
                try
                {
                    responseMessage = await sender.SendAsync(requestMessage, request.Payload, cancellationToken)
                        .ConfigureAwait(false);
                }
                finally
                {
                    sender.BlockSent -= OnBlock1Progress;
                }
            }
            else
            {
                // Normal request (small payload or block transfer disabled)
                responseMessage = await RequestAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            }

            var payload = responseMessage.Payload;
            if (CoapClientBlockTransferReceiver.IsBlockTransfer(responseMessage))
            {
                var receiver = new CoapClientBlockTransferReceiver(requestMessage, responseMessage, this, _logger);
                receiver.BlockReceived += OnBlock2Progress;
                try
                {
                    payload = await receiver.ReceiveFullPayload(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    receiver.BlockReceived -= OnBlock2Progress;
                }
            }

            return _messageToResponseConverter.Convert(responseMessage, payload);
        }

        void OnBlock1Progress(object? sender, CoapBlockTransferProgress progress)
        {
            BlockTransferProgress?.Invoke(progress);
        }

        void OnBlock2Progress(object? sender, CoapBlockTransferProgress progress)
        {
            BlockTransferProgress?.Invoke(progress);
        }

        public async Task<CoapObserveResponse> ObserveAsync(CoapObserveOptions options,
            CancellationToken cancellationToken)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var request = new CoapRequest
            {
                Method = CoapRequestMethod.Get,
                Options = options.Request.Options
            };

            var token = _messageTokenProvider.Next();

            var requestMessage = _requestToMessageConverter.Convert(request);
            requestMessage.Token = token.Value;
            requestMessage.Options.Add(new CoapMessageOptionFactory().CreateObserve(CoapObserveOptionValue.Register));

            var responseMessage = await RequestAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            var payload = responseMessage.Payload;
            if (CoapClientBlockTransferReceiver.IsBlockTransfer(responseMessage))
            {
                payload = await new CoapClientBlockTransferReceiver(requestMessage, responseMessage, this, _logger)
                    .ReceiveFullPayload(cancellationToken).ConfigureAwait(false);
            }

            _observationManager.Register(token, options.ResponseHandler);

            var response = _messageToResponseConverter.Convert(responseMessage, payload);
            return new CoapObserveResponse(response, this)
            {
                Token = token,
                Request = request
            };
        }

        public async Task StopObservationAsync(CoapObserveResponse observeResponse, CancellationToken cancellationToken)
        {
            if (observeResponse is null)
            {
                throw new ArgumentNullException(nameof(observeResponse));
            }

            var requestMessage = _requestToMessageConverter.Convert(observeResponse.Request!);
            requestMessage.Token = observeResponse.Token!.Value;

            requestMessage.Options.RemoveAll(o => o.Number == CoapMessageOptionNumber.Observe);
            requestMessage.Options.Add(new CoapMessageOptionFactory().CreateObserve(CoapObserveOptionValue.Deregister));

            var responseMessage = await RequestAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            _observationManager.Deregister(observeResponse.Token);
        }

        public void Dispose()
        {
            try
            {
                _cancellationToken?.Cancel(false);
            }
            finally
            {
                _cancellationToken?.Dispose();
                _lowLevelClient?.Dispose();
            }
        }

        internal async Task<CoapMessage> RequestAsync(CoapMessage requestMessage, CancellationToken cancellationToken)
        {
            if (requestMessage is null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            requestMessage.Id = _messageIdProvider.Next();

            var responseAwaiter = _messageDispatcher.AddAwaiter(requestMessage.Id);
            
            try
            {
                await _lowLevelClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

                var responseMessage = await responseAwaiter.WaitOneAsync(_connectOptions!.CommunicationTimeout)
                    .ConfigureAwait(false);

                if (responseMessage.Code.Equals(CoapMessageCodes.Empty))
                {
                    // TODO: Support message which are sent later (no piggybacking).
                }

                return responseMessage;
            }
            finally
            {
                _messageDispatcher.RemoveAwaiter(requestMessage.Id);
            }
        }

        async Task ReceiveMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _lowLevelClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (message == null)
                    {
                        continue;
                    }

                    if (!_messageDispatcher.TryHandleReceivedMessage(message))
                    {
                        if (!await _observationManager.TryHandleReceivedMessage(message).ConfigureAwait(false))
                        {
                            _logger.Trace(nameof(CoapClient), "Received an unexpected message ({0}).", message.Id);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    if (!_cancellationToken!.IsCancellationRequested)
                    {
                        _logger.Error(nameof(CoapClient), exception, "Error while receiving messages.");
                    }
                    else
                    {
                        _logger.Information(nameof(CoapClient), "Stopped receiving messages due to cancellation.");
                    }
                    
                    _messageDispatcher.Dispatch(exception);
                }
            }
        }
    }
}