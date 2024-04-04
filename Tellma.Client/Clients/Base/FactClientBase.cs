using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;

namespace Tellma.Client
{
    public abstract class FactClientBase<TEntity> : ClientBase where TEntity : Entity
    {
        #region Lifecycle

        internal FactClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntitiesResult<TEntity>> GetEntities(Request<GetArguments> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder();

            // Add query parameters
            var args = request?.Arguments ?? new GetArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            urlBldr.AddQueryParameter(nameof(args.OrderBy), args.OrderBy);
            urlBldr.AddQueryParameter(nameof(args.Search), args.Search);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());
            urlBldr.AddQueryParameter(nameof(args.CountEntities), args.CountEntities.ToString());

            // Prepare the message
            var method = HttpMethod.Get;
            using var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content.ReadAsAsync<GetResponse<TEntity>>(cancellation).ConfigureAwait(false);

            var entities = response.Result.ToList();
            var totalCount = response.TotalCount;
            var relatedEntities = response.RelatedEntities;

            Unflatten(entities, relatedEntities, cancellation);

            var result = new EntitiesResult<TEntity>(entities, totalCount);
            return result;
        }

        public virtual async Task<FactResult> GetFact(Request<FactArguments> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("fact");

            // Add query parameters
            var args = request?.Arguments ?? new FactArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.OrderBy), args.OrderBy);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());
            urlBldr.AddQueryParameter(nameof(args.CountEntities), args.CountEntities.ToString());

            // Prepare the message
            var method = HttpMethod.Get;
            using var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<GetFactResponse>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result.ToList();
            var totalCount = response.TotalCount;

            var result = new FactResult(entities, totalCount);
            return result;
        }

        public virtual async Task<AggregateResult> GetAggregate(Request<GetAggregateArguments> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("aggregate");

            // Add query parameters
            var args = request?.Arguments ?? new GetAggregateArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.OrderBy), args.OrderBy);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.Having), args.Having);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());

            // Prepare the message
            var method = HttpMethod.Get;
            using var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<GetAggregateResponse>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result.ToList();
            var ancestors = response.DimensionAncestors.Select(e => new DimensionAncestorsResult(e.Result, e.IdIndex, e.MinIndex));

            var result = new AggregateResult(entities, ancestors);
            return result;
        }

        // TODO: Print API

        #endregion

        #region Helpers

        protected void Unflatten(IEnumerable<TEntity> resultEntities, RelatedEntities relatedEntities, CancellationToken cancellation)
        {
            ClientUtil.Unflatten(resultEntities, relatedEntities, cancellation);
        }

        #endregion
    }
}
