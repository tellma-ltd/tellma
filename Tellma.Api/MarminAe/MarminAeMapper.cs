using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Connector.MarminAe;
using Tellma.Repository.Application;

namespace Tellma.Api.MarminAe
{
    /// <summary>
    /// Translates the rows <c>[dal].[MarminAe__GetInvoices]</c> returns into the request models
    /// the vendor client sends.
    /// </summary>
    /// <remarks>
    /// Deliberately a pure static class with no dependencies, so the whole translation -- which is
    /// where the fiddly, easy-to-get-wrong conversions live -- can be unit tested without a
    /// database, an HTTP client or a tenant.
    /// </remarks>
    public static class MarminAeMapper
    {
        /// <summary>
        /// Maps an invoice row to a sales invoice request.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// The row is missing something the vendor requires. In practice
        /// <c>bll.Documents_Validate__Close</c> has already rejected these cases with a localized
        /// message, so reaching here means a validation gap rather than user error.
        /// </exception>
        public static MarminAeSalesInvoiceRequest ToSalesInvoice(MarminAeInvoice inv)
        {
            ArgumentNullException.ThrowIfNull(inv);

            return new MarminAeSalesInvoiceRequest
            {
                InvoiceTypeCode = Required(inv.TypeCode, nameof(inv.TypeCode)),

                // The vendor requires a due date on an invoice. The SQL already falls back to
                // issue date + the configured payment term, so this is only null if that ran on a
                // document with neither a posting date nor a StateAt, which cannot happen.
                DueDate = ToDateOnly(inv.DueDate ?? inv.IssueDate),

                IssueDate = ToDateOnly(inv.IssueDate),
                DocumentNumber = inv.DocumentNumber,
                ProfileExecutionId = Required(inv.ProfileExecutionId, nameof(inv.ProfileExecutionId)),
                DocumentCurrencyCode = Required(inv.DocumentCurrencyCode, nameof(inv.DocumentCurrencyCode)),
                Note = inv.Note,
                BuyerReference = inv.BuyerReference,
                PayableRoundingAmount = NullIfZero(inv.PayableRoundingAmount),
                AccountingCustomerParty = ToParty(inv),
                PaymentMeans = ToPaymentMeans(inv),
                DocumentLines = ToLines(inv),
            };
        }

        /// <summary>Maps an invoice row to a sales credit note request.</summary>
        /// <exception cref="ArgumentException">The row is missing something the vendor requires.</exception>
        public static MarminAeSalesCreditNoteRequest ToSalesCreditNote(MarminAeInvoice inv)
        {
            ArgumentNullException.ThrowIfNull(inv);

            return new MarminAeSalesCreditNoteRequest
            {
                CreditNoteTypeCode = Required(inv.TypeCode, nameof(inv.TypeCode)),

                // The authority's reason code, from Documents.Lookup2Id.
                DiscrepancyResponse = Required(inv.DiscrepancyResponse, nameof(inv.DiscrepancyResponse)),
                Reason = inv.Reason,

                // The vendor requires at least one, naming the invoice being adjusted.
                // bll.Documents_Validate__Close asserts exactly one candidate exists.
                BillingReference =
                [
                    new MarminAeDocumentReference
                    {
                        Id = Required(inv.BillingReferenceId, nameof(inv.BillingReferenceId)),
                        IssueDate = inv.BillingReferenceIssueDate is DateTime d ? ToDateOnly(d) : null,
                    }
                ],

                DueDate = inv.DueDate is DateTime due ? ToDateOnly(due) : null,
                IssueDate = ToDateOnly(inv.IssueDate),
                DocumentNumber = inv.DocumentNumber,
                ProfileExecutionId = Required(inv.ProfileExecutionId, nameof(inv.ProfileExecutionId)),
                DocumentCurrencyCode = Required(inv.DocumentCurrencyCode, nameof(inv.DocumentCurrencyCode)),
                Note = inv.Note,
                BuyerReference = inv.BuyerReference,
                PayableRoundingAmount = NullIfZero(inv.PayableRoundingAmount),
                AccountingCustomerParty = ToParty(inv),
                PaymentMeans = ToPaymentMeans(inv),
                DocumentLines = ToLines(inv),
            };
        }

        /// <summary>
        /// Translates a Peppol status string into the state stored on the document.
        /// </summary>
        /// <remarks>
        /// Shared by the webhook handler and the "Refresh e-invoice status" action so the two can
        /// never disagree. The vendor's status vocabulary is explicitly open, so anything
        /// unrecognized is treated as still in flight rather than as a failure -- calling an
        /// unknown string a rejection would wrongly alarm the tenant about a live invoice.
        /// </remarks>
        public static MarminAeState ToState(string peppolStatus) => peppolStatus switch
        {
            MarminAePeppolStatus.Approved => MarminAeState.Delivered,
            MarminAePeppolStatus.Rejected => MarminAeState.PeppolRejected,
            MarminAePeppolStatus.ValidationFailed => MarminAeState.PeppolRejected,
            _ => MarminAeState.Submitted,
        };

        /// <summary>
        /// Maps the document type stored on the definition to the vendor's document kind.
        /// The stored values match the enum member names, which is what keeps this a parse.
        /// </summary>
        /// <remarks>
        /// Only the two sales kinds are accepted. The enum also has PurchaseInvoice and
        /// PurchaseCreditNote -- the client can read those, but nothing here can author them --
        /// so a plain Enum.Parse would quietly accept a value that fails much later, at the point
        /// of submission. The CHECK constraint on DocumentDefinitions.MarminAeDocumentType makes
        /// that unreachable today; this keeps it unreachable if the constraint is ever relaxed.
        /// </remarks>
        public static MarminAeDocumentKind ToKind(string documentType) => documentType switch
        {
            nameof(MarminAeDocumentKind.SalesInvoice) => MarminAeDocumentKind.SalesInvoice,
            nameof(MarminAeDocumentKind.SalesCreditNote) => MarminAeDocumentKind.SalesCreditNote,
            _ => throw new ArgumentException(
                $"Unrecognized Marmin document type '{documentType}'. Only SalesInvoice and SalesCreditNote can be submitted.",
                nameof(documentType)),
        };

        #region Helpers

        private static MarminAePartyRequest ToParty(MarminAeInvoice inv) => new()
        {
            Name = Required(inv.CustomerName, nameof(inv.CustomerName)),

            // The vendor wants party_name for a business. Both tenants invoice businesses, and we
            // have only the one name, so it does double duty.
            PartyName = inv.CustomerName,

            Email = Required(inv.CustomerEmail, nameof(inv.CustomerEmail)),
            EndpointId = Required(inv.CustomerEndpointId, nameof(inv.CustomerEndpointId)),
            EndpointSchemeId = Required(inv.CustomerEndpointSchemeId, nameof(inv.CustomerEndpointSchemeId)),

            // Only set for a UAE customer; the SQL nulls it otherwise, because the vendor rejects
            // a tin on a foreign party.
            Tin = inv.CustomerTin,

            PostalAddress = new MarminAeAddress
            {
                StreetName = inv.CustomerStreetName,
                AdditionalStreetName = inv.CustomerAdditionalStreetName,
                CityName = inv.CustomerCityName,
                PostalZone = inv.CustomerPostalZone,
                CountrySubentity = inv.CustomerCountrySubentity,
                Country = inv.CustomerCountry,
                CountryCode = inv.CustomerCountryCode,
            },
        };

        private static IReadOnlyList<MarminAePaymentMeans> ToPaymentMeans(MarminAeInvoice inv)
        {
            // The vendor requires at least one payment instruction unless the document is a deemed
            // supply. Sending an empty array would be worse than sending nothing, so omit it
            // entirely when the tenant has configured no means at all.
            if (string.IsNullOrWhiteSpace(inv.PaymentMeansCode))
            {
                return null;
            }

            return
            [
                new MarminAePaymentMeans
                {
                    PaymentMeansCode = inv.PaymentMeansCode,
                    PayeeFinancialAccount = string.IsNullOrWhiteSpace(inv.PayeeFinancialAccountId)
                        ? null
                        : new MarminAePayeeFinancialAccount { Id = inv.PayeeFinancialAccountId },
                }
            ];
        }

        private static IReadOnlyList<MarminAeDocumentLineRequest> ToLines(MarminAeInvoice inv)
        {
            if (inv.Lines == null || inv.Lines.Count == 0)
            {
                throw new ArgumentException(
                    $"Document {inv.Id} has no lines to send to Marmin.", nameof(inv));
            }

            return [.. inv.Lines.Select(ToLine)];
        }

        private static MarminAeDocumentLineRequest ToLine(MarminAeInvoiceLine line) => new()
        {
            Name = Required(line.Name, nameof(line.Name)),
            Description = Required(line.Description, nameof(line.Description)),
            Quantity = line.Quantity,
            UnitCode = Required(line.UnitCode, nameof(line.UnitCode)),

            Price = new MarminAePriceRequest
            {
                BaseAmount = line.PriceBaseAmount,

                // Guard against a zero slipping through and making the vendor divide by it.
                BaseQuantity = line.PriceBaseQuantity == 0m ? 1m : line.PriceBaseQuantity,
            },

            ClassifiedTaxCategory = new MarminAeTaxCategory
            {
                Id = Required(line.TaxCategoryId, nameof(line.TaxCategoryId)),

                // Already converted from Tellma's 0..1 fraction to a percentage in SQL.
                Percent = line.TaxPercent,
                TaxScheme = "VAT",

                // The vendor requires both of these when the category is exempt, and rejects the
                // document without them. ZATCA left the equivalent fields unmapped.
                TaxExemptionReasonCode = line.TaxExemptionReasonCode,
                TaxExemptionReason = line.TaxExemptionReason,
            },

            SellerItemIdentification = string.IsNullOrWhiteSpace(line.SellerItemIdentification)
                ? null
                : new MarminAeItemIdentification { Id = line.SellerItemIdentification },

            StandardItemIdentification = string.IsNullOrWhiteSpace(line.StandardItemIdentification)
                ? null
                : new MarminAeStandardItemIdentification { Id = line.StandardItemIdentification },

            LineObjectIdentifier = line.LineNumber.ToString(),
        };

        private static DateOnly ToDateOnly(DateTime value) => DateOnly.FromDateTime(value);

        /// <summary>Keeps a zero rounding adjustment off the wire entirely.</summary>
        private static decimal? NullIfZero(decimal? value) => value is null or 0m ? null : value;

        private static string Required(string value, string name) =>
            string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException($"Marmin requires {name}, which was empty.", name)
                : value;

        #endregion
    }
}
