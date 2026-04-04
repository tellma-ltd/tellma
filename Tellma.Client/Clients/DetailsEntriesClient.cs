using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class DetailsEntriesClient : FactWithIdClientBase<DetailsEntry, int>
    {
        internal DetailsEntriesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "details-entries";

        public async Task<StatementResponse> GetStatement(Request<StatementArguments> request, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("statement");

            var args = request?.Arguments ?? new StatementArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());
            urlBldr.AddQueryParameter(nameof(args.FromDate), args.FromDate?.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter(nameof(args.ToDate), args.ToDate?.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter(nameof(args.AccountId), args.AccountId?.ToString());
            urlBldr.AddQueryParameter(nameof(args.AgentId), args.AgentId?.ToString());
            urlBldr.AddQueryParameter(nameof(args.ResourceId), args.ResourceId?.ToString());
            urlBldr.AddQueryParameter(nameof(args.NotedAgentId), args.NotedAgentId?.ToString());
            urlBldr.AddQueryParameter(nameof(args.NotedResourceId), args.NotedResourceId?.ToString());
            urlBldr.AddQueryParameter(nameof(args.EntryTypeId), args.EntryTypeId?.ToString());
            urlBldr.AddQueryParameter(nameof(args.CenterId), args.CenterId?.ToString());
            urlBldr.AddQueryParameter(nameof(args.CurrencyId), args.CurrencyId);
            urlBldr.AddQueryParameter(nameof(args.IncludeCompleted), args.IncludeCompleted?.ToString());

            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<StatementResponse>(cancellation)
                .ConfigureAwait(false);
        }
    }
}
