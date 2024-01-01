namespace Tellma.Integration.Zatca.Tests
{
    internal static class InvoicesSamples
    {
        /// <summary>
        /// Create a valid Standard <see cref="Invoice"/>.
        /// </summary>
        internal static Invoice ValidStandardInvoice() => new()
        {
            UniqueInvoiceIdentifier = Guid.Parse("16e78469-64af-406d-9cfd-895e724198f0"),
            InvoiceNumber = "SME00062",
            InvoiceType = InvoiceType.TaxInvoice,
            InvoiceIssueDateTime = new DateTime(2022, 3, 13, 14, 40, 40),
            InvoiceTypeTransactions = InvoiceTransaction.Standard | InvoiceTransaction.ThirdParty | InvoiceTransaction.Nominal | InvoiceTransaction.Summary,
            InvoiceCurrency = "SAR",
            InvoiceCounterValue = "62",
            PreviousInvoiceHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==",
            Seller = new Party
            {
                Id = new(PartyIdScheme.CommercialRegistration, "454634645645654"),
                Address = new Address
                {
                    Street = "test",
                    AdditionalStreet = null,
                    BuildingNumber = "3454",
                    AdditionalNumber = "1234",
                    District = "test",
                    City = "Riyadh",
                    PostalCode = "12345",
                    Province = "test",
                    CountryCode = "SA",
                },
                VatNumber = "300075588700003",
                Name = "Ahmed Mohamed AL Ahmady"
            },
            Buyer = new Party
            {
                Id = new(PartyIdScheme.NationalId, "2345"),
                Address = new Address
                {
                    Street = "baaoun",
                    AdditionalStreet = "sdsd",
                    BuildingNumber = "3353",
                    AdditionalNumber = "3434",
                    District = "fgff",
                    City = "Dhurma",
                    PostalCode = "34534",
                    Province = "ulhk",
                    CountryCode = "SA",
                },
                VatNumber = null,
                Name = "sdsa"
            },
            SupplyDate = new DateTime(2022, 3, 13),
            SupplyEndDate = new DateTime(2022, 3, 15),
            PaymentMeans = PaymentMeans.InCash,
            AllowanceCharge = new AllowanceCharge
            {
                Indicator = AllowanceChargeType.Allowance,
                Reason = "discount",
                Amount = 2m,
                VatCategory = VatCategory.StandardRate,
                VatRate = 0.15m
            },
            VatCategoryTaxableAmount = 966.00m,
            VatCategory = VatCategory.StandardRate,
            VatRate = 0.1500m,
            InvoiceTotalVatAmountInAccountingCurrency = 144.9m,
            PrepaidAmount = 0.00m,

            Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Identifier = "1",
                        Quantity = 44.0m,
                        QuantityUnit = "PCE",

                        NetAmount = 968.00m,
                        VatAmount = 145.20m,
                        AmountIncludingVat = 1113.20m,

                        ItemName = "dsd",
                        ItemVatCategory = VatCategory.StandardRate,
                        ItemVatRate = 0.15m,

                        ItemNetPrice = 22.00m,
                        ItemPriceDiscount = 2.00m,
                    }
                }
        };

        /// <summary>
        /// Create an <see cref="Invoice"/> that has every field populated.
        /// </summary>
        internal static Invoice FullyPopulatedInvoice() => new()
        {
            UniqueInvoiceIdentifier = Guid.Parse("16e78469-64af-406d-9cfd-895e724198f0"),
            InvoiceNumber = "SME00062",
            InvoiceType = InvoiceType.TaxInvoice,
            InvoiceIssueDateTime = new DateTime(2022, 3, 13, 14, 40, 40),
            InvoiceTypeTransactions = InvoiceTransaction.Simplified | InvoiceTransaction.Nominal | InvoiceTransaction.Exports | InvoiceTransaction.Summary,
            InvoiceCurrency = "SAR",
            InvoiceCounterValue = "62",
            PreviousInvoiceHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==",
            Seller = new Party
            {
                Id = new(PartyIdScheme.CommercialRegistration, "454634645645654"),
                Address = new Address
                {
                    Street = "test",
                    AdditionalStreet = null,
                    BuildingNumber = "3454",
                    AdditionalNumber = "1234",
                    District = "test",
                    City = "Riyadh",
                    PostalCode = "12345",
                    Province = "test",
                    CountryCode = "SA",
                },
                VatNumber = "300075588700003",
                Name = "Ahmed Mohamed AL Ahmady",
            },
            Buyer = new Party
            {
                Id = new(PartyIdScheme.NationalId, "2345"),
                Address = new Address
                {
                    Street = "baaoun",
                    AdditionalStreet = "sdsd",
                    BuildingNumber = "3353",
                    AdditionalNumber = "3434",
                    District = "fgff",
                    City = "Dhurma",
                    PostalCode = "34534",
                    Province = "ulhk",
                    CountryCode = "SA",
                },
                VatNumber = null,
                Name = "sdsa"
            },

            BillingReferenceId = "billing-reference-id",
            ContractId = "12345",
            InvoiceNotes = new List<string> { "Note 1", "Note 2" },
            InvoiceTotalVatAmountInAccountingCurrency = 968.0m,
            AccountingCurrency = "SAR",

            PaymentAccountId = "12345",
            PaymentTerms = "payment-terms",
            PurchaseOrderId = "12345",
            ReasonsForIssuanceOfCreditDebitNote = new List<string> { "Reason 1", "Reason 2" },
            RoundingAmount = 0.21m,
            VatExemptionReason = "vat-exemption-reason",
            VatExemptionReasonCode = "vat-exemption-reason-code",

            SupplyDate = new DateTime(2022, 3, 13),
            SupplyEndDate = new DateTime(2022, 3, 15),
            PaymentMeans = PaymentMeans.InCash,
            AllowanceCharge = new AllowanceCharge
            {
                Indicator = AllowanceChargeType.Allowance,
                Reason = "discount",
                Amount = 2m,
                VatCategory = VatCategory.StandardRate,
                VatRate = 0.15m,
                BaseAmount = 15m,
                Percentage = 0.15m,
                ReasonCode = "99"
            },
            VatCategoryTaxableAmount = 966.00m,
            VatCategory = VatCategory.StandardRate,
            VatRate = 0.1500m,

            PrepaidAmount = 0.00m,

            Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Identifier = "1",
                        Quantity = 44.0m,
                        QuantityUnit = "PCE",

                        NetAmount = 968.00m,
                        VatAmount = 145.20m,
                        AmountIncludingVat = 1113.20m,

                        ItemName = "dsd",
                        ItemVatCategory = VatCategory.StandardRate,
                        ItemVatRate = 0.15m,

                        ItemNetPrice = 22.00m,
                        ItemPriceDiscount = 2.00m,

                        AllowanceCharge = new LineAllowanceCharge
                        {
                            Amount = 968.00m,
                            BaseAmount = 145.20m,
                            Indicator = AllowanceChargeType.Allowance,
                            Percentage = 0.05m,
                            Reason = "line discount reason",
                            ReasonCode = "99"
                        },
                        ItemBuyerIdentifier = "1",
                        ItemGrossPrice= 2.00m,
                        ItemPriceBaseQuantity = 34.0m,
                        ItemPriceBaseQuantityUnit = "PCE",

                        ItemSellerIdentifier = "1",
                        ItemStandardIdentifier = "2",
                        PrepaymentId= "1",
                        PrepaymentIssueDateTime = new DateTime(2023,12,20),
                        PrepaymentUuid = Guid.Parse("26e78469-64af-406d-9cfd-895e724198f0"),
                        PrepaymentVatCategory= VatCategory.StandardRate,
                        PrepaymentVatCategoryTaxableAmount= 600.0m,
                        PrepaymentVatCategoryTaxAmount = 120.0m,
                        PrepaymentVatRate = 0.15m
                    },

                    new InvoiceLine
                    {
                        Identifier = "2",
                        Quantity = 54.0m,
                        QuantityUnit = "PCE",

                        NetAmount = 1968.00m,
                        VatAmount = 1145.20m,
                        AmountIncludingVat = 1113.20m,

                        ItemName = "dsds",
                        ItemVatCategory = VatCategory.StandardRate,
                        ItemVatRate = 0.15m,

                        ItemNetPrice = 122.00m,
                        ItemPriceDiscount = 12.00m,

                        AllowanceCharge = new LineAllowanceCharge
                        {
                            Amount = 1968.00m,
                            BaseAmount = 1145.20m,
                            Indicator = AllowanceChargeType.Allowance,
                            Percentage = 0.05m,
                            Reason = "line 2 discount reason",
                            ReasonCode = "99"
                        },
                        ItemBuyerIdentifier = "2",
                        ItemGrossPrice= 12.00m,
                        ItemPriceBaseQuantity = 134.0m,
                        ItemPriceBaseQuantityUnit = "PCE",

                        ItemSellerIdentifier = "11",
                        ItemStandardIdentifier = "12",
                        PrepaymentId= "11",
                        PrepaymentIssueDateTime= new DateTime(2023,12,20),
                        PrepaymentUuid = Guid.Parse("36e78469-64af-406d-9cfd-895e724198f0"),
                        PrepaymentVatCategory= VatCategory.StandardRate,
                        PrepaymentVatCategoryTaxableAmount= 1600.0m,
                        PrepaymentVatCategoryTaxAmount = 1120.0m,
                        PrepaymentVatRate = 0.15m
                    }
                }
        };
    }
}
