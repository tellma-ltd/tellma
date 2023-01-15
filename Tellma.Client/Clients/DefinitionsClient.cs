using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Client
{
    public class DefinitionsClient : ClientBase
    {
        protected override string ControllerPath => "definitions";

        public DefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        public async Task<Versioned<DefinitionsForClient>> DefinitionsForClient(Request req = default, CancellationToken cancellation = default)
        {
            // Prepare the request
            var urlBldr = GetActionUrlBuilder("client");
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the request
            using var httpResponse = await SendAsync(msg, req, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            return await httpResponse.Content
                .ReadAsAsync<Versioned<DefinitionsForClient>>(cancellation)
                .ConfigureAwait(false);
        }
    }
}
