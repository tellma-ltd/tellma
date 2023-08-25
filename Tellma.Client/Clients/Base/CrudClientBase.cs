using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class CrudClientBase<TEntityForSave, TEntity, TKey> : FactGetByIdClientBase<TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        internal CrudClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntitiesResult<TEntity>> Save(List<TEntityForSave> entitiesForSave, Request<SaveArguments> request = null, CancellationToken cancellation = default)
        {
            // Common scenario to load entities, modify them and then save them,
            // Many TEntity types actually inherit from TEntityForSave (e.g. Unit)
            // This ensures that if a TEntity is passed in the list it is transformed
            // to TEntityForSave before deserialization
            for (int i = 0; i < entitiesForSave.Count; i++)
            {
                if (entitiesForSave[i] is TEntity entity)
                {
                    entitiesForSave[i] = MapToEntityToSave(entity);
                }
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder();

            // Add query parameters
            var args = request?.Arguments ?? new SaveArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            urlBldr.AddQueryParameter(nameof(args.ReturnEntities), args.ReturnEntities?.ToString());

            // Prepare the message
            var method = HttpMethod.Post;
            var msg = new HttpRequestMessage(method, urlBldr.Uri)
            {
                Content = ToJsonContent(entitiesForSave)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            EntitiesResult<TEntity> result;
            if (args.ReturnEntities ?? false)
            {
                // Extract the response
                var response = await httpResponse.Content
                    .ReadAsAsync<EntitiesResponse<TEntity>>(cancellation)
                    .ConfigureAwait(false);

                var entities = response.Result?.ToList();
                var relatedEntities = response.RelatedEntities;

                Unflatten(entities, relatedEntities, cancellation);

                result = new EntitiesResult<TEntity>(entities, entities?.Count);
            }
            else
            {
                result = EntitiesResult<TEntity>.Empty();
            }

            return result;
        }

        public virtual async Task DeleteByIds(List<TKey> ids, Request request = null, CancellationToken cancellation = default)
        {
            if (ids == null || ids.Count == 0)
            {
                return;
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder();

            // Add query parameters
            foreach (var id in ids)
            {
                urlBldr.AddQueryParameter("I", id?.ToString());
            }

            // Prepare the message
            var method = HttpMethod.Delete;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);
        }

        public virtual async Task DeleteById(TKey id, Request request = null, CancellationToken cancellation = default)
        {
            if (id == null)
            {
                return;
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder(id.ToString());

            // Prepare the message
            var method = HttpMethod.Delete;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);
        }

        #endregion

        #region Helpers

        private string _expandForSave;
        public virtual string ExpandForSave
            => _expandForSave ??= ClientUtil.ExpandForSave<TEntityForSave>();

        protected HttpContent ToJsonContent(object payload)
        {
            return JsonContent.Create(payload, options: new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        private TEntityForSave MapToEntityToSave(TEntity entity)
        {
            return entity as TEntityForSave; // TODO
        }

        protected async Task<EntitiesResult<TEntity>> ActivateImpl(List<TKey> ids, Request<ActivateArguments> request, CancellationToken cancellation = default)
        {
            var req = request == null ? null : new Request<ActionArguments>()
            {
                Arguments = request.Arguments,
                Calendar = request.Calendar,
                IsSilent = request.IsSilent
            };

            return await PutAction("activate", ids, req, AddActionArgumentsToUrl, cancellation).ConfigureAwait(false);
        }

        protected async Task<EntitiesResult<TEntity>> DeactivateImpl(List<TKey> ids, Request<DeactivateArguments> request, CancellationToken cancellation = default)
        {
            var req = request == null ? null : new Request<ActionArguments>()
            {
                Arguments = request.Arguments,
                Calendar = request.Calendar,
                IsSilent = request.IsSilent
            };

            return await PutAction("deactivate", ids, req, AddActionArgumentsToUrl, cancellation).ConfigureAwait(false);
        }

        protected void AddActionArgumentsToUrl(UriBuilder uri, ActionArguments args)
        {
            args ??= new ActionArguments();
            uri.AddQueryParameter(nameof(args.Select), args.Select);
            uri.AddQueryParameter(nameof(args.Expand), args.Expand);
            uri.AddQueryParameter(nameof(args.ReturnEntities), args.ReturnEntities?.ToString());
        }

        protected async Task<EntitiesResult<TEntity>> PutAction<TArgs>(
            string action, 
            List<TKey> ids, 
            Request<TArgs> request, 
            Action<UriBuilder, TArgs> addArgs, 
            CancellationToken cancellation = default) where TArgs : ActionArguments
        {
            if (ids == null || !ids.Any())
            {
                return EntitiesResult<TEntity>.Empty();
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder(action);

            // Add query parameters
            var args = request?.Arguments;
            addArgs(urlBldr, args); // Any other custom configuration

            // Prepare the message
            var method = HttpMethod.Put;
            var msg = new HttpRequestMessage(method, urlBldr.Uri)
            {
                Content = ToJsonContent(ids)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            EntitiesResult<TEntity> result;
            if (args?.ReturnEntities ?? false)
            {
                // Extract the response
                var response = await httpResponse.Content
                    .ReadAsAsync<EntitiesResponse<TEntity>>(cancellation)
                    .ConfigureAwait(false);

                var entities = response.Result?.ToList();
                var relatedEntities = response.RelatedEntities;

                Unflatten(entities, relatedEntities, cancellation);

                result = new EntitiesResult<TEntity>(entities, entities?.Count);
            }
            else
            {
                result = EntitiesResult<TEntity>.Empty();
            }

            return result;
        }

        #endregion
    }
}
