using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Dto;
using Tellma.Connector.MarminAe;
using Tellma.Repository.Application;
using Tellma.Utilities.Common;

namespace Tellma.Api.MarminAe
{
    /// <summary>
    /// Applies an inbound Marmin webhook delivery to the tenant that owns the document.
    /// </summary>
    /// <remarks>
    /// Deliberately separate from the middleware that receives the HTTP request, so that the
    /// "what does this event mean and what should it change" half is ordinary testable code with
    /// no <c>HttpContext</c> in sight.
    /// </remarks>
    public interface IMarminAeCallbackHandler
    {
        /// <summary>Handles one verified, parsed webhook event.</summary>
        Task<MarminAeCallbackOutcome> HandleAsync(
            int tenantId, MarminAeWebhookEvent webhookEvent, CancellationToken cancellation);

        /// <summary>
        /// Returns the tenant's webhook signing secrets, so the middleware can verify the
        /// signature before it parses or trusts anything in the body.
        /// </summary>
        Task<MarminAeCallbackContext> GetContextAsync(int tenantId, CancellationToken cancellation);
    }

    /// <summary>What the handler did with an event.</summary>
    public enum MarminAeCallbackOutcome
    {
        /// <summary>The document's status was updated.</summary>
        Applied,

        /// <summary>
        /// Nothing changed: a duplicate delivery, a stale one, or a document this tenant does not
        /// have. All three are answered with 200; see the middleware for why.
        /// </summary>
        Ignored,
    }

    /// <summary>What the middleware needs before it can trust a delivery.</summary>
    public class MarminAeCallbackContext
    {
        /// <summary>Every configured signing secret, to support rotation.</summary>
        public IReadOnlyList<string> Secrets { get; set; } = [];

        /// <summary>
        /// The vendor org id this tenant expects, or null if it has not been configured.
        /// Checked as defence in depth against one tenant being handed the other's callback URL.
        /// </summary>
        public string ExpectedOrgId { get; set; }
    }

    /// <inheritdoc cref="IMarminAeCallbackHandler"/>
    public class MarminAeCallbackHandler : IMarminAeCallbackHandler
    {
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ISettingsCache _settingsCache;
        private readonly MarminAeService _marminAeService;

        public MarminAeCallbackHandler(
            IApplicationRepositoryFactory repoFactory,
            ISettingsCache settingsCache,
            MarminAeService marminAeService)
        {
            _repoFactory = repoFactory;
            _settingsCache = settingsCache;
            _marminAeService = marminAeService;
        }

        /// <inheritdoc/>
        public async Task<MarminAeCallbackContext> GetContextAsync(int tenantId, CancellationToken cancellation)
        {
            var settings = await GetSettings(tenantId, cancellation);

            return new MarminAeCallbackContext
            {
                Secrets = _marminAeService.GetWebhookSecrets(settings),
                ExpectedOrgId = settings?.MarminAeOrgId,
            };
        }

        /// <inheritdoc/>
        public async Task<MarminAeCallbackOutcome> HandleAsync(
            int tenantId, MarminAeWebhookEvent webhookEvent, CancellationToken cancellation)
        {
            var settings = await GetSettings(tenantId, cancellation);

            // The event says only that something changed, never what it changed to, so the current
            // state has to be read back from the vendor. This is the same call, and the same
            // status mapping, that the "Refresh e-invoice status" action makes.
            var documentType = DocumentTypeOf(webhookEvent.EventType);
            var status = await _marminAeService.GetStatusAsync(
                webhookEvent.ResourceId.ToString(), documentType, settings, cancellation);

            // Its own transaction, and RequiresNew: this runs on a request that owns no other
            // work, and must not enlist in anything ambient.
            using var trx = TransactionFactory.Serializable(TransactionScopeOption.RequiresNew);

            var repo = _repoFactory.GetRepository(tenantId);
            int rowsAffected = await repo.MarminAe__ApplyWebhook(
                marminAeDocumentId: webhookEvent.ResourceId.ToString(),
                state: status.State,
                result: status.ResultJson,
                webhookEventId: webhookEvent.WebhookEventId,
                eventTimestamp: webhookEvent.EventTimestamp,
                cancellation: cancellation);

            trx.Complete();

            return rowsAffected > 0 ? MarminAeCallbackOutcome.Applied : MarminAeCallbackOutcome.Ignored;
        }

        private async Task<SettingsForClient> GetSettings(int tenantId, CancellationToken cancellation)
        {
            // Passing null forces the cache to load rather than to validate a version we do not
            // have: there is no user session behind a webhook to have carried one.
            var versioned = await _settingsCache.GetSettings(tenantId, version: null, cancellation);
            return versioned?.Data;
        }

        /// <summary>
        /// Maps a vendor event type to the document kind, so the status read hits the right route.
        /// </summary>
        /// <remarks>
        /// The event-type vocabulary is open, so anything unrecognized is treated as a sales
        /// invoice: those are the overwhelming majority, and a wrong guess costs one 404 from the
        /// vendor rather than a bad write.
        /// </remarks>
        private static string DocumentTypeOf(string eventType) =>
            eventType != null && eventType.Contains("credit", StringComparison.OrdinalIgnoreCase)
                ? nameof(MarminAeDocumentKind.SalesCreditNote)
                : nameof(MarminAeDocumentKind.SalesInvoice);
    }
}
