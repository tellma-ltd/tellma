using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Client
{
    public interface IHttpRequestSender
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, Request req, CancellationToken cancellation = default);
    }
}
