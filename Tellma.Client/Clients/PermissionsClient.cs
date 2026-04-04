using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Client
{
    public class PermissionsClient : ClientBase
    {
        internal PermissionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "permissions";

        public async Task<Versioned<PermissionsForClient>> PermissionsForClient(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<PermissionsForClient>>(cancellation)
                .ConfigureAwait(false);
        }
    }
}
