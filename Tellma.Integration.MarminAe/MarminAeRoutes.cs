// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Text;

namespace Tellma.Connector.MarminAe
{
    /// <summary>
    ///     Builds every path and query string the client sends, so that a vendor route correction
    ///     is one edit in one file.
    /// </summary>
    /// <remarks>
    ///     Every route is returned <em>relative</em> and without a leading slash. Resolving an
    ///     absolute path against a base address silently discards whatever path the base itself
    ///     carried, which would break any deployment sitting behind a gateway prefix; a relative
    ///     path composes.
    /// </remarks>
    internal static class MarminAeRoutes
    {
        /// <summary>The token endpoint, which sits outside the versioned API surface.</summary>
        internal const string TokenPath = "auth/token";

        /// <summary>The format every date on the wire uses.</summary>
        internal const string DateFormat = "yyyy-MM-dd";

        /// <summary>The path segment naming a document family.</summary>
        /// <param name="kind">The family.</param>
        /// <returns>The segment the vendor uses for that family.</returns>
        internal static string Segment(MarminAeDocumentKind kind)
        {
            return kind switch
            {
                MarminAeDocumentKind.SalesInvoice => "sales-invoices",
                MarminAeDocumentKind.SalesCreditNote => "sales-credit-notes",
                MarminAeDocumentKind.PurchaseInvoice => "purchase-invoices",
                MarminAeDocumentKind.PurchaseCreditNote => "purchase-credit-notes",

                // An unnamed value can only arrive from a cast, and turning one into a route would
                // put a garbage segment on the wire instead of failing where the mistake was made.
                _ => throw new ArgumentOutOfRangeException(
                    nameof(kind), kind, "Unknown document family."),
            };
        }

        /// <summary>The token request, carrying the client id as a query parameter.</summary>
        /// <param name="clientId">The organization client id.</param>
        /// <returns>The relative route.</returns>
        internal static string Token(string clientId)
        {
            return string.Concat(TokenPath, "?client_id=", Uri.EscapeDataString(clientId));
        }

        /// <summary>Creating a document in one of the sales families.</summary>
        /// <param name="kind">The family.</param>
        /// <param name="businessProfileId">The issuing business profile.</param>
        /// <returns>The relative route.</returns>
        internal static string Create(MarminAeDocumentKind kind, string businessProfileId)
        {
            return string.Concat("api/", Segment(kind), "/", Uri.EscapeDataString(businessProfileId));
        }

        /// <summary>Replacing and resubmitting a document in one of the sales families.</summary>
        /// <remarks>
        ///     The vendor puts the document id <em>before</em> the profile id on this one route,
        ///     the opposite order to how every method here takes them. Confining that difference to
        ///     this method is most of why the file exists.
        /// </remarks>
        /// <param name="kind">The family.</param>
        /// <param name="businessProfileId">The issuing business profile.</param>
        /// <param name="documentId">The document being replaced.</param>
        /// <returns>The relative route.</returns>
        internal static string Resubmit(
            MarminAeDocumentKind kind, string businessProfileId, string documentId)
        {
            return string.Concat(
                "api/",
                Segment(kind),
                "/",
                Uri.EscapeDataString(documentId),
                "/",
                Uri.EscapeDataString(businessProfileId));
        }

        /// <summary>Reading one document.</summary>
        /// <param name="kind">The family.</param>
        /// <param name="documentId">The document.</param>
        /// <returns>The relative route.</returns>
        internal static string Document(MarminAeDocumentKind kind, string documentId)
        {
            return string.Concat("api/", Segment(kind), "/", Uri.EscapeDataString(documentId));
        }

        /// <summary>Listing a family, filtered and paged.</summary>
        /// <param name="kind">The family.</param>
        /// <param name="query">The filters and paging.</param>
        /// <returns>The relative route.</returns>
        internal static string List(MarminAeDocumentKind kind, MarminAeDocumentQuery query)
        {
            StringBuilder builder = new();
            builder.Append("api/").Append(Segment(kind));

            QueryWriter writer = new(builder);
            writer.Add("document_number", query.DocumentNumber);
            writer.Add("billed_by_name", query.BilledByName);
            writer.Add("billed_to_name", query.BilledToName);
            writer.Add("billed_to_vat", query.BilledToVat);
            writer.Add("billed_by_vat", query.BilledByVat);
            writer.Add("status", query.Status);
            writer.Add("document_date", Format(query.DocumentDate));
            writer.Add("page", Format(query.Page));
            writer.Add("size", Format(query.Size));

            return builder.ToString();
        }

        /// <summary>Listing a family's document ids for one creation date.</summary>
        /// <remarks>
        ///     The creation date is mandatory at the vendor: this is a per-day listing rather than
        ///     a catalogue, and a caller wanting a range iterates days.
        /// </remarks>
        /// <param name="kind">The family.</param>
        /// <param name="createdOn">The creation date to list.</param>
        /// <param name="page">The zero-indexed page, when the caller pages explicitly.</param>
        /// <param name="size">The page size, when the caller sets one.</param>
        /// <returns>The relative route.</returns>
        internal static string DocumentIds(
            MarminAeDocumentKind kind, DateOnly createdOn, int? page, int? size)
        {
            StringBuilder builder = new();
            builder.Append("api/").Append(Segment(kind)).Append("/uuids");

            QueryWriter writer = new(builder);
            writer.Add("created_on", Format(createdOn));
            writer.Add("page", Format(page));
            writer.Add("size", Format(size));

            return builder.ToString();
        }

        /// <summary>The current Peppol transmission snapshot for a document.</summary>
        /// <param name="kind">The family.</param>
        /// <param name="documentId">The document.</param>
        /// <returns>The relative route.</returns>
        internal static string PeppolStatus(MarminAeDocumentKind kind, string documentId)
        {
            return string.Concat(Document(kind, documentId), "/peppol-status");
        }

        /// <summary>The chronological Peppol transmission log for a document.</summary>
        /// <param name="kind">The family.</param>
        /// <param name="documentId">The document.</param>
        /// <returns>The relative route.</returns>
        internal static string PeppolStatusLogs(MarminAeDocumentKind kind, string documentId)
        {
            return string.Concat(Document(kind, documentId), "/peppol-status-logs");
        }

        /// <summary>The UBL 2.1 XML rendering of a document.</summary>
        /// <param name="kind">The family.</param>
        /// <param name="documentId">The document.</param>
        /// <returns>The relative route.</returns>
        internal static string Xml(MarminAeDocumentKind kind, string documentId)
        {
            return string.Concat(Document(kind, documentId), "/xml");
        }

        /// <summary>The PDF rendering of a document.</summary>
        /// <param name="kind">The family.</param>
        /// <param name="documentId">The document.</param>
        /// <returns>The relative route.</returns>
        internal static string Pdf(MarminAeDocumentKind kind, string documentId)
        {
            return string.Concat(Document(kind, documentId), "/download-pdf");
        }

        /// <summary>One supporting file attached to a document.</summary>
        /// <param name="kind">The family.</param>
        /// <param name="documentId">The document.</param>
        /// <param name="attachmentId">The attachment, as identified in the document response.</param>
        /// <returns>The relative route.</returns>
        internal static string Attachment(
            MarminAeDocumentKind kind, string documentId, string attachmentId)
        {
            return string.Concat(
                Document(kind, documentId),
                "/attachments/",
                Uri.EscapeDataString(attachmentId),
                "/download");
        }

        /// <summary>Reading one business profile.</summary>
        /// <param name="businessProfileId">The profile.</param>
        /// <returns>The relative route.</returns>
        internal static string BusinessProfile(string businessProfileId)
        {
            return string.Concat("api/business-profiles/", Uri.EscapeDataString(businessProfileId));
        }

        private static string? Format(DateOnly? value)
        {
            return value?.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        private static string? Format(int? value)
        {
            return value?.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Appends query parameters, skipping the ones the caller left unset.</summary>
        private readonly struct QueryWriter
        {
            private readonly StringBuilder _builder;
            private readonly int _pathLength;

            /// <summary>Starts writing parameters onto a route already in the buffer.</summary>
            /// <param name="builder">The buffer holding the path so far.</param>
            internal QueryWriter(StringBuilder builder)
            {
                _builder = builder;

                // The length of the path as it stood before any parameter was appended: anything
                // past it means a query string has already been opened.
                _pathLength = builder.Length;
            }

            /// <summary>Appends one parameter, unless the caller left it unset.</summary>
            /// <param name="name">The parameter name.</param>
            /// <param name="value">Its value, or null to skip it.</param>
            internal void Add(string name, string? value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                _builder.Append(_builder.Length == _pathLength ? '?' : '&');
                _builder.Append(name).Append('=').Append(Uri.EscapeDataString(value));
            }
        }
    }
}
