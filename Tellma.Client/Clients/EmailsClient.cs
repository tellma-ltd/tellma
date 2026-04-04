using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class EmailsClient : FactGetByIdClientBase<EmailForQuery, int>
    {
        internal EmailsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "emails";

        public async Task<Stream> GetAttachment(int emailId, int attachmentId, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder(emailId.ToString(), "attachments", attachmentId.ToString());
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }
}
