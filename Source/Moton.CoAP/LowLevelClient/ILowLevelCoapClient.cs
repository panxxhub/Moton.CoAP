using Moton.CoAP.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Moton.CoAP.LowLevelClient
{
    public interface ILowLevelCoapClient : IDisposable
    {
        Task SendAsync(CoapMessage message, CancellationToken cancellationToken);

        Task<CoapMessage?> ReceiveAsync(CancellationToken cancellationToken);
    }
}
