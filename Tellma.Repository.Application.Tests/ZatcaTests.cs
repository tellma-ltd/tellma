using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tellma.Integration.Zatca;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.Repository.Application.Tests
{
    public class ZatcaTests : TestsBase, IClassFixture<ApplicationRepositoryFixture>
    {
        private const string ZATCA_SANDBOX_BASE_URL = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal";

        #region Lifecycle

        private readonly ITestOutputHelper _output;
        private readonly HttpClient _httpClient = new();

        public ZatcaTests(ApplicationRepositoryFixture fixture, ITestOutputHelper output) : base(fixture)
        {
            _output = output;
        }

        #endregion

        [Theory(DisplayName = "[dal].[Zatca__GetInvoices] works ")]
        [InlineData(11448)] // Each one is a document Id
        [InlineData(11449)]
        public async Task Zatca__GetInvoices(int docId)
        {
            // These were obtained from the FATOORA portal and CLI tool
            const string securityToken = "TUlJRDFEQ0NBM21nQXdJQkFnSVRid0FBZTNVQVlWVTM0SS8rNVFBQkFBQjdkVEFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURZeE1qRTNOREExTWxvWERUSTBNRFl4TVRFM05EQTFNbG93U1RFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCV0ZuYVd4bE1SWXdGQVlEVlFRTEV3MW9ZWGxoSUhsaFoyaHRiM1Z5TVJJd0VBWURWUVFERXdreE1qY3VNQzR3TGpFd1ZqQVFCZ2NxaGtqT1BRSUJCZ1VyZ1FRQUNnTkNBQVRUQUs5bHJUVmtvOXJrcTZaWWNjOUhEUlpQNGI5UzR6QTRLbTdZWEorc25UVmhMa3pVMEhzbVNYOVVuOGpEaFJUT0hES2FmdDhDL3V1VVk5MzR2dU1ObzRJQ0p6Q0NBaU13Z1lnR0ExVWRFUVNCZ0RCK3BId3dlakViTUJrR0ExVUVCQXdTTVMxb1lYbGhmREl0TWpNMGZETXRNVEV5TVI4d0hRWUtDWkltaVpQeUxHUUJBUXdQTXpBd01EYzFOVGc0TnpBd01EQXpNUTB3Q3dZRFZRUU1EQVF4TVRBd01SRXdEd1lEVlFRYURBaGFZWFJqWVNBeE1qRVlNQllHQTFVRUR3d1BSbTl2WkNCQ2RYTnphVzVsYzNNek1CMEdBMVVkRGdRV0JCU2dtSVdENmJQZmJiS2ttVHdPSlJYdkliSDlIakFmQmdOVkhTTUVHREFXZ0JSMllJejdCcUNzWjFjMW5jK2FyS2NybVRXMUx6Qk9CZ05WSFI4RVJ6QkZNRU9nUWFBL2hqMW9kSFJ3T2k4dmRITjBZM0pzTG5waGRHTmhMbWR2ZGk1ellTOURaWEowUlc1eWIyeHNMMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEV1WTNKc01JR3RCZ2dyQmdFRkJRY0JBUVNCb0RDQm5UQnVCZ2dyQmdFRkJRY3dBWVppYUhSMGNEb3ZMM1J6ZEdOeWJDNTZZWFJqWVM1bmIzWXVjMkV2UTJWeWRFVnVjbTlzYkM5VVUxcEZhVzUyYjJsalpWTkRRVEV1WlhoMFoyRjZkQzVuYjNZdWJHOWpZV3hmVkZOYVJVbE9WazlKUTBVdFUzVmlRMEV0TVNneEtTNWpjblF3S3dZSUt3WUJCUVVITUFHR0gyaDBkSEE2THk5MGMzUmpjbXd1ZW1GMFkyRXVaMjkyTG5OaEwyOWpjM0F3RGdZRFZSMFBBUUgvQkFRREFnZUFNQjBHQTFVZEpRUVdNQlFHQ0NzR0FRVUZCd01DQmdnckJnRUZCUWNEQXpBbkJna3JCZ0VFQVlJM0ZRb0VHakFZTUFvR0NDc0dBUVVGQndNQ01Bb0dDQ3NHQVFVRkJ3TURNQW9HQ0NxR1NNNDlCQU1DQTBrQU1FWUNJUUNWd0RNY3E2UE8rTWNtc0JYVXovdjFHZGhHcDdycVNhMkF4VEtTdjgzOElBSWhBT0JOREJ0OSszRFNsaWpvVmZ4enJkRGg1MjhXQzM3c21FZG9HV1ZyU3BHMQ==";
            const string secret = "Xlj15LyMCgSC66ObnEO/qVPfhSbs3kDTjWnGheYhfSs=";
            const string privateKeyContent = "MHQCAQEEIDyLDaWIn/1/g3PGLrwupV4nTiiLKM59UEqUch1vDfhpoAcGBSuBBAAKoUQDQgAEYYMMoOaFYAhMO/steotfZyavr6p11SSlwsK9azmsLY7b1b+FLhqMArhB2dqHKboxqKNfvkKDePhpqjui5hcn0Q==";

            // Call Stored Procedure
            var ids = new List<int> { docId };
            var output = await Repo.Zatca__GetInvoices(ids);

            // Map the returned invoice
            var inv = Assert.Single(output.Invoices);
            var invoice = new Invoice
            {
                InvoiceNumber = inv.InvoiceNumber,
                UniqueInvoiceIdentifier = inv.UniqueInvoiceIdentifier,
                InvoiceIssueDateTime = inv.InvoiceIssueDateTime,
                InvoiceType = (InvoiceType)inv.InvoiceType,
                InvoiceTypeTransactions = ToInvoiceTransaction(inv),
                InvoiceNotes = string.IsNullOrWhiteSpace(inv.InvoiceNote) ? new() : new() { inv.InvoiceNote },
                InvoiceCurrency = inv.InvoiceCurrency,
                PurchaseOrderId = inv.PurchaseOrderId,
                BillingReferenceId = inv.BillingReferenceId,
                ContractId = inv.ContractId,
                InvoiceCounterValue = (output.PreviousCounterValue + 1).ToString(),
                PreviousInvoiceHash = output.PreviousInvoiceHash,
                Seller = new Party
                {
                    Id = new(PartyIdScheme.CommercialRegistration, "454634645645654"),
                    Address = new Address
                    {
                        Street = "Tahlia St",
                        AdditionalStreet = "123",
                        BuildingNumber = "1820",
                        AdditionalNumber = "3104",
                        District = "Riyadh District",
                        City = "Riyadh",
                        PostalCode = "12345",
                        Province = "Riyadh Province",
                        CountryCode = "SA",
                    },
                    VatNumber = "300075588700003",
                    Name = "ACME International"
                },
                Buyer = new Party
                {
                    Id = inv.BuyerIdScheme == "VAT" ? null : new(ToPartyIdSchema(inv.BuyerIdScheme), inv.BuyerId),
                    Address = new Address
                    {
                        Street = inv.BuyerAddressStreet,
                        AdditionalStreet = inv.BuyerAddressAdditionalStreet,
                        BuildingNumber = inv.BuyerAddressBuildingNumber,
                        AdditionalNumber = inv.BuyerAddressAdditionalNumber,
                        District = inv.BuyerAddressDistrict,
                        City = inv.BuyerAddressCity,
                        PostalCode = inv.BuyerAddressPostalCode,
                        Province = inv.BuyerAddressProvince,
                        CountryCode = inv.BuyerAddressCountryCode,
                    },
                    VatNumber = inv.BuyerIdScheme == "VAT" ? inv.BuyerId : null,
                    Name = inv.BuyerName
                },
                SupplyDate = inv.SupplyDate,
                SupplyEndDate = inv.SupplyEndDate,
                PaymentMeans = (PaymentMeans)inv.PaymentMeans,
                ReasonsForIssuanceOfCreditDebitNote = string.IsNullOrWhiteSpace(inv.ReasonForIssuanceOfCreditDebitNote) ? new() : new() { inv.ReasonForIssuanceOfCreditDebitNote },
                PaymentTerms = inv.PaymentTerms,
                PaymentAccountId = inv.PaymentAccountId,
                AllowanceCharges = inv.AllowanceCharges.Select(ac => new AllowanceCharge
                {
                    Indicator = ac.IsCharge ? AllowanceChargeType.Charge : AllowanceChargeType.Allowance,
                    Amount = ac.Amount,
                    Reason = ac.Reason,
                    ReasonCode = ac.ReasonCode,
                    VatCategory = ToVatCategory(ac.VatCategory),
                    VatRate = ac.VatRate,
                }).ToList(),
                InvoiceTotalVatAmountInAccountingCurrency = inv.InvoiceTotalVatAmountInAccountingCurrency,
                PrepaidAmount = inv.PrepaidAmount,
                RoundingAmount = inv.RoundingAmount,
                VatCategoryTaxableAmount = inv.VatCategoryTaxableAmount,
                VatCategory = ToVatCategory(inv.VatCategory),
                VatRate = inv.VatRate,
                VatExemptionReason = inv.VatExemptionReason,
                VatExemptionReasonCode = inv.VatExemptionReasonCode,
                Lines = inv.Lines.Select((line, index) => new InvoiceLine
                {
                    Identifier = index + 1,
                    PrepaymentId = line.PrepaymentId,
                    PrepaymentUuid = line.PrepaymentUuid,
                    PrepaymentIssueDateTime = line.PrepaymentIssueDateTime,
                    Quantity = line.Quantity,
                    QuantityUnit = line.QuantityUnit,
                    NetAmount = line.NetAmount,
                    AllowanceCharge = new LineAllowanceCharge
                    {
                        Indicator = line.AllowanceChargeIsCharge ? AllowanceChargeType.Charge : AllowanceChargeType.Allowance,
                        Amount = line.AllowanceChargeAmount,
                        Reason = line.AllowanceChargeReason,
                        ReasonCode = line.AllowanceChargeReasonCode,
                    },
                    VatAmount = line.VatAmount,
                    PrepaymentVatCategoryTaxableAmount = line.PrepaymentVatCategoryTaxableAmount,
                    ItemName = line.ItemName,
                    ItemBuyerIdentifier = line.ItemBuyerIdentifier,
                    ItemSellerIdentifier = line.ItemSellerIdentifier,
                    ItemStandardIdentifier = line.ItemStandardIdentifier,
                    ItemNetPrice = line.ItemNetPrice,
                    ItemVatCategory = ToVatCategory(line.ItemVatCategory),
                    ItemVatRate = line.ItemVatRate,
                    PrepaymentVatCategory = ToVatCategory(line.PrepaymentVatCategory),
                    PrepaymentVatRate = line.PrepaymentVatRate,
                    ItemPriceBaseQuantity = line.ItemPriceBaseQuantity,
                    ItemPriceBaseQuantityUnit = line.ItemPriceBaseQuantityUnit,
                    ItemPriceDiscount = line.ItemPriceDiscount,
                    ItemGrossPrice = line.ItemGrossPrice,
                }).ToList(),
            };

            // Build and sign the XML
            var xmlBuilder = new InvoiceXml(invoice).Build();
            var certificateContent = Encoding.UTF8.GetString(Convert.FromBase64String(securityToken));
            var signatureInfo = xmlBuilder.Sign(certificateContent, privateKeyContent);
            var xml = xmlBuilder.GetXml();

            // Prepare the call the ZATCA sandbox
            var client = new ZatcaClient(ZATCA_SANDBOX_BASE_URL, _httpClient);
            var credentials = new Credentials(username: securityToken, password: secret);
            Response response;
            string responseBody;
            if (inv.IsSimplified)
            {
                // Report the simplified invoice
                var res = await client.ReportInvoice(new ReportingRequest
                {
                    InvoiceHash = signatureInfo.InvoiceHash,
                    Uuid = invoice.UniqueInvoiceIdentifier,
                    Description = "Test invoice",
                    Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml))
                },
                credentials);

                response = res;
                responseBody = JsonSerializer.Serialize(res.Result, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                // Clear the simplified invoice
                var res = await client.ClearInvoice(new ClearanceRequest
                {
                    InvoiceHash = signatureInfo.InvoiceHash,
                    Uuid = invoice.UniqueInvoiceIdentifier,
                    Description = "Test invoice",
                    Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml))
                },
                credentials);

                response = res;
                responseBody = JsonSerializer.Serialize(res.Result, new JsonSerializerOptions { WriteIndented = true });
            }

            // Print the response for debugging
            _output.WriteLine("----- Sandbox Response -----");
            _output.WriteLine(responseBody);
            _output.WriteLine("");

            // Print the XML for debugging
            _output.WriteLine("----- Signed XML -----");
            _output.WriteLine(xml);

            // Assert success
            Assert.Equal(ResponseStatus.Success, response.Status);
            Assert.True(response.IsSuccess);
        }

        #region Helpers

        private static InvoiceTransaction ToInvoiceTransaction(ZatcaInvoice inv)
        {
            InvoiceTransaction result = 0;

            result |= inv.IsSimplified ? InvoiceTransaction.Simplified : InvoiceTransaction.Standard;
            if (inv.IsThirdParty)
                result |= InvoiceTransaction.ThirdParty;
            if (inv.IsNominal)
                result |= InvoiceTransaction.Nominal;
            if (inv.IsExports)
                result |= InvoiceTransaction.Exports;
            if (inv.IsSummary)
                result |= InvoiceTransaction.Summary;
            if (inv.IsSelfBilled)
                result |= InvoiceTransaction.SelfBilled;

            return result;
        }

        private static PartyIdScheme ToPartyIdSchema(string scheme)
        {
            return scheme switch
            {
                "TIN" => PartyIdScheme.TaxIdentificationNumber,
                "CRN" => PartyIdScheme.CommercialRegistration,
                "MOM" => PartyIdScheme.Momrah,
                "MLS" => PartyIdScheme.Mhrsd,
                "700" => PartyIdScheme.Number700,
                "SAG" => PartyIdScheme.Misa,
                "NAT" => PartyIdScheme.NationalId,
                "GCC" => PartyIdScheme.GccId,
                "IQA" => PartyIdScheme.IqamaNumber,
                "PAS" => PartyIdScheme.PassportId,
                "OTH" => PartyIdScheme.OtherId,
                _ => throw new InvalidOperationException($"Unrecognized Party ID scheme {scheme}"),
            };
        }

        private static VatCategory ToVatCategory(string category)
        {
            return category switch
            {
                "E" => VatCategory.ExemptFromTax,
                "S" => VatCategory.StandardRate,
                "Z" => VatCategory.ZeroRatedGoods,
                "O" => VatCategory.NotSubjectToTax,
                _ => throw new InvalidOperationException($"Unrecognized VAT Category {category}"),
            };
        }

        #endregion
    }
}
