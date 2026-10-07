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
        /// Builds everything needed to send one document: which vendor route, the request body,
        /// and whether it is a new document (POST) or a resubmission of one the vendor already
        /// holds (PUT).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the whole mapping, so <c>DocumentsService</c> calls it as a dry run before
        /// claiming a document, while the close can still be rolled back: anything the mapper
        /// would refuse then refuses the close, with the reason, instead of being discovered after
        /// the document has committed.
        /// </para>
        /// <para>
        /// A resubmission is chosen by the presence of the vendor's id, not by the state. The id
        /// is recorded only once the vendor has accepted a submission, so if it is there the vendor
        /// holds this document, and a POST would be refused as a duplicate document number.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The row is missing or carries something the vendor would refuse. In practice
        /// <c>bll.Documents_Validate__Close</c> has already rejected these cases with a localized
        /// message, so reaching here means a validation gap rather than user error.
        /// </exception>
        public static MarminAePreparedSubmission Prepare(MarminAeInvoice inv)
        {
            ArgumentNullException.ThrowIfNull(inv);

            var kind = ToKind(inv.DocumentType);
            return new MarminAePreparedSubmission
            {
                DocumentId = inv.Id,
                DocumentNumber = inv.DocumentNumber,
                Kind = kind,
                ExistingDocumentId = string.IsNullOrWhiteSpace(inv.MarminAeDocumentId) ? null : inv.MarminAeDocumentId,
                Invoice = kind == MarminAeDocumentKind.SalesInvoice ? ToSalesInvoice(inv) : null,
                CreditNote = kind == MarminAeDocumentKind.SalesCreditNote ? ToSalesCreditNote(inv) : null,
            };
        }

        /// <summary>
        /// Maps an invoice row to a sales invoice request.
        /// </summary>
        /// <exception cref="ArgumentException">The row is missing something the vendor requires.</exception>
        public static MarminAeSalesInvoiceRequest ToSalesInvoice(MarminAeInvoice inv)
        {
            ArgumentNullException.ThrowIfNull(inv);

            return new MarminAeSalesInvoiceRequest
            {
                InvoiceTypeCode = Required(inv.TypeCode, nameof(inv.TypeCode)),

                // The vendor requires a due date on an invoice. The SQL already falls back to
                // issue date + 30 days, so this is only null if that ran on a document with neither
                // a posting date nor a StateAt, which cannot happen.
                DueDate = ToDateOnly(inv.DueDate ?? inv.IssueDate),

                IssueDate = ToDateOnly(inv.IssueDate),
                DocumentNumber = inv.DocumentNumber,
                ProfileExecutionId = Required(inv.ProfileExecutionId, nameof(inv.ProfileExecutionId)),
                DocumentCurrencyCode = Required(inv.DocumentCurrencyCode, nameof(inv.DocumentCurrencyCode)),
                Note = inv.Note,
                BuyerReference = inv.BuyerReference,
                PayableRoundingAmount = NullIfZero(inv.PayableRoundingAmount),
                AccountingCustomerParty = ToParty(inv),
                Delivery = ToDelivery(inv),
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
                Delivery = ToDelivery(inv),
                PaymentMeans = ToPaymentMeans(inv),
                DocumentLines = ToLines(inv),
            };
        }

        /// <summary>
        /// Translates a Peppol status string into the state stored on the document.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Shared by the webhook handler, the "Refresh e-invoice status" action and the resubmit
        /// action, so they can never disagree.
        /// </para>
        /// <para>
        /// VALIDATION_FAILED and REJECTED are kept apart because the vendor treats them
        /// differently: it lets a VALIDATION_FAILED document be resubmitted, and refuses to
        /// resubmit a REJECTED one, whose remedy is a credit note and a new invoice.
        /// </para>
        /// <para>
        /// The vendor's status vocabulary is explicitly open, so anything unrecognized is treated
        /// as still in flight rather than as a failure -- calling an unknown string a rejection
        /// would wrongly alarm the tenant about a live invoice.
        /// </para>
        /// </remarks>
        public static MarminAeState ToState(string peppolStatus) => peppolStatus switch
        {
            MarminAePeppolStatus.Approved => MarminAeState.Delivered,
            MarminAePeppolStatus.ValidationFailed => MarminAeState.PeppolValidationFailed,
            MarminAePeppolStatus.Rejected => MarminAeState.PeppolRejected,
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

            // The SQL decides the endpoint from the customer's Peppol registration (their TIN, or
            // one of the FTA placeholders 9900000098 / 9900000099). It is required on every
            // document: since 2026-07-07 the vendor no longer supplies a default.
            EndpointId = Required(inv.CustomerEndpointId, nameof(inv.CustomerEndpointId)),
            EndpointSchemeId = Required(inv.CustomerEndpointSchemeId, nameof(inv.CustomerEndpointSchemeId)),

            // The 10-digit TIN, set only for a UAE customer: the vendor rejects tin on a foreign one.
            Tin = NullIfBlank(inv.CustomerTin),

            // The 15-digit TRN travels separately from the TIN, as the buyer's VAT registration.
            // It is UAE VAT Executive Regulation Art. 59(1)(c) / MoF field #24, and the vendor
            // forbids it for a non-UAE buyer, which is why the SQL sets it for UAE customers only.
            PartyTaxScheme = string.IsNullOrWhiteSpace(inv.CustomerTrn)
                ? null
                : new MarminAePartyTaxScheme { CompanyId = inv.CustomerTrn, TaxScheme = "VAT" },

            PostalAddress = ToAddress(inv),
        };

        // The vendor requires every one of these on every address, UAE or not.
        private static MarminAeAddress ToAddress(MarminAeInvoice inv) => new()
        {
            StreetName = Required(inv.CustomerStreetName, nameof(inv.CustomerStreetName)),
            AdditionalStreetName = NullIfBlank(inv.CustomerAdditionalStreetName),
            CityName = Required(inv.CustomerCityName, nameof(inv.CustomerCityName)),
            PostalZone = NullIfBlank(inv.CustomerPostalZone),
            CountrySubentity = Required(inv.CustomerCountrySubentity, nameof(inv.CustomerCountrySubentity)),
            Country = Required(inv.CustomerCountry, nameof(inv.CustomerCountry)),
            CountryCode = Required(inv.CustomerCountryCode, nameof(inv.CustomerCountryCode)),
        };

        /// <summary>
        /// The delivery block, which the vendor requires on an export (profile_execution_id
        /// position 8) and refuses the document without.
        /// </summary>
        /// <remarks>
        /// Tellma records no separate delivery address, so the customer's address stands in for
        /// it; for an export that is the point, since the vendor requires the delivery country to
        /// be outside the UAE, and bll.Documents_Validate__Close only allows an export to a
        /// customer outside the UAE. No delivery date is sent, because Tellma does not know one.
        /// </remarks>
        private static MarminAeDelivery ToDelivery(MarminAeInvoice inv) =>
            IsExport(inv.ProfileExecutionId)
                ? new MarminAeDelivery { DeliveryLocation = new MarminAeDeliveryLocation { Address = ToAddress(inv) } }
                : null;

        /// <summary>Whether the scenario flags mark an export: the eighth of eight flags.</summary>
        public static bool IsExport(string profileExecutionId) =>
            profileExecutionId is { Length: 8 } && profileExecutionId[7] == '1';

        private static IReadOnlyList<MarminAePaymentMeans> ToPaymentMeans(MarminAeInvoice inv)
        {
            // The vendor requires at least one payment instruction unless the document is a deemed
            // supply. The SQL defaults the code to 30 (credit transfer), so it is always present.
            var code = Required(inv.PaymentMeansCode, nameof(inv.PaymentMeansCode));

            // A credit transfer must name the account to pay into (IBR-192-AE).
            var account = code == CreditTransfer
                ? Required(inv.PayeeFinancialAccountId, nameof(inv.PayeeFinancialAccountId))
                : NullIfBlank(inv.PayeeFinancialAccountId);

            return
            [
                new MarminAePaymentMeans
                {
                    PaymentMeansCode = code,
                    PayeeFinancialAccount = account == null ? null : new MarminAePayeeFinancialAccount { Id = account },
                }
            ];
        }

        /// <summary>UNCL4461 "credit transfer", the default payment means.</summary>
        private const string CreditTransfer = "30";

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

            // The vendor refuses a quantity that is not positive. The sign convention is applied in
            // SQL, so a non-positive value here means a line entered with the wrong direction.
            Quantity = line.Quantity > 0m
                ? line.Quantity
                : throw new ArgumentException(
                    $"Line '{line.Name}' has a quantity of {line.Quantity}; Marmin requires a positive quantity.",
                    nameof(line)),
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

            // standard_item_identification is deliberately never sent: Resources.Identifier is free
            // text with no ISO 6523 scheme, and PINT-AE IBR-064 rejects an identifier without one.

            LineObjectIdentifier = line.LineNumber.ToString(),
        };

        private static DateOnly ToDateOnly(DateTime value) => DateOnly.FromDateTime(value);

        /// <summary>Keeps a zero rounding adjustment off the wire entirely.</summary>
        private static decimal? NullIfZero(decimal? value) => value is null or 0m ? null : value;

        private static string NullIfBlank(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

        private static string Required(string value, string name) =>
            string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException($"Marmin requires {name}, which was empty.", name)
                : value;

        #endregion
    }

    /// <summary>
    /// One document, fully mapped and ready to send. Produced by
    /// <see cref="MarminAeMapper.Prepare(MarminAeInvoice)"/>.
    /// </summary>
    public sealed class MarminAePreparedSubmission
    {
        /// <summary>The Tellma document Id.</summary>
        public int DocumentId { get; init; }

        /// <summary>Tellma's document code, which is also the vendor's document_number.</summary>
        public string DocumentNumber { get; init; }

        /// <summary>Which vendor route the document goes to.</summary>
        public MarminAeDocumentKind Kind { get; init; }

        /// <summary>
        /// The vendor's id when it already holds this document; the submission is then a PUT
        /// resubmission of it, which the vendor allows only while it is VALIDATION_FAILED.
        /// </summary>
        public string ExistingDocumentId { get; init; }

        /// <summary>Set when <see cref="Kind"/> is a sales invoice.</summary>
        public MarminAeSalesInvoiceRequest Invoice { get; init; }

        /// <summary>Set when <see cref="Kind"/> is a sales credit note.</summary>
        public MarminAeSalesCreditNoteRequest CreditNote { get; init; }

        /// <summary>True when this replaces a document the vendor already holds.</summary>
        public bool IsResubmission => ExistingDocumentId != null;
    }
}
