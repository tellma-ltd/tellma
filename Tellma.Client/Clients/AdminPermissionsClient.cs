using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Client
{
    public class AdminPermissionsClient : ClientBase
    {
        internal AdminPermissionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "admin-permissions";

        public async Task<Versioned<AdminPermissionsForClient>> PermissionsForClient(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<AdminPermissionsForClient>>(cancellation)
                .ConfigureAwait(false);
        }
    }
}
