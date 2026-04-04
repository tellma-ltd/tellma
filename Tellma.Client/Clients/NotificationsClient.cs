using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Client
{
    public class NotificationsClient : ClientBase
    {
        internal NotificationsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "notifications";

        public async Task<NotificationSummary> Recap(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("recap");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<NotificationSummary>(cancellation)
                .ConfigureAwait(false);
        }
    }
}
