using System.Xml;

namespace Tellma.Integration.Zatca
{
    public class InvoiceValidator
    {
        #region Constants

        private static readonly HashSet<PartyIdScheme> ACCEPTABLE_SELLER_ID_SCHEMES = new()
        {
            PartyIdScheme.CommercialRegistration,
            PartyIdScheme.Momrah,
            PartyIdScheme.Mhrsd,
            PartyIdScheme.Number700,
            PartyIdScheme.Misa,
            PartyIdScheme.OtherId
        };

        private static readonly HashSet<PartyIdScheme> ACCEPTABLE_BUYER_ID_SCHEMES = new()
        {
            PartyIdScheme.TaxIdentificationNumber,
            PartyIdScheme.CommercialRegistration,
            PartyIdScheme.Momrah,
            PartyIdScheme.Mhrsd,
            PartyIdScheme.Number700,
            PartyIdScheme.Misa,
            PartyIdScheme.NationalId,
            PartyIdScheme.GccId,
            PartyIdScheme.IqamaNumber,
            PartyIdScheme.PassportId ,
            PartyIdScheme.OtherId
        };


        #endregion

        private readonly Invoice inv;

        public InvoiceValidator(Invoice invoice)
        {
            inv = invoice ?? throw new ArgumentNullException(nameof(invoice));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (string.IsNullOrWhiteSpace(inv.InvoiceNumber))
                yield return new("BR-02", "An Invoice shall have an Invoice number (BT-1).");

            if (inv.InvoiceIssueDateTime == default)
                yield return new("BR-03", "An Invoice shall have an Invoice issue date (BT-2).");

            if (inv.InvoiceType == default)
                yield return new("BR-04", "An Invoice shall have an Invoice type code (BT-3).");

            if (string.IsNullOrWhiteSpace(inv.InvoiceCurrency))
                yield return new("BR-05", "An Invoice shall have an Invoice currency code (BT-5)");

            if (string.IsNullOrWhiteSpace(inv.Seller?.Name))
                yield return new("BR-06", "An Invoice shall contain the Seller name (BT-27).");

            if (new string?[] {
                inv.Seller?.Address?.Street,
                inv.Seller?.Address?.AdditionalStreet,
                inv.Seller?.Address?.BuildingNumber,
                inv.Seller?.Address?.AdditionalNumber,
                inv.Seller?.Address?.District,
                inv.Seller?.Address?.City,
                inv.Seller?.Address?.PostalCode,
                inv.Seller?.Address?.Province,
                inv.Seller?.Address?.CountryCode }.All(string.IsNullOrWhiteSpace))
                yield return new("BR-08", "An Invoice shall contain the Seller postal address (BG-5).");

            if (string.IsNullOrWhiteSpace(inv.Seller?.Address?.CountryCode))
                yield return new("BR-09", "The Seller postal address (BG-5) shall contain a Seller country code (BT-40).");

            if (inv.InvoiceTypeTransactions.HasFlag(InvoiceTransaction.Standard) && string.IsNullOrWhiteSpace(inv.InvoiceCurrency))
                yield return new("BR-10", "An Invoice shall contain the Buyer postal address (BG-8)."); // Only applicable to standard invoices


            // TODO: All the rest


            if (string.IsNullOrWhiteSpace(inv.InvoiceCurrency))
                yield return new("BR-13", "YYYYY");

            if (string.IsNullOrWhiteSpace(inv.InvoiceCurrency))
                yield return new("BR-14", "YYYYY");

            if (string.IsNullOrWhiteSpace(inv.InvoiceCurrency))
                yield return new("BR-15", "YYYYY");

            if (string.IsNullOrWhiteSpace(inv.InvoiceCurrency))
                yield return new("BR-16", "YYYYY");

            if (string.IsNullOrWhiteSpace(inv.InvoiceCurrency))
                yield return new("BR-21", "YYYYY");

            foreach (var e in Validate_BR_KSA())
                yield return e;
        }

        /// <summary>
        /// Validate KSA - business rules (BR-KSA-03).
        /// </summary>
        /// <returns>Any violations.</returns>
        public IEnumerable<ValidationResult> Validate_BR_KSA()
        {
            if (inv.UniqueInvoiceIdentifier == default)
                yield return new("BR-KSA-03", $"The invoice must contain a unique identifier 'UUID' (KSA-1). Current value is '{inv.UniqueInvoiceIdentifier}'");

            if (inv.InvoiceIssueDateTime.Date > NowInSaudiArabia().Date)
                yield return new("BR-KSA-04", $"The document issue date (BT-2) must be less or equal to the current date. Current issue date is {inv.InvoiceIssueDateTime.Date}.");

            if (!Enum.IsDefined(inv.InvoiceType))
                yield return new("BR-KSA-05", $"The invoice type code (BT-3) must be equal to a value from the subset of UN/CEFACT code list 1001, agreed for KSA electronic invoices. Current value is {inv.InvoiceType}");

            // TODO Fix
            {
                uint tr = (uint)inv.InvoiceTypeTransactions;
                if ((tr & 0b0000011U) < 0b0000001U || // Neither Standard nor Simplified
                    (tr & 0b0000011U) > 0b0000010U || // Both Standard and Simplified
                    tr > 0b1111111U)   // Bits are set beyond the standard 7
                    yield return new("BR-KSA-06", $"The invoice transaction code (KSA-2) must exist and respect the defined structure. Current value is '{inv.InvoiceTypeTransactions}'");
            }

            if (inv.InvoiceTypeTransactions.HasFlag(InvoiceTransaction.Exports) && inv.InvoiceTypeTransactions.HasFlag(InvoiceTransaction.SelfBilled))
                yield return new("BR-KSA-07", $"Self-billing is not allowed (KSA-2, position 7 cannot be '1') for export invoices (KSA-2, position 5 = '1')");

            if (inv.Seller == null || inv.Seller.Id == null)
                yield return new("BR-KSA-08", $"The seller identification (BT-29) must exist.");
            else if (!ACCEPTABLE_SELLER_ID_SCHEMES.Contains(inv.Seller.Id.Value.Scheme))
                yield return new("BR-KSA-08", $"The seller identification (BT-29) must have one of the following scheme IDs (BT-29-1) ['{string.Join("', '", ACCEPTABLE_SELLER_ID_SCHEMES)}']. Current value is '{inv.Seller.Id.Value.Scheme}'");

            if (string.IsNullOrWhiteSpace(inv.Seller?.Address?.Street))
                yield return new("BR-KSA-09", $"Seller address must contain street name (BT-35).");

            if (string.IsNullOrWhiteSpace(inv.Seller?.Address?.BuildingNumber))
                yield return new("BR-KSA-09", $"Seller address must contain building number (KSA-17).");

            if (string.IsNullOrWhiteSpace(inv.Seller?.Address?.PostalCode))
                yield return new("BR-KSA-09", $"Seller address must contain postal code (BT-38).");

            if (string.IsNullOrWhiteSpace(inv.Seller?.Address?.City))
                yield return new("BR-KSA-09", $"Seller address must contain city (BT-37).");

            if (string.IsNullOrWhiteSpace(inv.Seller?.Address?.District))
                yield return new("BR-KSA-09", $"Seller address must contain district (KSA-3).");

            if (string.IsNullOrWhiteSpace(inv.Seller?.Address?.CountryCode))
                yield return new("BR-KSA-09", $"Seller address must contain country code (BT-40).");

            if (IsStandard(inv))
            {
                if (string.IsNullOrWhiteSpace(inv.Buyer?.Address?.Street))
                    yield return new("BR-KSA-10", $"Buyer address must contain street name (BT-35).");

                if (string.IsNullOrWhiteSpace(inv.Buyer?.Address?.City))
                    yield return new("BR-KSA-10", $"Buyer address must contain city (BT-37).");

                if (string.IsNullOrWhiteSpace(inv.Buyer?.Address?.CountryCode))
                    yield return new("BR-KSA-10", $"Buyer address must contain country code (BT-40).");
            }

            foreach (var ac in inv.AllowanceCharges)
            {
                if (ac.Indicator == AllowanceChargeType.Allowance && ac.VatCategory == VatCategory.NotSubjectToTax && ac.VatRate != 0.0m)
                    yield return new("BR-KSA-12", $"A Document level allowance (BG-20) where VAT category code (BT-95) is 'Not subject to VAT', the Document level allowance VAT rate (BT-96) shall be 0 (Zero). Current value is {ac.VatRate}.");

                if (ac.Indicator == AllowanceChargeType.Charge && ac.VatCategory == VatCategory.NotSubjectToTax && ac.VatRate != 0.0m)
                    yield return new("BR-KSA-13", $"A Document level allowance (BG-20) where VAT category code (BT-95) is 'Not subject to VAT', the Document level allowance VAT rate (BT-96) shall be 0 (Zero). Current value is {ac.VatRate}.");
            }

            // ??? Is Buyer Id required even in Simple invoice?
            if (inv.Buyer != null)
            {
                var buyer = inv.Buyer;

                if (string.IsNullOrWhiteSpace(buyer.VatNumber))
                {
                    if (buyer.Id == null)
                        yield return new("BR-KSA-14", $"The buyer identification (BT-46), required only if buyer is not VAT registered.");
                    else if (!ACCEPTABLE_BUYER_ID_SCHEMES.Contains(buyer.Id.Value.Scheme))
                        yield return new("BR-KSA-14", $"The buyer identification (BT-46) must have one of the following scheme IDs (BT-46-1) ['{string.Join("', '", ACCEPTABLE_BUYER_ID_SCHEMES)}']. Current value is '{buyer.Id.Value.Scheme}'");
                }
            }

            foreach (var (line, index) in inv.Lines.Select((e, i) => (e, i)))
            {
                if (line.ItemVatCategory == VatCategory.NotSubjectToTax && line.ItemVatRate != 0.0m)
                    yield return new("BR-KSA-11", $"An Invoice line (BG-25) where the VAT category code (BT-151) is 'Not subject to VAT', the invoiced item VAT rate (BT-152) shall be 0 (zero), if exist. Current value is {line.ItemVatRate} at line index {index}.");
            }

            if (inv.InvoiceType == InvoiceType.TaxInvoice && IsStandard(inv) && inv.SupplyDate == default)
                yield return new("BR-KSA-15", $"A standard tax invoice must contain the supply date (KSA-5).");

            if (!Enum.IsDefined(inv.PaymentMeans))
                yield return new("BR-KSA-16", $"Payment means code (BT-81) must be one of the values from a subet of UNTDID 4461 code list. Current value is {inv.PaymentMeans}.");

            if (inv.InvoiceType == InvoiceType.CreditNote || inv.InvoiceType == InvoiceType.DebitNote)
            {
                if (inv.ReasonsForIssuanceOfCreditDebitNote == null || !inv.ReasonsForIssuanceOfCreditDebitNote.Where(e => !string.IsNullOrWhiteSpace(e)).Any())
                    yield return new("BR-KSA-17", $"Debit and credit note (invoice type code (BT-3) is equal to 383 or 381) must contain the reason (KSA-10) for this invoice type issuing.");
            }

            //if (!Enum.IsDefined(inv.VatCategory))
            //    yield return new("BR-KSA-18", $"VAT category code must contain one of the values ({VatCategory.StandardRate}, {VatCategory.ZeroRatedGoods}, {VatCategory.ExemptFromTax}, {VatCategory.NotSubjectToTax}). Current value is {inv.VatCategory}.");

            foreach (var charge in inv.AllowanceCharges.Where(e => e.Indicator == AllowanceChargeType.Charge))
            {
                if (string.IsNullOrWhiteSpace(charge.ReasonCode))
                {
                    yield return new("BR-KSA-19", $"Each Document level charge (BG-21) shall have a code for the reason for document level charge (BT-105).");
                }
            }

            foreach (var charge in inv.Lines.Select(line => line.AllowanceCharge).Where(e => e?.Indicator == AllowanceChargeType.Charge))
            {
                if (charge != null && string.IsNullOrWhiteSpace(charge.ReasonCode))
                {
                    yield return new("BR-KSA-20", $"Each Invoice line charge (BG-28) shall have a code for the reason for invoice line charge (BT-145).");
                }
            }

            // TODO ...
        }

        /// <summary>
        /// Returns true when validating a standard invoice.
        /// </summary>
        private static bool IsStandard(Invoice inv) => inv.InvoiceTypeTransactions.HasFlag(InvoiceTransaction.Standard);

        /// <summary>
        /// Returns the current <see cref="DateTime"/> in Saudi Arabia's time zone.
        /// </summary>
        private static DateTime NowInSaudiArabia()
        {
            var saudiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            return TimeZoneInfo.ConvertTime(DateTime.Now, saudiTimeZone);
        }
    }

    public class ValidationResult
    {
        public ValidationResult(string message)
        {
            Message = message;
            Severity = Severity.Error;
        }

        public ValidationResult(Severity sev, string message)
        {
            Message = message;
            Severity = sev;
        }

        public ValidationResult(string rule, string message)
        {
            Rule = rule;
            Message = message;
            Severity = Severity.Error;
        }

        public string? Rule { get; }
        public string Message { get; }
        public Severity Severity { get; }
    }

    public enum Severity
    {
        Error = 0, Warning = 1
    }
}
