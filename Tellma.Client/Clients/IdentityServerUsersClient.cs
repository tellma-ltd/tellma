using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Admin;

namespace Tellma.Client
{
    public class IdentityServerUsersClient : FactGetByIdClientBase<IdentityServerUser, string>
    {
        internal IdentityServerUsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "identity-server-users";

        public async Task<EntitiesResult<IdentityServerUser>> ResetPassword(ResetPasswordArguments args, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("reset-password");

            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri)
            {
                Content = JsonContent.Create(args, options: new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                })
            };

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<IdentityServerUser>>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result?.ToList();
            var relatedEntities = response.RelatedEntities;
            ClientUtil.Unflatten(entities, relatedEntities, cancellation);

            return new EntitiesResult<IdentityServerUser>(entities, entities?.Count);
        }
    }
}
