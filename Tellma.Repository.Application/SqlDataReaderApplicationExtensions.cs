using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    public static class SqlDataReaderApplicationExtensions
    {
        /// <summary>
        /// First loads the <see cref="ValidationError"/>s, if none are returned it moves
        /// to the next result set and loads the ids of deleted images, then if returnIds 
        /// is true moves to the next result set and loads the entity ids sorted by index. 
        /// Returns the errors, the ids, and images ids in a <see cref="SaveWithImagesOutput"/> object.
        /// </summary>
        /// <param name="returnIds">Whether or not to return the Ids.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<SaveWithImagesOutput> LoadSaveWithImagesResult(this SqlDataReader reader, bool returnIds, bool validateOnly, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);
            bool proceed = !errors.Any() && !validateOnly;

            // (2) Load the deleted image ids
            var deletedImageIds = new List<string>();
            List<int> ids = null;
            if (proceed)
            {
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    deletedImageIds.Add(reader.String(0));
                }

                // (3) If no errors => load the Ids
                await reader.NextResultAsync(cancellation);
                if (returnIds)
                {
                    ids = await reader.LoadIds(cancellation);
                }
            }

            // (4) Return the result
            return new SaveWithImagesOutput(errors, ids, deletedImageIds);
        }

        /// <summary>
        /// First loads the <see cref="ValidationError"/>s, if none are returned it moves 
        /// to the next result set and loads the ids of deleted images. 
        /// Returns the errors and images ids in a <see cref="DeleteWithImagesOutput"/> object.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<DeleteWithImagesOutput> LoadDeleteWithImagesResult(this SqlDataReader reader, bool validateOnly, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);
            bool proceed = !errors.Any() && !validateOnly;

            // (2) Load the deleted image ids
            var deletedImageIds = new List<string>();
            if (proceed)
            {
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    deletedImageIds.Add(reader.String(0));
                }

                // (3) Execute the delete (othewise any SQL errors won't be returned)
                await reader.NextResultAsync(cancellation);
            }

            // (4) Return the result
            return new DeleteWithImagesOutput(errors, deletedImageIds);
        }

        /// <summary>
        /// First loads the <see cref="ValidationError"/>s, if none are returned and returnIds is true it moves
        /// to the next result set and loads the document ids. Returns both the errors and the ids in a <see cref="SaveOutput"/> object.
        /// </summary>
        /// <param name="returnIds">Whether or not to return the document Ids.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public static async Task<SignOutput> LoadSignResult(this SqlDataReader reader, bool returnIds, bool validateOnly, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);
            bool proceed = !errors.Any() && !validateOnly;

            // (2) If no errors => load the Ids
            var documentIds = new List<int>();
            if (proceed)
            {
                await reader.NextResultAsync(cancellation);
                if (returnIds)
                {
                    while (await reader.ReadAsync(cancellation))
                    {
                        documentIds.Add(reader.GetInt32(0));
                    }
                }
            }

            // (3) Return the result
            return new SignOutput(errors, documentIds);
        }

        public static async Task<List<InboxStatus>> LoadInboxStatuses(this SqlDataReader reader, CancellationToken cancellation = default)
        {
            var result = new List<InboxStatus>();

            while (await reader.ReadAsync(cancellation))
            {
                int i = 0;
                var externalId = reader.GetString(i++);
                var count = reader.GetInt32(i++);
                var unknownCount = reader.GetInt32(i++);

                result.Add(new InboxStatus(externalId, count, unknownCount));
            }

            return result;
        }

        public static async Task<InboxStatusOutput> LoadInboxStatusOutput(this SqlDataReader reader, bool validateOnly, CancellationToken cancellation = default)
        {
            // (1) Load the errors
            var errors = await reader.LoadErrors(cancellation);
            bool proceed = !errors.Any() && !validateOnly;

            // (2) If no errors => load the Ids
            List<InboxStatus> inboxStatuses = default;
            if (proceed)
            {
                await reader.NextResultAsync(cancellation);
                inboxStatuses = await reader.LoadInboxStatuses(cancellation);
            }

            // (3) Return the result
            return new InboxStatusOutput(errors, inboxStatuses);
        }

        public static async Task<List<ZatcaInvoice>> LoadZatcaInvoices(this SqlDataReader reader, CancellationToken cancellation = default)
        {
            var result = new List<ZatcaInvoice>();

            // 1 - Load the invoices
            while (await reader.ReadAsync(cancellation))
            {
                int i = 0;
                int index = reader.GetInt32(i++);
                while (result.Count <= index) result.Add(null);

                result[index] = new ZatcaInvoice
                {
                    Id = reader.GetInt32(i++),
                    InvoiceNumber = reader.String(i++),
                    UniqueInvoiceIdentifier = reader.GetGuid(i++),
                    InvoiceIssueDateTime = reader.GetDateTimeOffset(i++),
                    InvoiceType = reader.GetInt32(i++),
                    IsSimplified = reader.Boolean(i++) ?? false,
                    IsThirdParty = reader.Boolean(i++) ?? false,
                    IsNominal = reader.Boolean(i++) ?? false,
                    IsExports = reader.Boolean(i++) ?? false,
                    IsSummary = reader.Boolean(i++) ?? false,
                    IsSelfBilled = reader.Boolean(i++) ?? false,
                    InvoiceNote = reader.String(i++),
                    InvoiceCurrency = reader.String(i++),
                    PurchaseOrderId = reader.String(i++),
                    BillingReferenceId = reader.String(i++),
                    ContractId = reader.String(i++),
                    BuyerId = reader.String(i++),
                    BuyerIdScheme = reader.String(i++), // VAT or else
                    BuyerAddressStreet = reader.String(i++),
                    BuyerAddressAdditionalStreet = reader.String(i++),
                    BuyerAddressBuildingNumber = reader.String(i++),
                    BuyerAddressAdditionalNumber = reader.String(i++),
                    BuyerAddressCity = reader.String(i++),
                    BuyerAddressPostalCode = reader.String(i++),
                    BuyerAddressProvince = reader.String(i++),
                    BuyerAddressDistrict = reader.String(i++),
                    BuyerAddressCountryCode = reader.String(i++),
                    BuyerName = reader.String(i++),
                    SupplyDate = reader.DateTime(i++) ?? default,
                    SupplyEndDate = reader.DateTime(i++) ?? default,
                    PaymentMeans = reader.GetInt32(i++),
                    ReasonForIssuanceOfCreditDebitNote = reader.String(i++),
                    PaymentTerms = reader.String(i++),
                    PaymentAccountId = reader.String(i++),
                    InvoiceTotalVatAmountInAccountingCurrency = reader.Decimal(i++) ?? 0m,
                    PrepaidAmount = reader.Decimal(i++) ?? 0m,
                    RoundingAmount = reader.Decimal(i++) ?? 0m,
                    VatCategoryTaxableAmount = reader.Decimal(i++) ?? 0m,
                    VatCategory = reader.String(i++), // E, S, Z, O
                    VatRate = reader.Decimal(i++) ?? 0m,
                    VatExemptionReason = reader.String(i++),
                    VatExemptionReasonCode = reader.String(i++),
                };
            }

            // 2 - Load the Allowances/Charges
            await reader.NextResultAsync(cancellation);
            while (await reader.ReadAsync(cancellation))
            {
                int i = 0;
                int invoiceIndex = reader.GetInt32(i++);
                var invoice = result[invoiceIndex];

                invoice.AllowanceCharges.Add(new ZatcaAllowanceCharge
                {
                    IsCharge = reader.Boolean(i++) ?? false,
                    Amount = reader.Decimal(i++) ?? 0m,
                    Reason = reader.String(i++),
                    ReasonCode = reader.String(i++),
                    VatCategory = reader.String(i++),
                    VatRate = reader.Decimal(i++) ?? 0m,
                });
            }

            // 3 - Load the Invoice/Lines
            await reader.NextResultAsync(cancellation);
            while (await reader.ReadAsync(cancellation))
            {
                int i = 0;
                int invoiceIndex = reader.GetInt32(i++);
                var invoice = result[invoiceIndex];

                invoice.Lines.Add(new ZatcaInvoiceLine
                {
                    Id = reader.GetInt32(i++),
                    PrepaymentId = reader.String(i++),
                    PrepaymentUuid = reader.Guid(i++) ?? default,
                    PrepaymentIssueDateTime = reader.GetDateTimeOffset(i++),
                    Quantity = reader.Decimal(i++) ?? 0m,
                    QuantityUnit = reader.String(i++),
                    NetAmount = reader.Decimal(i++) ?? 0m,
                    AllowanceChargeIsCharge = reader.Boolean(i++) ?? false,
                    AllowanceChargeAmount = reader.Decimal(i++) ?? 0m,
                    AllowanceChargeReason = reader.String(i++),
                    AllowanceChargeReasonCode = reader.String(i++),
                    VatAmount = reader.Decimal(i++) ?? 0m,
                    PrepaymentVatCategoryTaxableAmount = reader.Decimal(i++) ?? 0m,
                    ItemName = reader.String(i++),
                    ItemBuyerIdentifier = reader.String(i++),
                    ItemSellerIdentifier = reader.String(i++),
                    ItemStandardIdentifier = reader.String(i++),
                    ItemNetPrice = reader.Decimal(i++) ?? 0m,
                    ItemVatCategory = reader.String(i++), // E, S, Z, O
                    ItemVatRate = reader.Decimal(i++) ?? 0m,
                    PrepaymentVatCategory = reader.String(i++), // E, S, Z, O
                    PrepaymentVatRate = reader.Decimal(i++) ?? 0m,
                    ItemPriceBaseQuantity = reader.Decimal(i++) ?? 0m,
                    ItemPriceBaseQuantityUnit = reader.String(i++),
                    ItemPriceDiscount = reader.Decimal(i++) ?? 0m,
                    ItemGrossPrice = reader.Decimal(i++) ?? 0m,
                });
            }

            return result;
        }
    }
}
