using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class GeneralSettingsClient : ApplicationSettingsClientBase<GeneralSettings, GeneralSettingsForSave>
    {
        protected override string ControllerPath => "general-settings";

        public GeneralSettingsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        public async Task Ping(Request req = default, CancellationToken cancellation = default)
        {
            // Prepare the request
            var urlBldr = GetActionUrlBuilder("ping");
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the request
            using var httpResponse = await SendAsync(msg, req, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);
        }

        public async Task<Versioned<SettingsForClient>> SettingsForClient(Request req = default, CancellationToken cancellation = default)
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
                .ReadAsAsync<Versioned<SettingsForClient>>(cancellation)
                .ConfigureAwait(false);
        }
    }
}
