using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Client
{
    public class ReconciliationClient : ClientBase
    {
        internal ReconciliationClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "reconciliation";

        public async Task<ReconciliationGetUnreconciledResponse> GetUnreconciled(Request<ReconciliationGetUnreconciledArguments> request, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("unreconciled");

            var args = request?.Arguments ?? new ReconciliationGetUnreconciledArguments();
            urlBldr.AddQueryParameter(nameof(args.AccountId), args.AccountId.ToString());
            urlBldr.AddQueryParameter(nameof(args.AgentId), args.AgentId.ToString());
            urlBldr.AddQueryParameter(nameof(args.AsOfDate), args.AsOfDate?.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter(nameof(args.EntriesTop), args.EntriesTop.ToString());
            urlBldr.AddQueryParameter(nameof(args.EntriesSkip), args.EntriesSkip.ToString());
            urlBldr.AddQueryParameter(nameof(args.ExternalEntriesTop), args.ExternalEntriesTop.ToString());
            urlBldr.AddQueryParameter(nameof(args.ExternalEntriesSkip), args.ExternalEntriesSkip.ToString());

            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<ReconciliationGetUnreconciledResponse>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<ReconciliationGetReconciledResponse> GetReconciled(Request<ReconciliationGetReconciledArguments> request, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("reconciled");

            var args = request?.Arguments ?? new ReconciliationGetReconciledArguments();
            urlBldr.AddQueryParameter(nameof(args.AccountId), args.AccountId.ToString());
            urlBldr.AddQueryParameter(nameof(args.AgentId), args.AgentId.ToString());
            urlBldr.AddQueryParameter(nameof(args.FromDate), args.FromDate?.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter(nameof(args.ToDate), args.ToDate?.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter(nameof(args.FromAmount), args.FromAmount?.ToString());
            urlBldr.AddQueryParameter(nameof(args.ToAmount), args.ToAmount?.ToString());
            urlBldr.AddQueryParameter(nameof(args.ExternalReferenceContains), args.ExternalReferenceContains);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());

            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<ReconciliationGetReconciledResponse>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<ReconciliationGetUnreconciledResponse> SaveAndGetUnreconciled(ReconciliationSavePayload payload, Request<ReconciliationGetUnreconciledArguments> request, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("unreconciled");

            var args = request?.Arguments ?? new ReconciliationGetUnreconciledArguments();
            urlBldr.AddQueryParameter(nameof(args.AccountId), args.AccountId.ToString());
            urlBldr.AddQueryParameter(nameof(args.AgentId), args.AgentId.ToString());
            urlBldr.AddQueryParameter(nameof(args.AsOfDate), args.AsOfDate?.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter(nameof(args.EntriesTop), args.EntriesTop.ToString());
            urlBldr.AddQueryParameter(nameof(args.EntriesSkip), args.EntriesSkip.ToString());
            urlBldr.AddQueryParameter(nameof(args.ExternalEntriesTop), args.ExternalEntriesTop.ToString());
            urlBldr.AddQueryParameter(nameof(args.ExternalEntriesSkip), args.ExternalEntriesSkip.ToString());

            using var msg = new HttpRequestMessage(HttpMethod.Post, urlBldr.Uri)
            {
                Content = JsonContent.Create(payload, options: new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                })
            };

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<ReconciliationGetUnreconciledResponse>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<ReconciliationGetReconciledResponse> SaveAndGetReconciled(ReconciliationSavePayload payload, Request<ReconciliationGetReconciledArguments> request, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("reconciled");

            var args = request?.Arguments ?? new ReconciliationGetReconciledArguments();
            urlBldr.AddQueryParameter(nameof(args.AccountId), args.AccountId.ToString());
            urlBldr.AddQueryParameter(nameof(args.AgentId), args.AgentId.ToString());
            urlBldr.AddQueryParameter(nameof(args.FromDate), args.FromDate?.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter(nameof(args.ToDate), args.ToDate?.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter(nameof(args.FromAmount), args.FromAmount?.ToString());
            urlBldr.AddQueryParameter(nameof(args.ToAmount), args.ToAmount?.ToString());
            urlBldr.AddQueryParameter(nameof(args.ExternalReferenceContains), args.ExternalReferenceContains);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());

            using var msg = new HttpRequestMessage(HttpMethod.Post, urlBldr.Uri)
            {
                Content = JsonContent.Create(payload, options: new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                })
            };

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<ReconciliationGetReconciledResponse>(cancellation)
                .ConfigureAwait(false);
        }
    }
}
