using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Admin;

namespace Tellma.Client
{
    public class IdentityServerClientsClient : CrudClientBase<IdentityServerClientForSave, IdentityServerClient, int>
    {
        internal IdentityServerClientsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "identity-server-clients";

        public async Task<EntitiesResult<IdentityServerClient>> ResetSecret(Request<ResetClientSecretArguments> request, CancellationToken cancellation = default)
        {
            var args = request?.Arguments ?? throw new ArgumentNullException(nameof(request));

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("reset-secret");
            urlBldr.AddQueryParameter(nameof(args.Id), args.Id.ToString());
            AddActionArgumentsToUrl(urlBldr, args);

            // Prepare the message
            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<IdentityServerClient>>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result?.ToList();
            var relatedEntities = response.RelatedEntities;
            Unflatten(entities, relatedEntities, cancellation);

            return new EntitiesResult<IdentityServerClient>(entities, entities?.Count);
        }
    }
}
