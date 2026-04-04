using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class UsersClient : CrudClientBase<UserForSave, User, int>
    {
        internal UsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "users";

        public async Task<EntitiesResult<User>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<User>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<Versioned<UserSettingsForClient>> UserSettingsForClient(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<UserSettingsForClient>>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<Versioned<UserSettingsForClient>> SaveUserSetting(SaveUserSettingsArguments args, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client");
            args ??= new SaveUserSettingsArguments();
            urlBldr.AddQueryParameter(nameof(args.Key), args.Key);
            urlBldr.AddQueryParameter(nameof(args.Value), args.Value);

            using var msg = new HttpRequestMessage(HttpMethod.Post, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<UserSettingsForClient>>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<Versioned<UserSettingsForClient>> SaveUserPreferredLanguage(string preferredLanguage, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client", "preferred-language");
            urlBldr.AddQueryParameter("preferredLanguage", preferredLanguage);

            using var msg = new HttpRequestMessage(HttpMethod.Post, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<UserSettingsForClient>>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<Versioned<UserSettingsForClient>> SaveUserPreferredCalendar(string preferredCalendar, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("client", "preferred-calendar");
            urlBldr.AddQueryParameter("preferredCalendar", preferredCalendar);

            using var msg = new HttpRequestMessage(HttpMethod.Post, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<Versioned<UserSettingsForClient>>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<EntitiesResult<User>> SendInvitation(List<int> ids, Request<ActionArguments> request = null, CancellationToken cancellation = default)
            => await PutAction("invite", ids, request, AddActionArgumentsToUrl, cancellation);

        public async Task<Stream> GetImage(int id, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder(id.ToString(), "image");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public async Task<EntityResult<User>> GetMyUser(Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("me");
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            var response = await httpResponse.Content
                .ReadAsAsync<GetByIdResponse<User>>(cancellation)
                .ConfigureAwait(false);

            var entity = response.Result;
            var singleton = new List<User> { entity };
            Unflatten(singleton, response.RelatedEntities, cancellation);

            return new EntityResult<User>(entity);
        }

        public async Task<EntityResult<User>> SaveMyUser(MyUserForSave me, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("me");
            using var msg = new HttpRequestMessage(HttpMethod.Post, urlBldr.Uri)
            {
                Content = ToJsonContent(me)
            };

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            var response = await httpResponse.Content
                .ReadAsAsync<GetByIdResponse<User>>(cancellation)
                .ConfigureAwait(false);

            var entity = response.Result;
            var singleton = new List<User> { entity };
            Unflatten(singleton, response.RelatedEntities, cancellation);

            return new EntityResult<User>(entity);
        }

        public async Task<string> TestEmail(string email, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("test-email");
            urlBldr.AddQueryParameter("email", email);

            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> TestPhone(string phone, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("test-phone");
            urlBldr.AddQueryParameter("phone", phone);

            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
