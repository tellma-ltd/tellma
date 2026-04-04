using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;

namespace Tellma.Client
{
    public abstract class CrudTreeClientBase<TEntityForSave, TEntity, TKey> : CrudClientBase<TEntityForSave, TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        internal CrudTreeClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntitiesResult<TEntity>> GetChildrenOf(Request<GetChildrenArguments<TKey>> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("children-of");

            // Add query parameters
            var args = request?.Arguments ?? new GetChildrenArguments<TKey>();

            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.Roots), args.Roots.ToString());

            if (args.I != null)
            {
                foreach (var id in args.I)
                {
                    urlBldr.AddQueryParameter(nameof(args.I), id?.ToString());
                }
            }

            // Prepare the message
            var method = HttpMethod.Get;
            using var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<TEntity>>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result?.ToList();
            var relatedEntities = response.RelatedEntities;

            Unflatten(entities, relatedEntities, cancellation);

            return new EntitiesResult<TEntity>(entities, entities?.Count);
        }

        public virtual async Task DeleteWithDescendants(List<TKey> ids, Request request = null, CancellationToken cancellation = default)
        {
            if (ids == null || ids.Count == 0)
            {
                return;
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("with-descendants");

            // Add query parameters
            foreach (var id in ids)
            {
                urlBldr.AddQueryParameter("I", id?.ToString());
            }

            // Prepare the message
            var method = HttpMethod.Delete;
            using var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);
        }

        #endregion
    }
}
