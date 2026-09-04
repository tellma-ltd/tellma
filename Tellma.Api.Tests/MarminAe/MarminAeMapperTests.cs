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
    /// other, a due date with no source column. The mapper is a pure static class precisely so all
    /// of that can be tested here without a database, an HTTP client or a tenant.
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

        private static MarminAeInvoice Invoice(string type = "SalesInvoice") => new()
        {
            Id = 42,
            DocumentType = type,
            TypeCode = type == "SalesInvoice" ? "380" : "381",
            DocumentNumber = "INV-1042",
            IssueDate = new DateTime(2026, 9, 3),
            DueDate = new DateTime(2026, 10, 3),
            ProfileExecutionId = "01000000",
            DocumentCurrencyCode = "AED",
            CustomerName = "Al Noor Trading LLC",
            CustomerEmail = "ap@alnoor.example",
            CustomerEndpointId = "100123456700003",
            CustomerEndpointSchemeId = "0235",
            CustomerTin = "100123456700003",
            CustomerCityName = "Dubai",
            CustomerCountrySubentity = "DXB",
            CustomerCountryCode = "AE",
            DiscrepancyResponse = type == "SalesCreditNote" ? "1" : null,
            BillingReferenceId = type == "SalesCreditNote" ? "INV-1042" : null,
            BillingReferenceIssueDate = type == "SalesCreditNote" ? new DateTime(2026, 9, 3) : null,
            Lines = [Line()],
        };

        [Fact]
        public void SalesInvoice_MapsTheRequiredFields()
        {
            var request = MarminAeMapper.ToSalesInvoice(Invoice());

            Assert.Equal("380", request.InvoiceTypeCode);
            Assert.Equal(new DateOnly(2026, 9, 3), request.IssueDate);
            Assert.Equal(new DateOnly(2026, 10, 3), request.DueDate);
            Assert.Equal("INV-1042", request.DocumentNumber);
            Assert.Equal("01000000", request.ProfileExecutionId);
            Assert.Equal("AED", request.DocumentCurrencyCode);

            var party = request.AccountingCustomerParty;
            Assert.Equal("Al Noor Trading LLC", party.Name);
            Assert.Equal("ap@alnoor.example", party.Email);
            Assert.Equal("100123456700003", party.EndpointId);
            Assert.Equal("0235", party.EndpointSchemeId);
            Assert.Equal("DXB", party.PostalAddress.CountrySubentity);
        }

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
        public void CreditNote_CarriesTheDiscrepancyResponseAndBillingReference()
        {
            var request = MarminAeMapper.ToSalesCreditNote(Invoice("SalesCreditNote"));

            Assert.Equal("381", request.CreditNoteTypeCode);
            Assert.Equal("1", request.DiscrepancyResponse);

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
        public void AMissingRequiredField_FailsWithTheFieldName()
        {
            var invoice = Invoice();
            invoice.CustomerEmail = null;

            var ex = Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesInvoice(invoice));
            Assert.Contains(nameof(MarminAeInvoice.CustomerEmail), ex.Message);
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
        public void AZeroRoundingAmount_IsOmittedEntirely()
        {
            var invoice = Invoice();
            invoice.PayableRoundingAmount = 0m;
            Assert.Null(MarminAeMapper.ToSalesInvoice(invoice).PayableRoundingAmount);

            invoice.PayableRoundingAmount = 0.01m;
            Assert.Equal(0.01m, MarminAeMapper.ToSalesInvoice(invoice).PayableRoundingAmount);
        }

        [Fact]
        public void PaymentMeans_AreOmittedWhenNoCodeIsConfigured()
        {
            // An empty array would be worse than absence: the vendor validates the contents.
            var invoice = Invoice();
            Assert.Null(MarminAeMapper.ToSalesInvoice(invoice).PaymentMeans);

            invoice.PaymentMeansCode = "30";
            invoice.PayeeFinancialAccountId = "AE070331234567890123456";
            var means = Assert.Single(MarminAeMapper.ToSalesInvoice(invoice).PaymentMeans);
            Assert.Equal("30", means.PaymentMeansCode);
            Assert.Equal("AE070331234567890123456", means.PayeeFinancialAccount.Id);
        }

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
        [InlineData(MarminAePeppolStatus.Rejected, MarminAeState.PeppolRejected)]
        [InlineData(MarminAePeppolStatus.ValidationFailed, MarminAeState.PeppolRejected)]
        [InlineData(MarminAePeppolStatus.Pending, MarminAeState.Submitted)]
        public void PeppolStatus_MapsToTheStoredState(string status, MarminAeState expected)
        {
            Assert.Equal(expected, MarminAeMapper.ToState(status));
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

        [Fact]
        public void ADocumentWithNoLines_IsRejected()
        {
            var invoice = Invoice();
            invoice.Lines = new List<MarminAeInvoiceLine>();

            Assert.Throws<ArgumentException>(() => MarminAeMapper.ToSalesInvoice(invoice));
        }
    }
}
