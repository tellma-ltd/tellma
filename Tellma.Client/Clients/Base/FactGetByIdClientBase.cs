using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;

namespace Tellma.Client
{
    public abstract class FactGetByIdClientBase<TEntity, TKey> : FactWithIdClientBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>

    {
        #region Lifecycle

        internal FactGetByIdClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntityResult<TEntity>> GetById(TKey id, Request<GetByIdArguments> request = null, CancellationToken cancellation = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder(id.ToString());

            // Add query parameters
            var args = request?.Arguments ?? new GetByIdArguments();

            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<GetByIdResponse<TEntity>>(cancellation)
                .ConfigureAwait(false);

            var entity = response.Result;
            var relatedEntities = response.RelatedEntities;

            var singleton = new List<TEntity> { entity };
            Unflatten(singleton, relatedEntities, cancellation);

            var result = new EntityResult<TEntity>(entity);
            return result;
        }

        public virtual async Task<Stream> PrintById(TKey id, int templateId, Request<PrintEntityByIdArguments> request, CancellationToken cancellation = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder($"{id}/print/{templateId}");

            // Add query parameters
            var args = request?.Arguments ?? new PrintEntityByIdArguments();

            urlBldr.AddQueryParameter(nameof(args.Culture), args.Culture);

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var stream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return stream;
        }

        #endregion
    }
}
