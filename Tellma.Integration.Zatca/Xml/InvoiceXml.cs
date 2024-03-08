using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Class for generating and signing ZATCA-compliant invoice XML.
    /// </summary>
    public class InvoiceXml(Invoice inv)
    {
        public const string DATE_FORMAT = "yyyy-MM-dd";
        public const string TIME_FORMAT = "HH:mm:ssZ";
        public const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";
        public const string DECIMAL_FORMAT = "0.00";
        public const string UUID_FORMAT = "D";

        readonly XNamespace defaultNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        readonly XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        readonly XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        readonly XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        readonly XNamespace sig = "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2";
        readonly XNamespace sac = "urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2";
        readonly XNamespace sbc = "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2";
        readonly XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
        readonly XNamespace xades = "http://uri.etsi.org/01903/v1.3.2#";

        private readonly Invoice _inv = inv ?? throw new ArgumentNullException(nameof(inv));
        private XDocument? _xdoc;

        protected virtual string GetCurrentTime() =>
            DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        public virtual string GetXml() =>
            ToString(_xdoc ?? throw new InvalidOperationException("Build the invoice XML first."));

        /// <summary>
        /// Generate a UBL 2.1 compliant <see cref="XmlDocument"/> representing the given <paramref name="_inv"/>.
        /// <para/>
        /// The XML specs are defined in 
        /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_XML_Implementation_Standard_%20vF.pdf">ZATCA E-Invoice Standard</see>
        /// and <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary</see> (2023-5-19).
        /// </summary>
        public InvoiceXml Build()
        {
            // Create the Invoice element which is the document root
            var invoiceElem =
                new XElement(defaultNs + "Invoice",
                    new XAttribute(XNamespace.Xmlns + "cbc", cbc),
                    new XAttribute(XNamespace.Xmlns + "cac", cac),
                    new XAttribute(XNamespace.Xmlns + "ext", ext),
                    new XElement(cbc + "ProfileID", "reporting:1.0"),
                    new XElement(cbc + "ID", _inv.InvoiceNumber),
                    new XElement(cbc + "UUID", _inv.UniqueInvoiceIdentifier.ToString(UUID_FORMAT)),
                    new XElement(cbc + "IssueDate", _inv.InvoiceIssueDateTime.UtcDateTime.ToString(DATE_FORMAT, CultureInfo.InvariantCulture)),
                    new XElement(cbc + "IssueTime", _inv.InvoiceIssueDateTime.UtcDateTime.ToString(TIME_FORMAT, CultureInfo.InvariantCulture)),
                    new XElement(cbc + "InvoiceTypeCode",
                        new XAttribute("name", _inv.InvoiceTypeTransactions.ToXml()), // e.g. 0111010
                        ((int)_inv.InvoiceType).ToString() // e.g. 388
                    )
                );

            foreach (var note in _inv.InvoiceNotes.Where(e => !string.IsNullOrWhiteSpace(e)))
                invoiceElem.Add(new XElement(cbc + "Note", note));

            // Add currencies
            invoiceElem.Add(
                new XElement(cbc + "DocumentCurrencyCode", _inv.InvoiceCurrency),
                new XElement(cbc + "TaxCurrencyCode", _inv.TaxCurrency)
            );

            // A few optional references
            if (!string.IsNullOrWhiteSpace(_inv.PurchaseOrderId))
                invoiceElem.Add(new XElement(cac + "OrderReference",
                    new XElement(cbc + "ID", _inv.PurchaseOrderId)
                ));

            if (IsDebitOrCreditNote) // Required for debit and credit notes
                invoiceElem.Add(new XElement(cac + "BillingReference",
                    new XElement(cac + "InvoiceDocumentReference",
                        new XElement(cbc + "ID", _inv.BillingReferenceId)
                    )
                ));

            if (!string.IsNullOrWhiteSpace(_inv.ContractId))
                invoiceElem.Add(new XElement(cac + "ContractDocumentReference",
                    new XElement(cbc + "ID", _inv.ContractId)
                ));

            // Invoice counter and previous document hash
            invoiceElem.Add(
                new XElement(cac + "AdditionalDocumentReference",
                    new XElement(cbc + "ID", "ICV"),
                    new XElement(cbc + "UUID", _inv.InvoiceCounterValue.ToString())
                ),
                new XElement(cac + "AdditionalDocumentReference",
                    new XElement(cbc + "ID", "PIH"),
                    new XElement(cac + "Attachment",
                        new XElement(cbc + "EmbeddedDocumentBinaryObject",
                            new XAttribute("mimeCode", "text/plain"),
                            _inv.PreviousInvoiceHash
                        )
                    )
                ),
                new XElement(cac + "AccountingSupplierParty",
                    MakeParty(_inv.Seller)
                ),
                new XElement(cac + "AccountingCustomerParty",
                    MakeParty(_inv.Buyer)
                )
            );

            // Seller and Buyer
            XElement MakeParty(Party? party)
            {
                var partyElem = new XElement(cac + "Party");

                // Party Id
                if (party?.Id != null)
                {
                    PartyId id = party.Id.Value;
                    partyElem.Add(new XElement(cac + "PartyIdentification",
                        new XElement(cbc + "ID", id.Value,
                            new XAttribute("schemeID", id.Scheme.ToXml())
                        )
                    ));
                }

                // Address
                if (party?.Address != null)
                {
                    var address = party.Address;
                    partyElem.Add(new XElement(cac + "PostalAddress").Grab(out XElement addressElem));

                    if (!string.IsNullOrWhiteSpace(address.Street))
                        addressElem.Add(new XElement(cbc + "StreetName", address.Street));
                    if (!string.IsNullOrWhiteSpace(address.AdditionalStreet))
                        addressElem.Add(new XElement(cbc + "AdditionalStreetName", address.AdditionalStreet));
                    if (!string.IsNullOrWhiteSpace(address.BuildingNumber))
                        addressElem.Add(new XElement(cbc + "BuildingNumber", address.BuildingNumber));
                    if (!string.IsNullOrWhiteSpace(address.AdditionalNumber))
                        addressElem.Add(new XElement(cbc + "PlotIdentification", address.AdditionalNumber));
                    if (!string.IsNullOrWhiteSpace(address.District))
                        addressElem.Add(new XElement(cbc + "CitySubdivisionName", address.District));
                    if (!string.IsNullOrWhiteSpace(address.City))
                        addressElem.Add(new XElement(cbc + "CityName", address.City));
                    if (!string.IsNullOrWhiteSpace(address.PostalCode))
                        addressElem.Add(new XElement(cbc + "PostalZone", address.PostalCode));
                    if (!string.IsNullOrWhiteSpace(address.Province))
                        addressElem.Add(new XElement(cbc + "CountrySubentity", address.Province));
                    if (!string.IsNullOrWhiteSpace(address.CountryCode))
                        addressElem.Add(
                            new XElement(cac + "Country",
                                new XElement(cbc + "IdentificationCode", address.CountryCode)
                            )
                        );
                }

                // VAT number
                partyElem.Add(new XElement(cac + "PartyTaxScheme").Grab(out XElement taxSchemeElem));

                if (!string.IsNullOrWhiteSpace(party?.VatNumber))
                    taxSchemeElem.Add(new XElement(cbc + "CompanyID", party.VatNumber));

                taxSchemeElem.Add(new XElement(cac + "TaxScheme",
                    new XElement(cbc + "ID", "VAT")
                ));

                // Party name
                if (!string.IsNullOrWhiteSpace(party?.Name))
                {
                    partyElem.Add(new XElement(cac + "PartyLegalEntity",
                        new XElement(cbc + "RegistrationName", party.Name)
                    ));
                }

                return partyElem;
            }

            // Supply dates
            if (_inv.SupplyDate != default || _inv.SupplyEndDate != default)
            {
                invoiceElem.Add(new XElement(cac + "Delivery").Grab(out XElement deliveryElem));

                if (_inv.SupplyDate != default)
                    deliveryElem.Add(new XElement(cbc + "ActualDeliveryDate", _inv.SupplyDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture)));
                if (_inv.SupplyEndDate != default)
                    deliveryElem.Add(new XElement(cbc + "LatestDeliveryDate", _inv.SupplyEndDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture)));
            }

            // Payment means
            if (_inv.PaymentMeans != default ||
                _inv.ReasonsForIssuanceOfCreditDebitNote.Any(e => !string.IsNullOrWhiteSpace(e)) ||
                !string.IsNullOrWhiteSpace(_inv.PaymentAccountId) ||
                !string.IsNullOrWhiteSpace(_inv.PaymentTerms))
            {
                invoiceElem.Add(new XElement(cac + "PaymentMeans", // Required sub-element
                    new XElement(cbc + "PaymentMeansCode", _inv.PaymentMeans == default ? "" : ((int)_inv.PaymentMeans).ToString())).Grab(out XElement paymentMeansElem)
                );

                foreach (var reason in _inv.ReasonsForIssuanceOfCreditDebitNote.Where(e => !string.IsNullOrWhiteSpace(e)))
                    paymentMeansElem.Add(new XElement(cbc + "InstructionNote", reason));

                if (!string.IsNullOrWhiteSpace(_inv.PaymentAccountId) || !string.IsNullOrWhiteSpace(_inv.PaymentTerms))
                {
                    paymentMeansElem.Add(new XElement(cac + "PayeeFinancialAccount").Grab(out XElement payeeFinancialAccountElem));

                    if (!string.IsNullOrWhiteSpace(_inv.PaymentAccountId))
                        payeeFinancialAccountElem.Add(new XElement(cbc + "ID", _inv.PaymentAccountId));

                    if (!string.IsNullOrWhiteSpace(_inv.PaymentTerms))
                        payeeFinancialAccountElem.Add(new XElement(cbc + "PaymentNote", _inv.PaymentTerms));
                }
            }

            // Extra charges or discounts
            foreach (var ac in _inv.AllowanceCharges)
            {
                if (ac != null)
                {
                    invoiceElem.Add(
                        new XElement(cac + "AllowanceCharge",
                            new XElement(cbc + "ChargeIndicator", ac.Indicator.ToXml()) // Required sub-element
                        ).Grab(out XElement allowanceChargeElem)
                    );

                    if (!string.IsNullOrWhiteSpace(ac.ReasonCode))
                        allowanceChargeElem.Add(new XElement(cbc + "AllowanceChargeReasonCode", ac.ReasonCode));

                    if (!string.IsNullOrWhiteSpace(ac.Reason))
                        allowanceChargeElem.Add(new XElement(cbc + "AllowanceChargeReason", ac.Reason));

                    if (ac.Percentage != default)
                        allowanceChargeElem.Add(new XElement(cbc + "MultiplierFactorNumeric", (ac.Percentage * 100m).ToString(DECIMAL_FORMAT)));

                    allowanceChargeElem.Add(RoundedAmount("Amount", ac.Amount)); // Required sub-element

                    if (ac.BaseAmount != default)
                        allowanceChargeElem.Add(RoundedAmount("BaseAmount", ac.BaseAmount));

                    // Add the Tax category
                    allowanceChargeElem.Add(
                        new XElement(cac + "TaxCategory",
                            new XElement(cbc + "ID", ac.VatCategory.ToXml()),
                            new XElement(cbc + "Percent", (ac.VatRate * 100m).ToString(DECIMAL_FORMAT)),
                            new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "VAT")
                            )
                        )
                    );
                }
            }

            // VAT amount (Rule BR-KSAEN16931-09)
            {
                // Rule BR-KSAEN16931-08
                invoiceElem.Add(
                    new XElement(cac + "TaxTotal",
                        RoundedAmount("TaxAmount", _inv.InvoiceTotalVatAmount)
                    ).Grab(out XElement taxAmountElem)
                );

                foreach (var vatEntry in _inv.VatBreakdown)
                {
                    taxAmountElem.Add(new XElement(cac + "TaxSubtotal",
                           RoundedAmount("TaxableAmount", vatEntry.VatCategoryTaxableAmount),
                           RoundedAmount("TaxAmount", vatEntry.VatCategoryTaxAmount),
                           new XElement(cac + "TaxCategory",
                               new XElement(cbc + "ID", vatEntry.VatCategory.ToXml()),
                               new XElement(cbc + "Percent", (vatEntry.VatRate * 100m).ToString(DECIMAL_FORMAT)),
                               // ... Optional elements go here ...
                               new XElement(cac + "TaxScheme",
                                   new XElement(cbc + "ID", "VAT")
                               ).Grab(out XElement taxSchemeElem)
                           )
                       )
                    );

                    if (vatEntry.VatExemptionReasonCode != null)
                        taxSchemeElem.AddBeforeSelf(new XElement(cbc + "TaxExemptionReasonCode", vatEntry.VatExemptionReasonCode.Value.ToXml()));

                    if (!string.IsNullOrWhiteSpace(vatEntry.VatExemptionReasonText))
                        taxSchemeElem.AddBeforeSelf(new XElement(cbc + "TaxExemptionReason", vatEntry.VatExemptionReasonText));
                }
            }

            // VAT amount in accounting currency (Rule BR-KSAEN16931-09)
            invoiceElem.Add(
                new XElement(cac + "TaxTotal",
                    new XElement(cbc + "TaxAmount", _inv.InvoiceTotalVatAmountInAccountingCurrency.ToString(DECIMAL_FORMAT),
                        new XAttribute("currencyID", _inv.TaxCurrency)
                    )
                )
            );

            {
                invoiceElem.Add(
                    new XElement(cac + "LegalMonetaryTotal",
                        RoundedAmount("LineExtensionAmount", _inv.SumOfInvoiceLineNetAmount),
                        RoundedAmount("TaxExclusiveAmount", _inv.InvoiceTotalAmountWithoutVat),
                        RoundedAmount("TaxInclusiveAmount", _inv.InvoiceTotalAmountWithVat)
                    ).Grab(out XElement legalMonetaryTotalElem)
                );

                // Conditional
                if (_inv.SumOfAllowancesOnDocumentLevel != default)
                    legalMonetaryTotalElem.Add(RoundedAmount("AllowanceTotalAmount", _inv.SumOfAllowancesOnDocumentLevel));
                if (_inv.SumOfChargesDocumentLevel != default)
                    legalMonetaryTotalElem.Add(RoundedAmount("ChargeTotalAmount", _inv.SumOfChargesDocumentLevel));

                // Optional
                if (_inv.PrepaidAmount != default)
                    legalMonetaryTotalElem.Add(RoundedAmount("PrepaidAmount", _inv.PrepaidAmount));
                if (_inv.RoundingAmount != default)
                    legalMonetaryTotalElem.Add(RoundedAmount("PayableRoundingAmount", _inv.RoundingAmount));

                // Required
                legalMonetaryTotalElem.Add(RoundedAmount("PayableAmount", _inv.AmountDueForPayment));
            }

            // Invoice Lines
            foreach (var line in _inv.Lines)
            {
                invoiceElem.Add(
                    new XElement(cac + "InvoiceLine",
                        new XElement(cbc + "ID", line.Identifier.ToString()),
                        new XElement(cbc + "InvoicedQuantity", line.Quantity.ToString()).Grab(out XElement quantityElem),
                        RoundedAmount("LineExtensionAmount", line.NetAmount)
                    ).Grab(out XElement lineElem)
                );

                // Quantity unit
                if (!string.IsNullOrWhiteSpace(line.QuantityUnit))
                    quantityElem.Add(new XAttribute("unitCode", line.QuantityUnit));

                // Prepayment
                if (_inv.PrepaidAmount != default) // Rule BR-KSA-73 
                {
                    lineElem.Add(
                        new XElement(cac + "DocumentReference",
                            new XElement(cbc + "ID", line.PrepaymentId).Grab(out XElement idElem),
                            // ... Optional UUID goes here ...
                            new XElement(cbc + "IssueDate", line.PrepaymentIssueDateTime.UtcDateTime.ToString(DATE_FORMAT, CultureInfo.InvariantCulture)),
                            new XElement(cbc + "IssueTime", line.PrepaymentIssueDateTime.UtcDateTime.ToString(TIME_FORMAT, CultureInfo.InvariantCulture)),
                            new XElement(cbc + "DocumentTypeCode", ((int)InvoiceType.TaxInvoice).ToString()) // always '388'
                        )
                    );

                    if (line.PrepaymentUuid != default)
                        idElem.AddAfterSelf(new XElement(cbc + "UUID", line.PrepaymentUuid.ToString(UUID_FORMAT)));
                }

                // Extra charges or discounts per line
                var lac = line.AllowanceCharge;
                if (lac != null)
                {
                    lineElem.Add(
                        new XElement(cac + "AllowanceCharge",
                            new XElement(cbc + "ChargeIndicator", lac.Indicator.ToXml())
                        ).Grab(out XElement allowanceChargeElem)
                    );

                    if (!string.IsNullOrWhiteSpace(lac.ReasonCode))
                        allowanceChargeElem.Add(new XElement(cbc + "AllowanceChargeReasonCode", lac.ReasonCode));

                    if (!string.IsNullOrWhiteSpace(lac.Reason))
                        allowanceChargeElem.Add(new XElement(cbc + "AllowanceChargeReason", lac.Reason));

                    if (lac.Percentage != default)
                        allowanceChargeElem.Add(new XElement(cbc + "MultiplierFactorNumeric", (lac.Percentage * 100m).ToString(DECIMAL_FORMAT)));

                    allowanceChargeElem.Add(RoundedAmount("Amount", lac.Amount));

                    if (lac.BaseAmount != default)
                        allowanceChargeElem.Add(RoundedAmount("BaseAmount", lac.BaseAmount));
                }

                // Vat info
                {
                    lineElem.Add(
                        new XElement(cac + "TaxTotal",
                            RoundedAmount("TaxAmount", line.VatAmount),
                            RoundedAmount("RoundingAmount", line.AmountIncludingVat)
                        ).Grab(out XElement taxTotalElem)
                    );

                    // Rule BR-KSA-75
                    if (_inv.PrepaidAmount != default) // ??? is this the correct interpretation?
                    {
                        taxTotalElem.Add(
                            new XElement(cac + "TaxSubtotal",
                                RoundedAmount("TaxableAmount", line.PrepaymentVatCategoryTaxableAmount),
                                RoundedAmount("TaxAmount", line.PrepaymentVatCategoryTaxAmount),
                                new XElement(cac + "TaxCategory",
                                    new XElement(cbc + "ID", line.PrepaymentVatCategory.ToXml()),
                                    new XElement(cbc + "Percent", (line.PrepaymentVatRate * 100m).ToString(DECIMAL_FORMAT)),
                                    new XElement(cac + "TaxScheme",
                                        new XElement(cbc + "ID", "VAT")
                                    )
                                )
                            ).Grab(out XElement taxSubtotalElem)
                        );
                    }
                }

                // Item info
                {
                    lineElem.Add(
                        new XElement(cac + "Item",
                            new XElement(cbc + "Name", line.ItemName)
                        ).Grab(out XElement itemElem)
                    );


                    if (!string.IsNullOrWhiteSpace(line.ItemBuyerIdentifier))
                        itemElem.Add(
                            new XElement(cac + "BuyersItemIdentification",
                                new XElement(cbc + "ID", line.ItemBuyerIdentifier)
                            )
                        );

                    if (!string.IsNullOrWhiteSpace(line.ItemSellerIdentifier))
                        itemElem.Add(
                            new XElement(cac + "SellersItemIdentification",
                                new XElement(cbc + "ID", line.ItemSellerIdentifier)
                            )
                        );

                    if (!string.IsNullOrWhiteSpace(line.ItemStandardIdentifier))
                        itemElem.Add(
                            new XElement(cac + "StandardItemIdentification",
                                new XElement(cbc + "ID", line.ItemStandardIdentifier)
                            )
                        );

                    itemElem.Add(
                        new XElement(cac + "ClassifiedTaxCategory",
                            new XElement(cbc + "ID", line.ItemVatCategory.ToXml()),
                            new XElement(cbc + "Percent", (line.ItemVatRate * 100m).ToString(DECIMAL_FORMAT)),
                            new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "VAT")
                            )
                        )
                    );
                }

                // Price info
                {
                    lineElem.Add(
                        new XElement(cac + "Price",
                            UnroundedAmount("PriceAmount", line.ItemNetPrice)
                        ).Grab(out XElement priceElem)
                    );

                    // Base Quantity
                    if (line.ItemPriceBaseQuantity != default)
                    {
                        priceElem.Add(
                            new XElement(cbc + "BaseQuantity", line.ItemPriceBaseQuantity.ToString()).Grab(out XElement baseQuantityElem)
                        );

                        if (!string.IsNullOrWhiteSpace(line.ItemPriceBaseQuantityUnit))
                            baseQuantityElem.Add(new XAttribute("unitCode", line.ItemPriceBaseQuantityUnit));
                    }

                    if (line.ItemPriceDiscount != default || line.ItemGrossPrice != default)
                    {
                        priceElem.Add(
                            new XElement(cac + "AllowanceCharge",
                                new XElement(cbc + "ChargeIndicator", AllowanceChargeType.Allowance.ToXml()),
                                UnroundedAmount("Amount", line.ItemPriceDiscount),
                                UnroundedAmount("BaseAmount", line.ItemGrossPrice)
                            )
                        );
                    }
                }
            }

            _xdoc = new XDocument();
            _xdoc.Add(invoiceElem);

            return this; // Allows chaining calls
        }

        /// <summary>
        /// Generate a UBL 2.1 compliant <see cref="XmlDocument"/> representing the given <paramref name="_inv"/>.
        /// <para/>
        /// The XML specs are defined in 
        /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_XML_Implementation_Standard_%20vF.pdf">ZATCA E-Invoice Standard</see>
        /// and <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary</see> (2023-5-19).
        /// </summary>

        public SignatureInfo Sign(string certificateContent, string privateKeyContent)
        {
            // Basic null check
            if (_xdoc == null)
            {
                throw new InvalidOperationException("Build the invoice XML before signing.");
            }

            var ublExtensionsElem = new XElement(ext + "UBLExtensions",
                new XElement(ext + "UBLExtension",
                    new XElement(ext + "ExtensionURI", "urn:oasis:names:specification:ubl:dsig:enveloped:xades"),
                    new XElement(ext + "ExtensionContent",
                        new XElement(sig + "UBLDocumentSignatures",
                            new XAttribute(XNamespace.Xmlns + "sig", sig),
                            new XAttribute(XNamespace.Xmlns + "sac", sac),
                            new XAttribute(XNamespace.Xmlns + "sbc", sbc),
                            new XElement(sac + "SignatureInformation",
                                new XElement(cbc + "ID", "urn:oasis:names:specification:ubl:signature:1"),
                                new XElement(sbc + "ReferencedSignatureID", "urn:oasis:names:specification:ubl:signature:Invoice"),
                                new XElement(ds + "Signature",
                                    new XAttribute(XNamespace.Xmlns + "ds", ds),
                                    new XAttribute("Id", "signature"),
                                    new XElement(ds + "SignedInfo",
                                        new XElement(ds + "CanonicalizationMethod",
                                            new XAttribute("Algorithm", "http://www.w3.org/2006/12/xml-c14n11")
                                        ),
                                        new XElement(ds + "SignatureMethod",
                                            new XAttribute("Algorithm", "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256")
                                        ),
                                        new XElement(ds + "Reference",
                                            new XAttribute("Id", "invoiceSignedData"),
                                            new XAttribute("URI", ""),
                                            new XElement(ds + "Transforms",
                                                new XElement(ds + "Transform",
                                                    new XAttribute("Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116"),
                                                    new XElement(ds + "XPath", "not(//ancestor-or-self::ext:UBLExtensions)")
                                                ),
                                                new XElement(ds + "Transform",
                                                    new XAttribute("Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116"),
                                                    new XElement(ds + "XPath", "not(//ancestor-or-self::cac:Signature)")
                                                ),
                                                new XElement(ds + "Transform",
                                                    new XAttribute("Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116"),
                                                    new XElement(ds + "XPath", "not(//ancestor-or-self::cac:AdditionalDocumentReference[cbc:ID='QR'])")
                                                ),
                                                new XElement(ds + "Transform",
                                                    new XAttribute("Algorithm", "http://www.w3.org/2006/12/xml-c14n11")
                                                )
                                            ),
                                            new XElement(ds + "DigestMethod", new XAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256")),
                                            new XElement(ds + "DigestValue").Grab(out XElement invoiceHashElem)
                                        ),
                                        new XElement(ds + "Reference",
                                            new XAttribute("Type", "http://www.w3.org/2000/09/xmldsig#SignatureProperties"),
                                            new XAttribute("URI", "#xadesSignedProperties"),
                                            new XElement(ds + "DigestMethod", new XAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256")),
                                            new XElement(ds + "DigestValue").Grab(out XElement signedPropsHashElem)
                                        )
                                    ),
                                    new XElement(ds + "SignatureValue").Grab(out XElement digitalSignatureElem),
                                    new XElement(ds + "KeyInfo",
                                        new XElement(ds + "X509Data",
                                            new XElement(ds + "X509Certificate").Grab(out XElement certElem)
                                        )
                                    ),
                                    new XElement(ds + "Object",
                                        new XElement(xades + "QualifyingProperties",
                                            new XAttribute(XNamespace.Xmlns + "xades", xades),
                                            new XAttribute("Target", "signature"),
                                            new XElement(xades + "SignedProperties",
                                                new XAttribute("Id", "xadesSignedProperties"),
                                                new XElement(xades + "SignedSignatureProperties",
                                                    new XElement(xades + "SigningTime").Grab(out XElement signingTimeElem),
                                                    new XElement(xades + "SigningCertificate",
                                                        new XElement(xades + "Cert",
                                                            new XElement(xades + "CertDigest",
                                                                new XElement(ds + "DigestMethod",
                                                                    new XAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256")
                                                                ),
                                                                new XElement(ds + "DigestValue").Grab(out XElement certHashElem)
                                                            ),
                                                            new XElement(xades + "IssuerSerial",
                                                                new XElement(ds + "X509IssuerName").Grab(out XElement certIssuerNameElem),
                                                                new XElement(ds + "X509SerialNumber").Grab(out XElement certSerialNumberElem)
                                                            )
                                                        )
                                                    )
                                                )
                                            ).Grab(out XElement signedPropsElem)
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            // Signing time
            string signingTime = GetCurrentTime();

            // Hash the invoice before adding the UBLExtensions element
            byte[] invoiceHashBytes = InvoiceHash(GetXml());
            string invoiceHash = Convert.ToBase64String(invoiceHashBytes);
            byte[] digitalSignatureBytes = DigitalSignature(invoiceHashBytes, privateKeyContent);
            string digitalSignature = Convert.ToBase64String(digitalSignatureBytes);

            // Add the UBL element
            _xdoc.Root?.AddFirst(ublExtensionsElem);

            // Populate SignedProperties and get their hash

            // Get certificate hash
            byte[] certBytes = Encoding.UTF8.GetBytes(certificateContent);
            string certHash = Sha256Hash(certBytes);

            // Get certificate issuer name
            X509Certificate2 cert = new(certBytes);
            string certIssuerName = cert.IssuerName.Name;

            // Get certificate serial number
            byte[] serialNumberBytes = cert.GetSerialNumber();
            Array.Reverse(serialNumberBytes);
            string certSerialNumber = new BigInteger(serialNumberBytes).ToString();

            // Populate XML elements
            signingTimeElem.Value = signingTime;
            certHashElem.Value = certHash;
            certIssuerNameElem.Value = certIssuerName;
            certSerialNumberElem.Value = certSerialNumber;

            // Calculate the hash of SignedProperties
            var xwriterSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,
            };

            // Use StringWriter with XmlWriter to write the XDocument
            using StringWriter swriter = new Utf8StringWriter();
            {
                using XmlWriter xwriter = XmlWriter.Create(swriter, xwriterSettings);
                signedPropsElem.Save(xwriter);
            }
            string signedPropsXml = Canonicalize(swriter.ToString()).Replace(" />", "/>").Replace("></ds:DigestMethod>", "/>");
            string signedPropsHash = Sha256Hash(Encoding.UTF8.GetBytes(signedPropsXml));


            // Populate Remaining UBLExtension properties
            {
                digitalSignatureElem.Value = digitalSignature;
                certElem.Value = certificateContent;
                signedPropsHashElem.Value = signedPropsHash;
                invoiceHashElem.Value = invoiceHash;
            }

            // Generate QR code
            string qrCode;
            {
                // Extract the public key and signature bytes
                Org.BouncyCastle.X509.X509Certificate bouncyCastleCert = DotNetUtilities.FromX509Certificate(cert);
                byte[] publicKeyBytes = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(bouncyCastleCert.GetPublicKey()).GetEncoded();
                byte[] certSignatureBytes = bouncyCastleCert.GetSignature();

                qrCode = QrCode(
                    sellerName: _inv.Seller?.Name ?? string.Empty,
                    vatNumber: _inv.Seller?.VatNumber ?? string.Empty,
                    issueDateTime: _inv.InvoiceIssueDateTime.UtcDateTime.ToString(DATETIME_FORMAT, CultureInfo.InvariantCulture),
                    total: _inv.InvoiceTotalAmountWithVat,
                    vat: _inv.InvoiceTotalVatAmount,
                    invoiceHash: invoiceHash,
                    digitalSignature: digitalSignature,
                    publicKeyBytes: publicKeyBytes,
                    certSignatureBytes: _inv.InvoiceTypeTransactions.HasFlag(InvoiceTransaction.Simplified) ? certSignatureBytes : null
                );
            }

            // Add QR code and Signature elements
            var qrElem = new XElement(cac + "AdditionalDocumentReference",
                new XElement(cbc + "ID", "QR"),
                new XElement(cac + "Attachment",
                    new XElement(cbc + "EmbeddedDocumentBinaryObject",
                        new XAttribute("mimeCode", "text/plain"),
                        qrCode
                    )
                )
            );

            var signatureElem = new XElement(cac + "Signature",
                new XElement(cbc + "ID", "urn:oasis:names:specification:ubl:signature:Invoice"),
                new XElement(cbc + "SignatureMethod", "urn:oasis:names:specification:ubl:dsig:enveloped:xades")
            );

            // Add them after the last AdditionalDocumentReference
            _xdoc.Root?
                .Elements(cac + "AdditionalDocumentReference")?
                .LastOrDefault()?
                .AddAfterSelf(qrElem);

            qrElem?.AddAfterSelf(signatureElem);

            // Return the XML document
            return new SignatureInfo(
                signingTime,
                certHash,
                certIssuerName,
                certSerialNumber,
                digitalSignature,
                signedPropsHash,
                invoiceHash,
                qrCode);
        }

        /// <summary>
        /// Returns true if <see cref="_inv"/> is a standard invoice.
        /// </summary>
        private bool IsStandard => _inv.InvoiceTypeTransactions.HasFlag(InvoiceTransaction.Standard);

        /// <summary>
        /// Returns true if <see cref="_inv"/> is either a debit or a credit note.
        /// </summary>
        private bool IsDebitOrCreditNote => _inv.InvoiceType == InvoiceType.DebitNote || _inv.InvoiceType == InvoiceType.CreditNote;

        /// <summary>
        /// Syntactic sugar: Creates a cbc element with the given <paramref name="elementName"/>, 
        /// containing the given <paramref name="amount"/>, with the invoice currency 
        /// in the 'currencyID' attribute.
        /// </summary>
        private XElement RoundedAmount(string elementName, decimal amount)
        {
            return new XElement(cbc + elementName, amount.ToString(DECIMAL_FORMAT),
                new XAttribute("currencyID", _inv.InvoiceCurrency ?? "")
            );
        }

        /// <summary>
        /// Syntactic sugar: Creates a cbc element with the given <paramref name="elementName"/>, 
        /// containing the given <paramref name="amount"/>, with the invoice currency 
        /// in the 'currencyID' attribute.
        /// </summary>
        private XElement UnroundedAmount(string elementName, decimal amount)
        {
            return new XElement(cbc + elementName, amount.ToString(),
                new XAttribute("currencyID", _inv.InvoiceCurrency ?? "")
            );
        }

        /// <summary>
        /// Turns the <see cref="XDocument"/> into a string with standard formatting.
        /// </summary>
        private static string ToString(XDocument doc)
        {
            using StringWriter swriter = new Utf8StringWriter();
            {
                using XmlWriter xwriter = XmlWriter.Create(swriter);
                doc.Save(xwriter);
            }

            return swriter.ToString();
        }

        /// <summary>
        /// Helper class.
        /// </summary>
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        /// <summary>
        /// Generate the ZATCA Compliant QR code.
        /// </summary>
        /// <param name="sellerName">The name of the seller.</param>
        /// <param name="vatNumber">The VAT registration number of the seller.</param>
        /// <param name="issueDateTime">The issuer date of the invoice.</param>
        /// <param name="total">The <see cref="Invoice.InvoiceTotalAmountWithVat"/>.</param>
        /// <param name="vat">The <see cref="Invoice.InvoiceTotalVatAmount"/>.</param>
        /// <param name="invoiceHash">The invoice hash.</param>
        /// <param name="digitalSignature">The invoice digital signature.</param>
        /// <param name="publicKeyBytes">The ECDSA public key extracted from the signing private key.</param>
        /// <param name="certSignatureBytes">The ECDSA signature of the cryptographic stamp issued by ZATCA’s technical CA (Only for Simplified invoices).</param>
        /// <returns>The QR code encoded in base64.</returns>
        private static string QrCode(
            string sellerName,
            string vatNumber,
            string issueDateTime,
            decimal total,
            decimal vat,
            string invoiceHash,
            string digitalSignature,
            byte[] publicKeyBytes,
            byte[]? certSignatureBytes)
        {
            // Assemble the QR code contents
            var qrContentList = new List<byte>();

            // Seller name
            var sellerNameBytes = Encoding.UTF8.GetBytes(sellerName);
            if (sellerNameBytes.Length > byte.MaxValue)
            {
                throw new InvalidOperationException($"Seller name '{sellerName}' encodes to more than {byte.MaxValue} bytes.");
            }

            qrContentList.Add(1);
            qrContentList.Add((byte)sellerNameBytes.Length);
            qrContentList.AddRange(sellerNameBytes);

            // VAT number
            var vatNumberBytes = Encoding.UTF8.GetBytes(vatNumber);
            if (vatNumberBytes.Length > byte.MaxValue)
            {
                throw new InvalidOperationException($"VAT Number '{vatNumber}' encodes to more than {byte.MaxValue} bytes.");
            }

            qrContentList.Add(2);
            qrContentList.Add((byte)vatNumberBytes.Length);
            qrContentList.AddRange(vatNumberBytes);

            // Timestamp
            var issueDateTimeBytes = Encoding.UTF8.GetBytes(issueDateTime);

            qrContentList.Add(3);
            qrContentList.Add((byte)issueDateTimeBytes.Length);
            qrContentList.AddRange(issueDateTimeBytes);

            // Total
            var totalString = total.ToString(DECIMAL_FORMAT);
            var totalBytes = Encoding.UTF8.GetBytes(totalString);

            qrContentList.Add(4);
            qrContentList.Add((byte)totalBytes.Length);
            qrContentList.AddRange(totalBytes);

            // VAT
            var vatString = vat.ToString(DECIMAL_FORMAT);
            var vatBytes = Encoding.UTF8.GetBytes(vatString);

            qrContentList.Add(5);
            qrContentList.Add((byte)vatBytes.Length);
            qrContentList.AddRange(vatBytes); // ??? What if it was another currency?

            // Invoice hash
            var invoiceHashBytes = Encoding.UTF8.GetBytes(invoiceHash);

            qrContentList.Add(6);
            qrContentList.Add((byte)invoiceHashBytes.Length);
            qrContentList.AddRange(invoiceHashBytes);

            // Digital signature
            var digitalSignatureBytes = Encoding.UTF8.GetBytes(digitalSignature);

            qrContentList.Add(7);
            qrContentList.Add((byte)digitalSignatureBytes.Length);
            qrContentList.AddRange(digitalSignatureBytes);

            // Public key
            qrContentList.Add(8);
            qrContentList.Add((byte)publicKeyBytes.Length);
            qrContentList.AddRange(publicKeyBytes);

            // Certificate signature
            if (certSignatureBytes != null)
            {
                qrContentList.Add(9);
                qrContentList.Add((byte)certSignatureBytes.Length);
                qrContentList.AddRange(certSignatureBytes);
            }

            var qrContent = Convert.ToBase64String(qrContentList.ToArray());
            return qrContent;
        }

        /// <summary>
        /// Generates the SHA256 hash of a an array of bytes.
        /// </summary>
        private static string Sha256Hash(byte[] certRawData)
        {
            byte[] hashInBytes = SHA256.HashData(certRawData);

            StringBuilder stringBuilder = new(hashInBytes.Length * 2);
            foreach (byte b in hashInBytes)
                stringBuilder.Append(b.ToString("x2"));

            string hashInHex = stringBuilder.ToString();
            string hashBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(hashInHex));

            return hashBase64;
        }

        /// <summary>
        /// Generates the digital signature of the invoice hash using the private key.
        /// </summary>
        private static byte[] DigitalSignature(byte[] invoiceHash, string privateKeyContent)
        {
            string privateKeyPem = @$"-----BEGIN EC PRIVATE KEY-----
{privateKeyContent}
-----END EC PRIVATE KEY-----";

            using TextReader reader = new StringReader(privateKeyPem);
            AsymmetricKeyParameter parameters = ((AsymmetricCipherKeyPair)new PemReader(reader).ReadObject()).Private;

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(true, parameters);
            signer.BlockUpdate(invoiceHash, 0, invoiceHash.Length); // Is this needed?
            byte[] signature = signer.GenerateSignature();

            return signature;
        }

        /// <summary>
        /// Generates the SHA256 of the invoice XML.
        /// </summary>
        private static byte[] InvoiceHash(string xml)
        {
            var styledXmlBldr = new StringBuilder();
            using XmlWriter results = XmlWriter.Create(styledXmlBldr, new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8,
                Indent = false
            });

            var assmbly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assmbly.GetName().Name}.Xml.Resources.invoice.xsl";
            using var xslStream = assmbly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Missing resource '{resourceName}'.");

            using XmlReader stylesheet = XmlReader.Create(xslStream);

            XslCompiledTransform compiledTransform = new();
            compiledTransform.Load(stylesheet);

            XmlReader stylesheetReader = XmlReader.Create(new StringReader(xml));
            compiledTransform.Transform(stylesheetReader, results);

            string styledXml = styledXmlBldr.ToString();
            string canonicalXml = Canonicalize(styledXml);
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalXml));

            return hash;
        }

        /// <summary>
        /// Transforms the given <paramref name="xml"/> into c14n canonical form.
        /// </summary>
        private static string Canonicalize(string xml)
        {
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            XmlDsigC14NTransform dsigC14Ntransform = new(false);
            dsigC14Ntransform.LoadInput(memoryStream);

            using var canonicalXmlMemStream = (dsigC14Ntransform.GetOutput() as MemoryStream)
                ?? throw new InvalidOperationException("Null canonicalization output.");

            string canonicalXml = Encoding.UTF8.GetString(canonicalXmlMemStream.ToArray());
            return canonicalXml;
        }
    }

    internal static class InvoiceXmlBuilderExtensions
    {
        internal static XmlElement AddElementImpl(XmlElement parent, string prefix, string localName, string ns, string? value)
        {
            var elem = parent.OwnerDocument.CreateElement(prefix, localName, ns);
            if (value != null)
            {
                elem.InnerText = value;
            }

            parent.AppendChild(elem);
            return elem;
        }

        internal static string ToXml(this InvoiceTransaction v)
        {
            char[] resultList =
            {
                '0', // First one always 0
                v.HasFlag(InvoiceTransaction.Simplified) ? '2': v.HasFlag(InvoiceTransaction.Standard) ? '1' : '0', // Should never be zero
                v.HasFlag(InvoiceTransaction.ThirdParty) ? '1': '0',
                v.HasFlag(InvoiceTransaction.Nominal) ? '1': '0',
                v.HasFlag(InvoiceTransaction.Exports) ? '1': '0',
                v.HasFlag(InvoiceTransaction.Summary) ? '1': '0',
                v.HasFlag(InvoiceTransaction.SelfBilled) ? '1': '0',
            };

            return new string(resultList);
        }

        internal static string ToXml(this PartyIdScheme v)
        {
            return v switch
            {
                PartyIdScheme.TaxIdentificationNumber => "TIN",
                PartyIdScheme.CommercialRegistration => "CRN",
                PartyIdScheme.Momrah => "MOM",
                PartyIdScheme.Mhrsd => "MLS",
                PartyIdScheme.Number700 => "700",
                PartyIdScheme.Misa => "SAG",
                PartyIdScheme.NationalId => "NAT",
                PartyIdScheme.GccId => "GCC",
                PartyIdScheme.IqamaNumber => "IQA",
                PartyIdScheme.PassportId => "PAS",
                PartyIdScheme.OtherId => "OTH",
                _ => "",
            };
        }

        internal static string ToXml(this AllowanceChargeType v)
        {
            return v switch
            {
                AllowanceChargeType.Allowance => "false",
                AllowanceChargeType.Charge => "true",
                _ => "",
            };
        }

        internal static string ToXml(this VatCategory v)
        {
            return v switch
            {
                VatCategory.ExemptFromTax => "E",
                VatCategory.StandardRate => "S",
                VatCategory.ZeroRatedGoods => "Z",
                VatCategory.NotSubjectToTax => "O",
                _ => "",
            };
        }

        internal static string ToXml(this VatExemptionReason v)
        {
            return v switch
            {
                // E
                VatExemptionReason.VATEX_SA_29 => "VATEX-SA-29",
                VatExemptionReason.VATEX_SA_29_7 => "VATEX-SA-29-7",
                VatExemptionReason.VATEX_SA_30 => "VATEX-SA-30",

                // Z
                VatExemptionReason.VATEX_SA_32 => "VATEX-SA-32",
                VatExemptionReason.VATEX_SA_33 => "VATEX-SA-33",
                VatExemptionReason.VATEX_SA_34_1 => "VATEX-SA-34-1",
                VatExemptionReason.VATEX_SA_34_2 => "VATEX-SA-34-2",
                VatExemptionReason.VATEX_SA_34_3 => "VATEX-SA-34-3",
                VatExemptionReason.VATEX_SA_34_4 => "VATEX-SA-34-4",
                VatExemptionReason.VATEX_SA_34_5 => "VATEX-SA-34-5",
                VatExemptionReason.VATEX_SA_35 => "VATEX-SA-35",
                VatExemptionReason.VATEX_SA_36 => "VATEX-SA-36",
                VatExemptionReason.VATEX_SA_EDU => "VATEX-SA-EDU",
                VatExemptionReason.VATEX_SA_HEA => "VATEX-SA-HEA",
                VatExemptionReason.VATEX_SA_MLTRY => "VATEX-SA-MLTRY",

                // O
                VatExemptionReason.VATEX_SA_OOS => "VATEX-SA-OOS",

                _ => throw new InvalidOperationException($"Unrecognized VAT Exemption reason code {v}"),
            };
        }

        internal static XElement Grab(this XElement element, out XElement captured)
        {
            captured = element;
            return element;
        }
    }
}
