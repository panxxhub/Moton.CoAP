using System.Threading.Tasks;

namespace Moton.CoAP.Client
{
    public interface ICoapResponseHandler
    {
        Task HandleResponseAsync(HandleResponseContext context);
    }
}
