using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Admin;

namespace Tellma.Client
{
    public class AdminUsersClient : CrudClientBase<AdminUserForSave, AdminUser, int>
    {
        internal AdminUsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "admin-users";

        public async Task<EntitiesResult<AdminUser>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<AdminUser>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<AdminUser>> SendInvitation(List<int> ids, Request<ActionArguments> request = null, CancellationToken cancellation = default)
            => await PutAction("invite", ids, request, AddActionArgumentsToUrl, cancellation);

        public async Task<Versioned<AdminUserSettingsForClient>> UserSettingsForClient(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<AdminUserSettingsForClient>>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<Versioned<AdminUserSettingsForClient>> SaveUserSetting(SaveUserSettingsArguments args, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client");
            args ??= new SaveUserSettingsArguments();
            urlBldr.AddQueryParameter(nameof(args.Key), args.Key);
            urlBldr.AddQueryParameter(nameof(args.Value), args.Value);

            using var msg = new HttpRequestMessage(HttpMethod.Post, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<AdminUserSettingsForClient>>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<EntityResult<AdminUser>> GetMyUser(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("me");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            var response = await httpResponse.Content
                .ReadAsAsync<GetByIdResponse<AdminUser>>(cancellation)
                .ConfigureAwait(false);

            var entity = response.Result;
            var singleton = new List<AdminUser> { entity };
            ClientUtil.Unflatten(singleton, response.RelatedEntities, cancellation);

            return new EntityResult<AdminUser>(entity);
        }

        public async Task<EntityResult<AdminUser>> SaveMyUser(MyAdminUserForSave me, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("me");
            using var msg = new HttpRequestMessage(HttpMethod.Post, urlBldr.Uri)
            {
                Content = ToJsonContent(me)
            };

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            var response = await httpResponse.Content
                .ReadAsAsync<GetByIdResponse<AdminUser>>(cancellation)
                .ConfigureAwait(false);

            var entity = response.Result;
            var singleton = new List<AdminUser> { entity };
            ClientUtil.Unflatten(singleton, response.RelatedEntities, cancellation);

            return new EntityResult<AdminUser>(entity);
        }
    }
}
