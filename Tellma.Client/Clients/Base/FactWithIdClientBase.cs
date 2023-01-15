using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;

namespace Tellma.Client
{
    public abstract class FactWithIdClientBase<TEntity, TKey> : FactClientBase<TEntity>
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        internal FactWithIdClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntitiesResult<TEntity>> GetByIds(Request<GetByIdsArguments<TKey>> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("by-ids");

            // Add query parameters
            var args = request?.Arguments ?? new GetByIdsArguments<TKey>();
            if (args.I == null || !args.I.Any())
            {
                // Not Ids, no entities
                return new EntitiesResult<TEntity>(new List<TEntity>(), 0);
            }

            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            foreach (var id in args.I)
            {
                urlBldr.AddQueryParameter(nameof(args.I), id?.ToString());
            }

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<TEntity>>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result.ToList();
            var relatedEntities = response.RelatedEntities;

            Unflatten(entities, relatedEntities, cancellation);

            var result = new EntitiesResult<TEntity>(entities, entities.Count);
            return result;
        }

        #endregion
    }
}
