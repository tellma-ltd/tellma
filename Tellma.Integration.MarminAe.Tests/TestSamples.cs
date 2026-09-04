// Copyright (c) Tellma Ltd. All rights reserved.

namespace Tellma.Integration.MarminAe.Tests
{
    /// <summary>Request fixtures shared by the serialization and client suites.</summary>
    internal static class TestSamples
    {
        /// <summary>A UAE customer with exactly the five party fields the vendor requires.</summary>
        internal static MarminAePartyRequest Customer { get; } = new()
        {
            Name = "Al Noor Trading LLC",
            Email = "ap@alnoor.example",
            EndpointId = "100123456700003",
            EndpointSchemeId = "0235",
            Tin = "100123456700003",
            PostalAddress = new MarminAeAddress
            {
                StreetName = "Sheikh Zayed Road",
                CityName = "Dubai",
                CountrySubentity = "DXB",
                Country = "United Arab Emirates",
                CountryCode = "AE",
            },
        };

        /// <summary>One standard-rated line at the UAE default of 5%.</summary>
        internal static MarminAeDocumentLineRequest Line { get; } = new()
        {
            Name = "Consulting",
            Description = "Implementation consulting, October",
            Quantity = 3m,
            UnitCode = "HUR",
            Price = new MarminAePriceRequest { BaseAmount = 500m, BaseQuantity = 1m },

            // Percent is a percentage here, not the 0..1 fraction ZATCA uses.
            ClassifiedTaxCategory = new MarminAeTaxCategory
            {
                Id = "S",
                Percent = 5m,
                TaxScheme = "VAT",
            },
        };

        internal static MarminAeSalesInvoiceRequest Invoice { get; } = new()
        {
            IssueDate = new DateOnly(2026, 9, 3),
            DueDate = new DateOnly(2026, 10, 3),
            InvoiceTypeCode = "380",
            ProfileExecutionId = "01000000",
            DocumentCurrencyCode = "AED",
            DocumentNumber = "INV-1042",
            AccountingCustomerParty = Customer,
            DocumentLines = [Line],
        };

        internal static MarminAeSalesCreditNoteRequest CreditNote { get; } = new()
        {
            IssueDate = new DateOnly(2026, 9, 10),
            CreditNoteTypeCode = "381",
            DiscrepancyResponse = "1",
            ProfileExecutionId = "01000000",
            DocumentCurrencyCode = "AED",
            DocumentNumber = "CRN-77",
            AccountingCustomerParty = Customer,
            DocumentLines = [Line],
            BillingReference = [new MarminAeDocumentReference
            {
                Id = "INV-1042",
                IssueDate = new DateOnly(2026, 9, 3),
            }],
        };
    }
}
