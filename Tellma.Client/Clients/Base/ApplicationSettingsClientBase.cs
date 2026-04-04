using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;

namespace Tellma.Client
{
    public abstract class ApplicationSettingsClientBase<TSettingsForSave, TSettings> : ClientBase
        where TSettings : Entity
        where TSettingsForSave : Entity
    {
        #region Lifecycle

        internal ApplicationSettingsClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<TSettings> GetSettings(Request<SelectExpandArguments> request = null, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder();

            // Add query parameters
            var args = request?.Arguments ?? new SelectExpandArguments();

            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);

            // Prepare the message
            var method = HttpMethod.Get;
            using var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<GetEntityResponse<TSettings>>(cancellation)
                .ConfigureAwait(false);

            var settings = response.Result;
            var relatedEntities = response.RelatedEntities;

            var singleton = new List<TSettings> { settings };
            ClientUtil.Unflatten(singleton, relatedEntities, cancellation);

            return settings;
        }

        public virtual async Task<SaveSettingsResult<TSettings>> SaveSettings(TSettingsForSave settingsForSave, Request<SaveArguments> request = null, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder();

            // Add query parameters
            var args = request?.Arguments ?? new SaveArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            urlBldr.AddQueryParameter(nameof(args.ReturnEntities), args.ReturnEntities?.ToString());

            // Prepare the message
            var method = HttpMethod.Post;
            using var msg = new HttpRequestMessage(method, urlBldr.Uri)
            {
                Content = JsonContent.Create(settingsForSave, options: new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                })
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<SaveSettingsResponse<TSettings>>(cancellation)
                .ConfigureAwait(false);

            var settings = response.Result;
            var relatedEntities = response.RelatedEntities;

            var singleton = new List<TSettings> { settings };
            ClientUtil.Unflatten(singleton, relatedEntities, cancellation);

            return new SaveSettingsResult<TSettings>(settings, response.SettingsForClient);
        }

        #endregion
    }
}
