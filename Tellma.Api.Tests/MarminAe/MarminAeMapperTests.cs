using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Api.MarminAe;
using Tellma.Connector.MarminAe;
using Tellma.Repository.Application;
using Xunit;

namespace Tellma.Api.Tests.MarminAe
{
    /// <summary>
    /// Covers <see cref="MarminAeMapper"/>, the translation from the rows
    /// <c>dal.MarminAe__GetInvoices</c> returns into the vendor's request models.
    /// </summary>
    /// <remarks>
    /// This is where the conversions that are easy to get quietly wrong live -- the sign of a
    /// credit note's quantities, a VAT rate that is a fraction on one side and a percentage on the
    /// other, a TIN that is the first ten digits of a TRN. The mapper is a pure static class
    /// precisely so all of that can be tested here without a database, an HTTP client or a tenant.
    /// </remarks>
    public class MarminAeMapperTests
    {
        private static MarminAeInvoiceLine Line() => new()
        {
            LineNumber = 1,
            Name = "Consulting",
            Description = "Implementation consulting",
            Quantity = 3m,
            UnitCode = "HUR",
            PriceBaseAmount = 500m,
            PriceBaseQuantity = 1m,
            TaxCategoryId = "S",

            // As the SQL emits it: Tellma stores 0.05, the SP multiplies by 100.
            TaxPercent = 5m,
        };

        /// <summary>
        /// A row exactly as dal.MarminAe__GetInvoices emits it for a Peppol-registered UAE customer
        /// whose stored tax number is the 15-digit TRN.
        /// </summary>
        private static MarminAeInvoice Invoice(string type = "SalesInvoice") => new()
        {
            Id = 42,
            DocumentType = type,
            TypeCode = type == "SalesInvoice" ? "380" : "381",
            DocumentNumber = "INV-1042",
            IssueDate = new DateTime(2026, 9, 3),
            DueDate = new DateTime(2026, 10, 3),
            ProfileExecutionId = "00000000",
            DocumentCurrencyCode = "AED",
            CustomerName = "Al Noor Trading LLC",
            CustomerEmail = "ap@alnoor.example",
            CustomerEndpointId = "1001234567",
            CustomerEndpointSchemeId = "0235",
            CustomerTin = "1001234567",
            CustomerTrn = "100123456700003",
            CustomerStreetName = "Sheikh Zayed Road",
            CustomerCityName = "Dubai",
            CustomerCountrySubentity = "DXB",
            CustomerCountry = "United Arab Emirates",
            CustomerCountryCode = "AE",
            PaymentMeansCode = "30",
            PayeeFinancialAccountId = "AE070331234567890123456",
            DiscrepancyResponse = type == "SalesCreditNote" ? "DL8.61.1.A" : null,
            BillingReferenceId = type == "SalesCreditNote" ? "INV-1042" : null,
            BillingReferenceIssueDate = type == "SalesCreditNote" ? new DateTime(2026, 9, 3) : null,
            Lines = [Line()],
        };

        #region Header

        [Fact]
        public void SalesInvoice_MapsTheRequiredFields()
        {
            var request = MarminAeMapper.ToSalesInvoice(Invoice());

            Assert.Equal("380", request.InvoiceTypeCode);
            Assert.Equal(new DateOnly(2026, 9, 3), request.IssueDate);
            Assert.Equal(new DateOnly(2026, 10, 3), request.DueDate);
            Assert.Equal("INV-1042", request.DocumentNumber);
            Assert.Equal("00000000", request.ProfileExecutionId);
            Assert.Equal("AED", request.DocumentCurrencyCode);

            var party = request.AccountingCustomerParty;
            Assert.Equal("Al Noor Trading LLC", party.Name);
            Assert.Equal("ap@alnoor.example", party.Email);
            Assert.Equal("1001234567", party.EndpointId);
            Assert.Equal("0235", party.EndpointSchemeId);

            var address = party.PostalAddress;
            Assert.Equal("Sheikh Zayed Road", address.StreetName);
            Assert.Equal("Dubai", address.CityName);
            Assert.Equal("DXB", address.CountrySubentity);
            Assert.Equal("United Arab Emirates", address.Country);
            Assert.Equal("AE", address.CountryCode);
        }

        [Fact]
        public void CreditNote_CarriesTheDiscrepancyResponseAndBillingReference()
        {
            var request = MarminAeMapper.ToSalesCreditNote(Invoice("SalesCreditNote"));

            Assert.Equal("381", request.CreditNoteTypeCode);
            Assert.Equal("DL8.61.1.A", request.DiscrepancyResponse);

            // The vendor requires at least one, naming the invoice being adjusted.
            var reference = Assert.Single(request.BillingReference);
            Assert.Equal("INV-1042", reference.Id);
            Assert.Equal(new DateOnly(2026, 9, 3), reference.IssueDate);
        }

        [Fact]
        public void CreditNote_WithoutAnOriginalInvoice_IsRejected()
        {
            // bll.Documents_Validate__Close blocks this at the close, so reaching the mapper means
            // a validation gap. Failing loudly here is better than sending an empty reference.
            var invoice = Invoice("SalesCreditNote");
            invoice.BillingReferenceId = null;

            Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesCreditNote(invoice));
        }

        [Fact]
        public void AZeroRoundingAmount_IsOmittedEntirely()
        {
            var invoice = Invoice();
            invoice.PayableRoundingAmount = 0m;
            Assert.Null(MarminAeMapper.ToSalesInvoice(invoice).PayableRoundingAmount);

            invoice.PayableRoundingAmount = 0.01m;
            Assert.Equal(0.01m, MarminAeMapper.ToSalesInvoice(invoice).PayableRoundingAmount);
        }

        [Fact]
        public void PaymentMeans_AreAlwaysSent()
        {
            // The vendor requires a payment instruction unless the document is a deemed supply,
            // and the SQL defaults the code to 30, so there is always exactly one.
            var means = Assert.Single(MarminAeMapper.ToSalesInvoice(Invoice()).PaymentMeans);

            Assert.Equal("30", means.PaymentMeansCode);
            Assert.Equal("AE070331234567890123456", means.PayeeFinancialAccount.Id);
        }

        [Fact]
        public void ACreditTransferWithoutAPayeeAccount_IsRejected()
        {
            // IBR-192-AE, confirmed against the sandbox: the vendor refuses a credit transfer that
            // does not say which account to pay into.
            var invoice = Invoice();
            invoice.PayeeFinancialAccountId = " ";

            var ex = Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesInvoice(invoice));
            Assert.Contains(nameof(MarminAeInvoice.PayeeFinancialAccountId), ex.Message);
        }

        [Fact]
        public void OtherPaymentMeans_DoNotNeedAPayeeAccount()
        {
            var invoice = Invoice();
            invoice.PaymentMeansCode = "10"; // in cash
            invoice.PayeeFinancialAccountId = null;

            var means = Assert.Single(MarminAeMapper.ToSalesInvoice(invoice).PaymentMeans);

            Assert.Equal("10", means.PaymentMeansCode);
            Assert.Null(means.PayeeFinancialAccount);
        }

        [Fact]
        public void AMissingPaymentMeansCode_IsRejected()
        {
            var invoice = Invoice();
            invoice.PaymentMeansCode = null;

            Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesInvoice(invoice));
        }

        #endregion

        #region Customer tax identity

        [Fact]
        public void AUaeCustomersTrn_TravelsInPartyTaxScheme_SeparatelyFromTheTin()
        {
            // The 10-digit TIN is the tin and the endpoint; the 15-digit TRN is the buyer's VAT
            // registration (UAE VAT ER Art. 59(1)(c), MoF field #24). Sending the TRN as the tin,
            // as the first version of this integration did, fails Peppol IBR-148 / IBR-135.
            var party = MarminAeMapper.ToSalesInvoice(Invoice()).AccountingCustomerParty;

            Assert.Equal("1001234567", party.Tin);
            Assert.Equal("100123456700003", party.PartyTaxScheme.CompanyId);
            Assert.Equal("VAT", party.PartyTaxScheme.TaxScheme);
        }

        [Fact]
        public void ACustomerWithOnlyATin_SendsNoPartyTaxScheme()
        {
            // If either party_tax_scheme field is sent, both are required, with a valid TRN.
            var invoice = Invoice();
            invoice.CustomerTrn = null;

            var party = MarminAeMapper.ToSalesInvoice(invoice).AccountingCustomerParty;

            Assert.Equal("1001234567", party.Tin);
            Assert.Null(party.PartyTaxScheme);
        }

        /// <summary>
        /// A row as the SQL emits it for a customer outside the UAE who is not on Peppol: no tin
        /// or TRN, the FTA's export placeholder endpoint, and the export flag set by default.
        /// </summary>
        private static MarminAeInvoice ForeignInvoice(string type = "SalesInvoice")
        {
            var invoice = Invoice(type);
            invoice.CustomerTin = null;
            invoice.CustomerTrn = null;
            invoice.CustomerEndpointId = "9900000099";
            invoice.ProfileExecutionId = "00000001";
            invoice.CustomerStreetName = "Olaya Street";
            invoice.CustomerCityName = "Riyadh";
            invoice.CustomerCountrySubentity = "RUH";
            invoice.CustomerCountry = "Kingdom of Saudi Arabia";
            invoice.CustomerCountryCode = "SA";
            return invoice;
        }

        [Fact]
        public void AForeignCustomer_SendsNeitherTinNorPartyTaxScheme()
        {
            // The vendor rejects both on a non-UAE B2B party. The SQL leaves them null for one, and
            // routes the document to the FTA placeholder endpoint for an export.
            var party = MarminAeMapper.ToSalesInvoice(ForeignInvoice()).AccountingCustomerParty;

            Assert.Null(party.Tin);
            Assert.Null(party.PartyTaxScheme);
            Assert.Equal("9900000099", party.EndpointId);
            Assert.Equal("0235", party.EndpointSchemeId);
        }


        [Fact]
        public void AMissingEndpoint_IsRejected()
        {
            // Since 2026-07-07 the vendor no longer defaults the endpoint, so one must always be sent.
            var invoice = Invoice();
            invoice.CustomerEndpointId = null;

            Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesInvoice(invoice));
        }

        [Theory]
        [InlineData(nameof(MarminAeInvoice.CustomerStreetName))]
        [InlineData(nameof(MarminAeInvoice.CustomerCityName))]
        [InlineData(nameof(MarminAeInvoice.CustomerCountrySubentity))]
        [InlineData(nameof(MarminAeInvoice.CustomerCountry))]
        [InlineData(nameof(MarminAeInvoice.CustomerCountryCode))]
        [InlineData(nameof(MarminAeInvoice.CustomerEmail))]
        public void AMissingRequiredCustomerField_FailsWithTheFieldName(string field)
        {
            // The vendor requires every one of these on every customer, UAE or not.
            var invoice = Invoice();
            typeof(MarminAeInvoice).GetProperty(field).SetValue(invoice, null);

            var ex = Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesInvoice(invoice));
            Assert.Contains(field, ex.Message);
        }

        #endregion

        #region Exports

        [Theory]
        [InlineData("SalesInvoice")]
        [InlineData("SalesCreditNote")]
        public void AnExport_IsDeliveredToTheCustomersAddress(string type)
        {
            // Confirmed against the sandbox: an export without a delivery block is refused
            // ("Delivery information is required for export documents"), and one delivered to the
            // customer's address passes Peppol validation.
            MarminAeSalesDocumentRequest request = type == "SalesInvoice"
                ? MarminAeMapper.ToSalesInvoice(ForeignInvoice(type))
                : MarminAeMapper.ToSalesCreditNote(ForeignInvoice(type));

            var address = request.Delivery.DeliveryLocation.Address;
            Assert.Equal(request.AccountingCustomerParty.PostalAddress, address);
            Assert.Equal("SA", address.CountryCode);

            // Tellma does not know when the goods arrived, so it does not say.
            Assert.Null(request.Delivery.ActualDeliveryDate);
        }

        [Fact]
        public void ADomesticSupply_HasNoDeliveryBlock()
        {
            Assert.Null(MarminAeMapper.ToSalesInvoice(Invoice()).Delivery);
        }

        [Theory]
        [InlineData("00000001", true)]
        [InlineData("00001001", true)]   // e.g. a continuous supply that is also an export
        [InlineData("00000000", false)]
        [InlineData("10000000", false)]  // position 1 is the free trade zone, not exports
        [InlineData("0000001", false)]   // malformed: refused at close, never an export here
        [InlineData(null, false)]
        public void IsExport_ReadsTheEighthFlag(string profileExecutionId, bool expected)
        {
            Assert.Equal(expected, MarminAeMapper.IsExport(profileExecutionId));
        }

        #endregion

        #region Lines

        [Fact]
        public void TaxPercent_IsSentAsAPercentage_NotAFraction()
        {
            // The trap: ZATCA wants 0.05 for 5%, Marmin wants 5. The SQL does the multiplication,
            // and the mapper must pass it through untouched rather than "helpfully" converting.
            var request = MarminAeMapper.ToSalesInvoice(Invoice());

            Assert.Equal(5m, request.DocumentLines.Single().ClassifiedTaxCategory.Percent);
            Assert.Equal("VAT", request.DocumentLines.Single().ClassifiedTaxCategory.TaxScheme);
        }

        [Fact]
        public void ExemptLines_CarryTheirExemptionReason()
        {
            // The vendor rejects an exempt line with no reason. ZATCA left these fields unmapped,
            // so this is one of the places the two integrations genuinely differ.
            var invoice = Invoice();
            invoice.Lines[0].TaxCategoryId = "E";
            invoice.Lines[0].TaxPercent = 0m;
            invoice.Lines[0].TaxExemptionReasonCode = "VATEX-AE-EXEMPT";
            invoice.Lines[0].TaxExemptionReason = "Exempt financial service";

            var category = MarminAeMapper.ToSalesInvoice(invoice).DocumentLines.Single().ClassifiedTaxCategory;

            Assert.Equal("E", category.Id);
            Assert.Equal("VATEX-AE-EXEMPT", category.TaxExemptionReasonCode);
            Assert.Equal("Exempt financial service", category.TaxExemptionReason);
        }

        [Fact]
        public void StandardItemIdentification_IsNeverSent()
        {
            // Resources.Identifier is free text with no ISO 6523 scheme, and PINT-AE IBR-064
            // rejects an identifier without one. The vendor's API does not check it, so sending it
            // would only fail at Peppol, after the close.
            var line = MarminAeMapper.ToSalesInvoice(Invoice()).DocumentLines.Single();

            Assert.Null(line.StandardItemIdentification);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void ANonPositiveQuantity_IsRejected(int quantity)
        {
            var invoice = Invoice();
            invoice.Lines[0].Quantity = quantity;

            Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesInvoice(invoice));
        }

        [Fact]
        public void AZeroBaseQuantity_IsCorrectedToOne()
        {
            // The vendor divides by base_quantity, so a zero would be worse than a wrong price.
            var invoice = Invoice();
            invoice.Lines[0].PriceBaseQuantity = 0m;

            Assert.Equal(1m, MarminAeMapper.ToSalesInvoice(invoice).DocumentLines.Single().Price.BaseQuantity);
        }

        [Fact]
        public void ADocumentWithNoLines_IsRejected()
        {
            var invoice = Invoice();
            invoice.Lines = new List<MarminAeInvoiceLine>();

            Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesInvoice(invoice));
        }

        #endregion

        #region Prepare

        [Fact]
        public void Prepare_ANewDocument_IsACreate()
        {
            var prepared = MarminAeMapper.Prepare(Invoice());

            Assert.Equal(MarminAeDocumentKind.SalesInvoice, prepared.Kind);
            Assert.False(prepared.IsResubmission);
            Assert.NotNull(prepared.Invoice);
            Assert.Null(prepared.CreditNote);
            Assert.Equal(42, prepared.DocumentId);
            Assert.Equal("INV-1042", prepared.DocumentNumber);
        }

        [Fact]
        public void Prepare_ADocumentTheVendorAlreadyHolds_IsAResubmission()
        {
            // The vendor's id is recorded only once it has accepted the document, so its presence
            // means a POST would be refused as a duplicate number: it must be a PUT to that id.
            var invoice = Invoice("SalesCreditNote");
            invoice.MarminAeDocumentId = "doc-9001";

            var prepared = MarminAeMapper.Prepare(invoice);

            Assert.Equal(MarminAeDocumentKind.SalesCreditNote, prepared.Kind);
            Assert.True(prepared.IsResubmission);
            Assert.Equal("doc-9001", prepared.ExistingDocumentId);
            Assert.NotNull(prepared.CreditNote);
            Assert.Null(prepared.Invoice);
        }

        [Fact]
        public void Prepare_RunsTheWholeMapping()
        {
            // Prepare is the dry run DocumentsService performs before claiming a document, so it
            // must refuse everything the mapper would.
            var invoice = Invoice();
            invoice.CustomerStreetName = null;

            Assert.Throws<ArgumentException>(() => MarminAeMapper.Prepare(invoice));
        }

        #endregion

        #region Kinds and states

        [Theory]
        [InlineData("SalesInvoice", MarminAeDocumentKind.SalesInvoice)]
        [InlineData("SalesCreditNote", MarminAeDocumentKind.SalesCreditNote)]
        public void DocumentType_ParsesToTheVendorKind(string stored, MarminAeDocumentKind expected)
        {
            // The stored values match the enum member names exactly, which is what keeps the
            // definition column and the vendor client from drifting apart.
            Assert.Equal(expected, MarminAeMapper.ToKind(stored));
        }

        [Theory]
        [InlineData("PurchaseInvoice")]     // a real enum member, but not one we can author
        [InlineData("PurchaseCreditNote")]
        [InlineData("salesinvoice")]        // the stored values are case-sensitive
        [InlineData("Nonsense")]
        [InlineData(null)]
        public void ADocumentTypeThatCannotBeSubmitted_IsRejected(string documentType)
        {
            Assert.Throws<ArgumentException>(() => MarminAeMapper.ToKind(documentType));
        }

        [Theory]
        [InlineData(MarminAePeppolStatus.Approved, MarminAeState.Delivered)]
        [InlineData(MarminAePeppolStatus.ValidationFailed, MarminAeState.PeppolValidationFailed)]
        [InlineData(MarminAePeppolStatus.Rejected, MarminAeState.PeppolRejected)]
        [InlineData(MarminAePeppolStatus.Pending, MarminAeState.Submitted)]
        public void PeppolStatus_MapsToTheStoredState(string status, MarminAeState expected)
        {
            Assert.Equal(expected, MarminAeMapper.ToState(status));
        }

        [Fact]
        public void ValidationFailedAndRejected_AreDistinctStates()
        {
            // The vendor lets a VALIDATION_FAILED document be resubmitted and refuses to resubmit a
            // REJECTED one, whose remedy is a credit note. Collapsing them would either offer a
            // resubmit the vendor refuses, or hide one it allows.
            Assert.NotEqual(
                MarminAeMapper.ToState(MarminAePeppolStatus.ValidationFailed),
                MarminAeMapper.ToState(MarminAePeppolStatus.Rejected));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("SOME_NEW_STATUS")]
        public void AnUnrecognizedPeppolStatus_IsTreatedAsStillInFlight(string status)
        {
            // The vendor's status vocabulary is explicitly open. Calling an unknown value a
            // rejection would raise a false alarm about an invoice that is on the network and
            // may be perfectly fine.
            Assert.Equal(MarminAeState.Submitted, MarminAeMapper.ToState(status));
        }

        #endregion
    }

    /// <summary>
    /// Covers <see cref="MarminAeService.PickExistingDocumentId"/>, the duplicate guard behind
    /// Refresh and Resubmit: deciding whether the vendor already holds this document.
    /// </summary>
    /// <remarks>
    /// The vendor does not document whether its number filter is exact, so a wrong answer here
    /// either re-sends a live invoice (a duplicate in a counterparty's books) or attaches someone
    /// else's document to ours. Refusing to guess is the safe failure.
    /// </remarks>
    public class MarminAeFindExistingTests
    {
        private static MarminAeDocument Doc(string id, string number, string profileId = "MBP-1") => new()
        {
            Id = id,
            DocumentNumber = number,
            AccountingSupplierParty = profileId == null ? null : new MarminAeParty { ProfileId = profileId },
        };

        [Fact]
        public void NoCandidates_MeansNotFound()
        {
            Assert.Null(MarminAeService.PickExistingDocumentId([], "INV-7", "MBP-1"));
        }

        [Fact]
        public void AnExactMatch_IsFound()
        {
            Assert.Equal("a", MarminAeService.PickExistingDocumentId([Doc("a", "INV-7")], "INV-7", "MBP-1"));
        }

        [Fact]
        public void APrefixOrContainsMatch_IsIgnored()
        {
            // If the vendor's filter turns out to be a prefix or contains match, INV-7 must not
            // claim INV-70 or XINV-7 as itself.
            var candidates = new[] { Doc("a", "INV-70"), Doc("b", "XINV-7"), Doc("c", "inv-7") };

            Assert.Null(MarminAeService.PickExistingDocumentId(candidates, "INV-7", "MBP-1"));
        }

        [Fact]
        public void AnotherBusinessProfilesDocument_IsIgnored()
        {
            var candidates = new[] { Doc("a", "INV-7", profileId: "MBP-OTHER") };

            Assert.Null(MarminAeService.PickExistingDocumentId(candidates, "INV-7", "MBP-1"));
        }

        [Fact]
        public void ACandidateWithNoProfile_IsNotExcludedForIt()
        {
            // The vendor's responses leave optional fields out; absence is not a mismatch.
            var candidates = new[] { Doc("a", "INV-7", profileId: null) };

            Assert.Equal("a", MarminAeService.PickExistingDocumentId(candidates, "INV-7", "MBP-1"));
        }

        [Fact]
        public void TheSameDocumentOnTwoPages_IsOneMatch()
        {
            var candidates = new[] { Doc("a", "INV-7"), Doc("a", "INV-7") };

            Assert.Equal("a", MarminAeService.PickExistingDocumentId(candidates, "INV-7", "MBP-1"));
        }

        [Fact]
        public void TwoDistinctMatches_AreRefusedRatherThanGuessed()
        {
            var candidates = new[] { Doc("a", "INV-7"), Doc("b", "INV-7") };

            Assert.Throws<InvalidOperationException>(
                () => MarminAeService.PickExistingDocumentId(candidates, "INV-7", "MBP-1"));
        }
    }
}
