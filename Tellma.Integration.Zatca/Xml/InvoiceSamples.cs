namespace Tellma.Integration.Zatca
{
    public static class InvoiceSamples
    {
        public static IEnumerable<Invoice> CreateComplianceInvoices(Party seller)
        {
            yield return StandardInvoice(seller);
            yield return StandardCreditNote(seller);
            yield return StandardDebitNote(seller);
            yield return SimplifiedInvoice(seller);
            yield return SimplifiedCreditNote(seller);
            yield return SimplifiedDebitNote(seller);
        }

        public static Invoice StandardInvoice(Party seller)
        {
            var result = Template(seller);
            result.InvoiceNumber = "SIMPLIFIED-INVOICE";
            result.InvoiceType = InvoiceType.TaxInvoice;
            result.InvoiceTypeTransactions = InvoiceTransaction.Standard;

            return result;
        }

        public static Invoice StandardCreditNote(Party seller)
        {
            var result = Template(seller);
            result.InvoiceNumber = "STANDARD-CREDIT-NOTE";
            result.InvoiceType = InvoiceType.CreditNote;
            result.InvoiceTypeTransactions = InvoiceTransaction.Standard;

            result.BillingReferenceId = "STANDARD-INVOICE";
            result.ReasonsForIssuanceOfCreditDebitNote = ["goods or services refund"];

            return result;
        }

        public static Invoice StandardDebitNote(Party seller)
        {
            var result = Template(seller);
            result.InvoiceNumber = "STANDARD-DEBIT-NOTE";
            result.InvoiceType = InvoiceType.DebitNote;
            result.InvoiceTypeTransactions = InvoiceTransaction.Standard;

            result.BillingReferenceId = "STANDARD-INVOICE";
            result.ReasonsForIssuanceOfCreditDebitNote = ["goods or services refund"];

            return result;
        }

        public static Invoice SimplifiedInvoice(Party seller)
        {
            var result = Template(seller);
            result.InvoiceNumber = "SIMPLIFIED-INVOICE";
            result.InvoiceType = InvoiceType.TaxInvoice;
            result.InvoiceTypeTransactions = InvoiceTransaction.Simplified;

            result.Buyer = null;
            return result;
        }

        public static Invoice SimplifiedCreditNote(Party seller)
        {
            var result = Template(seller);
            result.InvoiceNumber = "SIMPLIFIED-CREDIT-NOTE";
            result.InvoiceType = InvoiceType.CreditNote;
            result.InvoiceTypeTransactions = InvoiceTransaction.Simplified;

            result.Buyer = null;
            result.BillingReferenceId = "STANDARD-INVOICE";
            result.ReasonsForIssuanceOfCreditDebitNote = ["goods or services refund"];
            
            return result;
        }

        public static Invoice SimplifiedDebitNote(Party seller)
        {
            var result = Template(seller);
            result.InvoiceNumber = "SIMPLIFIED-DEBIT-NOTE";
            result.InvoiceType = InvoiceType.DebitNote;
            result.InvoiceTypeTransactions = InvoiceTransaction.Simplified;

            result.Buyer = null;
            result.BillingReferenceId = "STANDARD-INVOICE";
            result.ReasonsForIssuanceOfCreditDebitNote = ["goods or services refund"];

            return result;
        }

        private static Invoice Template(Party seller) => new()
        {
            UniqueInvoiceIdentifier = Guid.NewGuid(),
            InvoiceIssueDateTime = DateTimeOffset.UtcNow,
            InvoiceNotes = ["Some notes"],
            InvoiceCurrency = "SAR",
            InvoiceCounterValue = 1,
            PreviousInvoiceHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==",
            Seller = seller,
            Buyer = new Party
            {
                Id = new(PartyIdScheme.NationalId, "2345"),
                Address = new Address
                {
                    Street = "Main St",
                    AdditionalStreet = null,
                    BuildingNumber = "820",
                    AdditionalNumber = null,
                    District = "Al Ghat",
                    City = "Riyadh",
                    PostalCode = "34534",
                    Province = null,
                    CountryCode = "SA",
                },
                VatNumber = null,
                Name = "Contoso"
            },
            SupplyDate = DateTime.Today,
            SupplyEndDate = DateTime.Today,
            AllowanceCharges = [],
            InvoiceTotalVatAmountInAccountingCurrency = 100m,
            Lines = [new InvoiceLine()
            {
                Identifier = 1,
                Quantity = 1m,
                QuantityUnit = null,
                NetAmount = 100,
                AllowanceCharge = null,
                VatAmount = 15,
                ItemName = "My Shiny Product",
                ItemNetPrice = 100,
                ItemVatCategory = VatCategory.StandardRate,
                ItemVatRate = 0.15m,
            }],
        };
    }
}
