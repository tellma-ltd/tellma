using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Client
{
    public class AdminSettingsClient : ClientBase
    {
        internal AdminSettingsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "admin-settings";

        public async Task<Versioned<AdminSettingsForClient>> SettingsForClient(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<AdminSettingsForClient>>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task Ping(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("ping");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);
        }
    }
}
