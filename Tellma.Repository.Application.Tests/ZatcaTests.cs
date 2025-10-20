using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Integration.Zatca;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.Repository.Application.Tests
{
    public class ZatcaTests : TestsBase, IClassFixture<ApplicationRepositoryFixture>
    {
        #region Lifecycle

        private readonly ITestOutputHelper _output;
        private readonly HttpClient _httpClient = new();

        public ZatcaTests(ApplicationRepositoryFixture fixture, ITestOutputHelper output) : base(fixture)
        {
            _output = output;
        }

        #endregion
        // Each one is a document Id
        [Theory(DisplayName = "[Zatca__GetInvoices] ")]
        [InlineData(21412, false)] // Simplified tax invoice a/t delivery
        [InlineData(20069, false)] // Standard tax invoice a/t delivery with Discounts/Sales retention
        //[InlineData(17031, false)] // Simplified tax invoice. Services => Sales + Invoice
        //[InlineData(17039, false)] // Standard Tax invoice,  Services => Sales + Invoice
        //[InlineData(16961, true)] // Standard Tax invoice,  Only Prepayment adjustment
        //[InlineData(17038, true)] // Standard Tax invoice, unrelated adjustment
        //[InlineData(16779, false)] // Standard Tax invoice, fractions
        public async Task Zatca__GetInvoices(int docId, bool assert_empty)
        {
            // These were obtained from the FATOORA portal and CLI tool
            const string securityToken = "TUlJRDFEQ0NBM21nQXdJQkFnSVRid0FBZTNVQVlWVTM0SS8rNVFBQkFBQjdkVEFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURZeE1qRTNOREExTWxvWERUSTBNRFl4TVRFM05EQTFNbG93U1RFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCV0ZuYVd4bE1SWXdGQVlEVlFRTEV3MW9ZWGxoSUhsaFoyaHRiM1Z5TVJJd0VBWURWUVFERXdreE1qY3VNQzR3TGpFd1ZqQVFCZ2NxaGtqT1BRSUJCZ1VyZ1FRQUNnTkNBQVRUQUs5bHJUVmtvOXJrcTZaWWNjOUhEUlpQNGI5UzR6QTRLbTdZWEorc25UVmhMa3pVMEhzbVNYOVVuOGpEaFJUT0hES2FmdDhDL3V1VVk5MzR2dU1ObzRJQ0p6Q0NBaU13Z1lnR0ExVWRFUVNCZ0RCK3BId3dlakViTUJrR0ExVUVCQXdTTVMxb1lYbGhmREl0TWpNMGZETXRNVEV5TVI4d0hRWUtDWkltaVpQeUxHUUJBUXdQTXpBd01EYzFOVGc0TnpBd01EQXpNUTB3Q3dZRFZRUU1EQVF4TVRBd01SRXdEd1lEVlFRYURBaGFZWFJqWVNBeE1qRVlNQllHQTFVRUR3d1BSbTl2WkNCQ2RYTnphVzVsYzNNek1CMEdBMVVkRGdRV0JCU2dtSVdENmJQZmJiS2ttVHdPSlJYdkliSDlIakFmQmdOVkhTTUVHREFXZ0JSMllJejdCcUNzWjFjMW5jK2FyS2NybVRXMUx6Qk9CZ05WSFI4RVJ6QkZNRU9nUWFBL2hqMW9kSFJ3T2k4dmRITjBZM0pzTG5waGRHTmhMbWR2ZGk1ellTOURaWEowUlc1eWIyeHNMMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEV1WTNKc01JR3RCZ2dyQmdFRkJRY0JBUVNCb0RDQm5UQnVCZ2dyQmdFRkJRY3dBWVppYUhSMGNEb3ZMM1J6ZEdOeWJDNTZZWFJqWVM1bmIzWXVjMkV2UTJWeWRFVnVjbTlzYkM5VVUxcEZhVzUyYjJsalpWTkRRVEV1WlhoMFoyRjZkQzVuYjNZdWJHOWpZV3hmVkZOYVJVbE9WazlKUTBVdFUzVmlRMEV0TVNneEtTNWpjblF3S3dZSUt3WUJCUVVITUFHR0gyaDBkSEE2THk5MGMzUmpjbXd1ZW1GMFkyRXVaMjkyTG5OaEwyOWpjM0F3RGdZRFZSMFBBUUgvQkFRREFnZUFNQjBHQTFVZEpRUVdNQlFHQ0NzR0FRVUZCd01DQmdnckJnRUZCUWNEQXpBbkJna3JCZ0VFQVlJM0ZRb0VHakFZTUFvR0NDc0dBUVVGQndNQ01Bb0dDQ3NHQVFVRkJ3TURNQW9HQ0NxR1NNNDlCQU1DQTBrQU1FWUNJUUNWd0RNY3E2UE8rTWNtc0JYVXovdjFHZGhHcDdycVNhMkF4VEtTdjgzOElBSWhBT0JOREJ0OSszRFNsaWpvVmZ4enJkRGg1MjhXQzM3c21FZG9HV1ZyU3BHMQ==";
            const string secret = "Xlj15LyMCgSC66ObnEO/qVPfhSbs3kDTjWnGheYhfSs=";
            const string privateKeyContent = "MHQCAQEEIDyLDaWIn/1/g3PGLrwupV4nTiiLKM59UEqUch1vDfhpoAcGBSuBBAAKoUQDQgAEYYMMoOaFYAhMO/steotfZyavr6p11SSlwsK9azmsLY7b1b+FLhqMArhB2dqHKboxqKNfvkKDePhpqjui5hcn0Q==";

            // Call Stored Procedure
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var ids = new List<int> { docId };
            var output = await Repo.Zatca__GetInvoices(ids);
            var dbElapsed = stopwatch.Elapsed.TotalSeconds;

            if (assert_empty)
            {
                // We expect
                Assert.Empty(output.Invoices);
                return;
            }

            // Map the returned invoice
            var inv = Assert.Single(output.Invoices);
            var invoice = DocumentsService.MapInvoice(inv, new(), output.PreviousCounterValue, output.PreviousInvoiceHash);

            invoice.Seller = new Party
            {
                Id = new(PartyIdScheme.CommercialRegistration, "1010753744"),
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
            };

            // Build and sign the XML
            var xmlBuilder = new InvoiceXml(invoice).Build();
            var certificateContent = Encoding.UTF8.GetString(Convert.FromBase64String(securityToken));
            var signatureInfo = xmlBuilder.Sign(certificateContent, privateKeyContent);
            var xml = xmlBuilder.GetXml();

            // Prepare the call the ZATCA sandbox
            var client = new ZatcaClient(Env.Sandbox, _httpClient);
            var credentials = new Credentials(username: securityToken, password: secret);
            Response response;
            string responseBody;
            stopwatch.Restart();
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
                stopwatch.Stop();

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
                stopwatch.Stop();

                response = res;
                responseBody = JsonSerializer.Serialize(res.Result, new JsonSerializerOptions { WriteIndented = true });
            }

            var zatcaElapsed = stopwatch.Elapsed.TotalSeconds;

            // Print the response for debugging
            _output.WriteLine("----- Performance Stats -----");
            _output.WriteLine($"Calling [dal].[Zatca__GetInvoices]: {dbElapsed:0.##} sec");
            _output.WriteLine($"Calling ZATCA Sandbox API: {zatcaElapsed:0.##} sec");
            _output.WriteLine("");


            _output.WriteLine("----- Response from Sandbox -----");
            _output.WriteLine(responseBody);
            _output.WriteLine("");

            // Print the XML for debugging
            _output.WriteLine("----- Invoice XML sent to Sandbox -----");
            _output.WriteLine(xml);

            // Assert success
            Assert.Equal(ResponseStatus.Success, response.Status);
            Assert.True(response.IsSuccess);
        }
    }
}
