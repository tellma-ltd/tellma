using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class AgentsClient : CrudClientBase<AgentForSave, Agent, int>
    {
        private readonly int _definitionId;

        internal AgentsClient(int definitionId, IClientBehavior behavior) : base(behavior)
        {
            _definitionId = definitionId;
        }

        protected override string ControllerPath => $"agents/{_definitionId}";

        public async Task<EntitiesResult<Agent>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Agent>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<Stream> GetImage(int id, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder(id.ToString(), "image");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public async Task<Stream> GetAttachment(int id, int attachmentId, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder(id.ToString(), "attachments", attachmentId.ToString());
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }
}
