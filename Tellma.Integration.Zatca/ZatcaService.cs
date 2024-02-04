using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Integration.Zatca
{
    public class ZatcaService
    {
        private readonly ZatcaOptions _options;
        private readonly ZatcaClient _zatcaClient;

        public ZatcaService(IHttpClientFactory httpClientFactory, IOptions<ZatcaOptions> options)
        {
            _options = options.Value;
            _zatcaClient = new ZatcaClient(httpClientFactory);
        }

        public async Task<(string csid, string secret, string privateKey)> Onboard(
            int tenantId, 
            string vatNumber, 
            string orgName,
            string orgIndustry,
            string otp, 
            CancellationToken cancellationToken = default)
        {
            // 1 - Create the Certificate Signing Request (CSR)
            // See section 2.2.2 in https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_Security_Features_Implementation_Standards_vF.pdf
            var csrCommonName = _options.CsrCommonName ?? throw new ZatcaException($"The setting 'Zatca:{nameof(_options.CsrCommonName)}' should be provided in a configuration provider");
            var csrHostingDomain = _options.CsrHostingDomain ?? throw new ZatcaException($"The setting 'Zatca:{nameof(_options.CsrHostingDomain)}' should be provided in a configuration provider");
            var csrInput = new CsrInput(
                commonName: csrCommonName,
                serialNumber: $"1-Tellma|2-{csrHostingDomain}|3-{tenantId}",
                organizationIdentifier: vatNumber,
                organizationUnitName: orgName,
                organizationName: orgName,
                countryCode: "SA",
                invoiceType: "1100",
                location: csrHostingDomain,
                industry: orgIndustry
            );

            var csrResult = new CsrBuilder(csrInput).GenerateCsr();

            // 2 - Submit CSR to ZATCA to retrieve the compliance Cryptographic Stamp Identifier (CSID)
            var csrRequest = new CsrRequest { Csr = csrResult.CsrContent };
            var csidResponse = await _zatcaClient.CreateComplianceCsid(csrRequest, otp, cancellationToken);
            if (!csidResponse.IsSuccess)
            {
                throw new ZatcaException($"Failed to create compliance CSID. Status: {csidResponse.Status}. Response: {csidResponse.Result}.");
            }

            var csidResult = csidResponse.ResultOrThrow();
            string tempCsid = csidResult.BinarySecurityToken ?? throw new ZatcaException("ZATCA Compliance CSID API returned null binarySecurityToken");
            string tempSecret = csidResult.Secret ?? throw new ZatcaException("ZATCA Compliance CSID API returned null secret");

            // 3 - Prove compliance by submitting 6 documents to ZATCA compliance API (Invoice, Debit Note, Credit Note) x (Standard, Simplified)
            {
                // TODO Prepare and send 6 documents ..
            }

            // 4 - Retrieve production compliance CSID
            var prodCsidRequest = new CreateProductionCsidRequest { ComplianceRequestId = csidResult.RequestId.ToString() };
            csidResponse = await _zatcaClient.CreateProductionCsid(prodCsidRequest, new Credentials(tempCsid, tempSecret), cancellationToken);
            if (!csidResponse.IsSuccess)
            {
                throw new ZatcaException($"Failed to create production CSID. Status: {csidResponse.Status}. Response: {csidResponse.Result}.");
            }

            string csid = csidResult.BinarySecurityToken ?? throw new ZatcaException("ZATCA Production CSID API returned null binarySecurityToken");
            string secret = csidResult.Secret ?? throw new ZatcaException("ZATCA Production CSID API returned null secret");
            string privateKey = csrResult.PrivateKey;

            return (csid, secret, privateKey);
        }

        public async Task Renew(string otp)
        {

        }

        public async Task<(string invoiceXml, string invoiceHash)> Report(Invoice inv)
        {
            return ("", "");
        }

        public async Task<(string invoiceXml, string invoiceHash)> Clear(Invoice inv)
        {
            return ("", "");
        }
    }

    public class ZatcaOptions
    {
        public string? CsrCommonName { get; set; }
        public string? CsrHostingDomain { get; set; }
    }

    public class ZatcaException : ReportableException
    {
        public ZatcaException(string msg) : base(msg)
        {
             
        }
    }
}
