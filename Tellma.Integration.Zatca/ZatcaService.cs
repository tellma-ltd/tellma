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

        public async Task Onboard(int tenantId, string vatNumber, string otp, CancellationToken cancellationToken = default)
        {
            // 1 - Create the Certificate Signing Request (CSR)
            var csrInput = new CsrInput(
                commonName: _options.CsrCommonName,
                serialNumber: $"1-Tellma|2-{_options.CsrHostingDomain}|3-{tenantId}",
                organizationIdentifier: vatNumber,
                organizationUnitName: "Riyad Branch",
                organizationName: "Tellma",
                countryCode: "SA",
                invoiceType: "1100",
                location: "Tahlia St",
                industry: "Software"
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
        }

        public async Task Renew(string otp)
        {

        }

        public async Task Report(Invoice inv)
        {

        }

        public async Task<SignatureInfo> Clear(Invoice inv)
        {
            return null;
        }
    }

    public class ZatcaOptions
    {
        public string CsrCommonName { get; set; }
        public string CsrHostingDomain { get; set; }
    }

    public class ZatcaException : ReportableException
    {
        public ZatcaException(string msg) : base(msg)
        {
             
        }
    }
}
