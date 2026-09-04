// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

namespace Tellma.Connector.MarminAe
{
    /// <summary>One webhook notification, read from a verified payload.</summary>
    /// <remarks>
    ///     A notification carries no document state — only enough to say which document changed —
    ///     so the handler that receives one fetches the document to learn what actually happened.
    /// </remarks>
    /// <param name="OrgId">The organization that owns the document.</param>
    /// <param name="EventType">The vendor's name for what happened, verbatim. The known names are
    ///     published as constants on <see cref="MarminAeWebhookEventTypes" />, and the set stays
    ///     open.</param>
    /// <param name="ProfileId">The business profile the document was issued under.</param>
    /// <param name="ResourceId">The document that changed.</param>
    /// <param name="ResourceUrl">Where the document's current state can be read.</param>
    /// <param name="EventTimestamp">When it changed, at the vendor.</param>
    /// <param name="WebhookEventId">The vendor's identifier for this delivery, and the key to
    ///     deduplicate on: delivery is at-least-once.</param>
    public sealed record MarminAeWebhookEvent(
        Guid OrgId,
        string EventType,
        string ProfileId,
        Guid ResourceId,
        Uri ResourceUrl,
        DateTimeOffset EventTimestamp,
        Guid WebhookEventId);

    /// <summary>The event names the vendor is known to send.</summary>
    /// <remarks>
    ///     The set is open, so an unrecognised name passes through untouched rather than being
    ///     rejected. A name is a routing hint and never a statement of state: the vendor keeps only
    ///     the latest pending delivery per document, so a change can be announced as an update
    ///     whose predecessor was never delivered.
    /// </remarks>
    public static class MarminAeWebhookEventTypes
    {
        /// <summary>A sales invoice was created.</summary>
        public const string SaleInvoiceCreate = "sale.invoice.create";

        /// <summary>A sales invoice changed.</summary>
        public const string SaleInvoiceUpdate = "sale.invoice.update";

        /// <summary>A sales credit note was created.</summary>
        public const string SaleCreditNoteCreate = "sale.credit_note.create";

        /// <summary>A sales credit note changed.</summary>
        public const string SaleCreditNoteUpdate = "sale.credit_note.update";

        /// <summary>A purchase invoice was created.</summary>
        public const string PurchaseInvoiceCreate = "purchase.invoice.create";

        /// <summary>A purchase invoice changed.</summary>
        public const string PurchaseInvoiceUpdate = "purchase.invoice.update";

        /// <summary>A purchase credit note was created.</summary>
        public const string PurchaseCreditNoteCreate = "purchase.credit_note.create";

        /// <summary>A purchase credit note changed.</summary>
        public const string PurchaseCreditNoteUpdate = "purchase.credit_note.update";
    }
}
