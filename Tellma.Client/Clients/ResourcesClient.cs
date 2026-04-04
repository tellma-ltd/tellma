using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class ResourcesClient : CrudClientBase<ResourceForSave, Resource, int>
    {
        private readonly int _definitionId;

        internal ResourcesClient(int definitionId, IClientBehavior behavior) : base(behavior)
        {
            _definitionId = definitionId;
        }

        protected override string ControllerPath => $"resources/{_definitionId}";

        public async Task<EntitiesResult<Resource>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Resource>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
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
