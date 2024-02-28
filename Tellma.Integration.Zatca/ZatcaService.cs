using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Tellma.Utilities.Common;

namespace Tellma.Integration.Zatca
{
    public class ZatcaService(IHttpClientFactory httpClientFactory, ILogger<ZatcaService> logger, IOptions<ZatcaOptions> options)
    {
        private readonly ZatcaOptions _options = options.Value;

        private readonly ZatcaClient _zatcaProductionClient = new(env: Env.Production, httpClientFactory);
        private readonly ZatcaClient _zatcaSimulationClient = new(env: Env.Simulation, httpClientFactory);
        private readonly ZatcaClient _zatcaSandboxClient = new(env: Env.Sandbox, httpClientFactory);

        private readonly ILogger<ZatcaService> _logger = logger;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        #region Sandbox Stuff

        private static readonly HashSet<string> _sandboxOtps = ["111111", "222222", "123456"];

        private const string _sandboxPrivateKey = "MHQCAQEEIDyLDaWIn/1/g3PGLrwupV4nTiiLKM59UEqUch1vDfhpoAcGBSuBBAAKoUQDQgAEYYMMoOaFYAhMO/steotfZyavr6p11SSlwsK9azmsLY7b1b+FLhqMArhB2dqHKboxqKNfvkKDePhpqjui5hcn0Q==";

        private const string _sandboxCompliancePassword = "QydVsSQAqTefIBK1lJxr7wBhzcofz1lHvmuZ0kWBC38=";
        private const string _sandboxOnboardingPassword = "LdustC+/JHbOBZno6HDeOiZBk8ON4wmIxWFBNGkQNI8=";
        private const string _sandboxReportingPassword = "Xlj15LyMCgSC66ObnEO/qVPfhSbs3kDTjWnGheYhfSs=";
        private const string _sandboxClearingPassword = _sandboxReportingPassword;

        private const string _sandboxComplianceUsername = "TUlJRDFEQ0NBM21nQXdJQkFnSVRid0FBZTNVQVlWVTM0SS8rNVFBQkFBQjdkVEFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURZeE1qRTNOREExTWxvWERUSTBNRFl4TVRFM05EQTFNbG93U1RFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCV0ZuYVd4bE1SWXdGQVlEVlFRTEV3MW9ZWGxoSUhsaFoyaHRiM1Z5TVJJd0VBWURWUVFERXdreE1qY3VNQzR3TGpFd1ZqQVFCZ2NxaGtqT1BRSUJCZ1VyZ1FRQUNnTkNBQVRUQUs5bHJUVmtvOXJrcTZaWWNjOUhEUlpQNGI5UzR6QTRLbTdZWEorc25UVmhMa3pVMEhzbVNYOVVuOGpEaFJUT0hES2FmdDhDL3V1VVk5MzR2dU1ObzRJQ0p6Q0NBaU13Z1lnR0ExVWRFUVNCZ0RCK3BId3dlakViTUJrR0ExVUVCQXdTTVMxb1lYbGhmREl0TWpNMGZETXRNVEV5TVI4d0hRWUtDWkltaVpQeUxHUUJBUXdQTXpBd01EYzFOVGc0TnpBd01EQXpNUTB3Q3dZRFZRUU1EQVF4TVRBd01SRXdEd1lEVlFRYURBaGFZWFJqWVNBeE1qRVlNQllHQTFVRUR3d1BSbTl2WkNCQ2RYTnphVzVsYzNNek1CMEdBMVVkRGdRV0JCU2dtSVdENmJQZmJiS2ttVHdPSlJYdkliSDlIakFmQmdOVkhTTUVHREFXZ0JSMllJejdCcUNzWjFjMW5jK2FyS2NybVRXMUx6Qk9CZ05WSFI4RVJ6QkZNRU9nUWFBL2hqMW9kSFJ3T2k4dmRITjBZM0pzTG5waGRHTmhMbWR2ZGk1ellTOURaWEowUlc1eWIyeHNMMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEV1WTNKc01JR3RCZ2dyQmdFRkJRY0JBUVNCb0RDQm5UQnVCZ2dyQmdFRkJRY3dBWVppYUhSMGNEb3ZMM1J6ZEdOeWJDNTZZWFJqWVM1bmIzWXVjMkV2UTJWeWRFVnVjbTlzYkM5VVUxcEZhVzUyYjJsalpWTkRRVEV1WlhoMFoyRjZkQzVuYjNZdWJHOWpZV3hmVkZOYVJVbE9WazlKUTBVdFUzVmlRMEV0TVNneEtTNWpjblF3S3dZSUt3WUJCUVVITUFHR0gyaDBkSEE2THk5MGMzUmpjbXd1ZW1GMFkyRXVaMjkyTG5OaEwyOWpjM0F3RGdZRFZSMFBBUUgvQkFRREFnZUFNQjBHQTFVZEpRUVdNQlFHQ0NzR0FRVUZCd01DQmdnckJnRUZCUWNEQXpBbkJna3JCZ0VFQVlJM0ZRb0VHakFZTUFvR0NDc0dBUVVGQndNQ01Bb0dDQ3NHQVFVRkJ3TURNQW9HQ0NxR1NNNDlCQU1DQTBrQU1FWUNJUUNWd0RNY3E2UE8rTWNtc0JYVXovdjFHZGhHcDdycVNhMkF4VEtTdjgzOElBSWhBT0JOREJ0OSszRFNsaWpvVmZ4enJkRGg1MjhXQzM3c21FZG9HV1ZyU3BHMQ==";
        private const string _sandboxOnboardingUsername = "TUlJQjhEQ0NBWmFnQXdJQkFnSUdBWUZ1TzBsRk1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nakl3TmpFMk1qQXpOakl6V2hjTk1qY3dOakUyTURBd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0EwZ0FNRVVDSUVheUg5UGRhVUViU3B2dFhNb0J5MDZPSjh6aU1BWXVFQnFMNmZyM01LNHZBaUVBL2JBaENIK2NXV21ucHhUay8vNkY3bXk2ZDhXWENlb3g5TjRLbjI5VExMbz0=";
        private const string _sandboxReportingUsername = _sandboxComplianceUsername;
        private const string _sandboxClearingUsername = _sandboxComplianceUsername;

        #endregion

        private string PrivateKey() => _options.PrivateKey ?? throw new ZatcaException($"The setting 'Zatca:{nameof(_options.PrivateKey)}' should be provided in a configuration provider");

        public async Task<ZatcaSecrets> Onboard(
            int tenantId,
            Party seller,
            string orgUnitName,
            string orgIndustry,
            string otp,
            Env env,
            CancellationToken cancellationToken = default)
        {
            // This is a safeguard to ensure that the "Use Sandbox" setting is intended
            // Haven't figured a way to apply a similar safeguard in Simulation
            if (env == Env.Sandbox && !_sandboxOtps.Contains(otp))
            {
                throw new ZatcaException($"The only OTPs allowed in Sandbox mode are: '{string.Join("', '", _sandboxOtps)}'.");
            }

            var zatcaClient = GetZatcaClient(env);

            // 1 - Create the Certificate Signing Request (CSR)
            // See section 2.2.2 in https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_Security_Features_Implementation_Standards_vF.pdf
            string privateKeyContent = PrivateKey();
            string csrProviderName = _options.ProviderName ?? throw new ZatcaException($"The setting 'Zatca:{nameof(_options.ProviderName)}' should be provided in a configuration provider");
            string csrHostingDomain = _options.CsrHostingDomain ?? throw new ZatcaException($"The setting 'Zatca:{nameof(_options.CsrHostingDomain)}' should be provided in a configuration provider");
            string vatNumber = seller.VatNumber ?? throw new ZatcaException("Please specify supplier VAT number.");
            string orgName = seller.Name ?? throw new ZatcaException("Please specify supplier Name.");

            var csrInput = new CsrInput(
                commonName: $"{csrProviderName}-{tenantId}-{vatNumber}", // Unique value per tenant
                serialNumber: $"1-{csrProviderName}|2-{csrHostingDomain}|3-{tenantId}",
                organizationIdentifier: vatNumber,
                organizationUnitName: orgUnitName,
                organizationName: orgName,
                countryCode: "SA",
                invoiceType: "1100", // Support both standard and simplified
                location: csrHostingDomain,
                industry: orgIndustry
            );

            var csrContent = new CsrBuilder(csrInput).GenerateCsr(privateKeyContent);
            var csrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(csrContent));

            // 2 - Submit CSR to ZATCA to retrieve the compliance Cryptographic Stamp Identifier (CSID)
            var csrRequest = new CsrRequest { Csr = csrBase64 };
            var csidResponse = await zatcaClient.CreateComplianceCsid(csrRequest, otp, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!csidResponse.IsSuccess)
            {
                var csrRequestJson = JsonSerializer.Serialize(csrRequest);
                throw new ZatcaException($"Failed to create compliance CSID. Status: {csidResponse.Status}.\n\n --- Response ---\n {csidResponse.ToJson()}.\n\n --- Request --- \n{csrRequestJson}.");
            }

            var csidResult = csidResponse.ResultOrThrow();
            string tempSecurityToken = csidResult.BinarySecurityToken ?? throw new ZatcaException("ZATCA Compliance CSID API returned null binarySecurityToken.");
            string tempSecret = csidResult.Secret ?? throw new ZatcaException("ZATCA Compliance CSID API returned null secret.");

            // 3 - Prove compliance by submitting 6 documents to ZATCA compliance API (Invoice, Debit Note, Credit Note) x (Standard, Simplified)
            foreach (var inv in InvoiceSamples.CreateComplianceInvoices(seller))
            {
                var complianceCredentials = env == Env.Sandbox ? new(_sandboxComplianceUsername, _sandboxCompliancePassword) : new Credentials(tempSecurityToken, tempSecret);

                // Create Invoice XML
                string certificateContent = Encoding.UTF8.GetString(Convert.FromBase64String(tempSecurityToken));
                var builder = new InvoiceXml(inv);
                var signatureInfo = builder.Build().Sign(certificateContent, privateKeyContent);
                var xml = builder.GetXml();

                // Call the ZATCA API
                var complianceRequest = new ComplianceCheckRequest
                {
                    InvoiceHash = signatureInfo.InvoiceHash,
                    Uuid = inv.UniqueInvoiceIdentifier,
                    Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml))
                };

                var response = await zatcaClient.CheckInvoiceCompliance(complianceRequest, complianceCredentials, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (!response.IsSuccess)
                {
                    throw new ZatcaException($@"Failed compliance check for invoice Id='{inv.InvoiceNumber}'.

----- Response -----
{response.ToJson()}
");
                }
            }

            // 4 - Retrieve production compliance CSID
            var onboardingCredentials = env == Env.Sandbox ? new(_sandboxOnboardingUsername, _sandboxOnboardingPassword) : new Credentials(tempSecurityToken, tempSecret);
            var prodCsidRequest = new CreateProductionCsidRequest { ComplianceRequestId = csidResult.RequestId.ToString() };
            var prodCsidResponse = await zatcaClient.CreateProductionCsid(prodCsidRequest, onboardingCredentials, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!prodCsidResponse.IsSuccess)
            {
                var prodCsidRequestJson = JsonSerializer.Serialize(prodCsidRequest);
                throw new ZatcaException($"Failed to create production CSID. Status: {prodCsidResponse.Status}.\n\n --- Response ---\n {prodCsidResponse.ToJson()}.\n\n --- Request --- \n{prodCsidRequestJson}.");
            }
            var prodCsidResult = prodCsidResponse.ResultOrThrow();

            var encryptionKeysString = _options.EncryptionKeys ?? throw new ZatcaException($"The setting 'Zatca:{nameof(_options.EncryptionKeys)}' should be provided in a configuration provider.");
            var encryptionKeys = encryptionKeysString.Split(",");
            var encryptionKey = encryptionKeys[^1].Trim();
            var encryptionKeyIndex = encryptionKeys.Length - 1;

            string securityToken = prodCsidResult.BinarySecurityToken ?? throw new ZatcaException("ZATCA Production CSID API returned null binarySecurityToken.");
            string secret = prodCsidResult.Secret ?? throw new ZatcaException("ZATCA Production CSID API returned null secret.");

            string encryptedSecurityToken = CryptoUtil.Encrypt(securityToken, encryptionKey);
            string encryptedSecret = CryptoUtil.Encrypt(secret, encryptionKey);

            return new ZatcaSecrets(encryptedSecurityToken, encryptedSecret, encryptionKeyIndex);
        }

        public Task Renew(string otp)
        {
            throw new NotImplementedException("");
        }

        public async Task<ClearanceReport> Report(Invoice inv, ZatcaSecrets secrets, Env env)
        {
            var privateKey = env == Env.Sandbox ? _sandboxPrivateKey : PrivateKey();
            var (securityToken, secret) = env == Env.Sandbox ?
                (_sandboxReportingUsername, _sandboxReportingPassword) :
                DecryptSecrets(secrets);

            if (env == Env.Sandbox && inv.Seller != null)
            {
                inv.Seller.VatNumber = "300075588700003"; // To match the hardcoded certificate
            }

            // Create Invoice XML
            string certificateContent = Encoding.UTF8.GetString(Convert.FromBase64String(securityToken));
            var builder = new InvoiceXml(inv);
            var signatureInfo = builder.Build().Sign(certificateContent, privateKey);
            var xml = builder.GetXml();

            // Call the ZATCA API
            var credentials = new Credentials(username: securityToken, password: secret);
            var request = new ReportingRequest
            {
                InvoiceHash = signatureInfo.InvoiceHash,
                Uuid = inv.UniqueInvoiceIdentifier,
                Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml))
            };

            var response = await GetZatcaClient(env).ReportInvoice(request, credentials);

            // Return report
            return new(
                isSuccess: response.IsSuccess,
                hasWarnings: response.Status == ResponseStatus.SuccessWithWarnings,
                invoiceXml: xml,
                invoiceHash: signatureInfo.InvoiceHash,
                validationResults: response.Result?.ValidationResults);
        }

        public async Task<ClearanceReport> Clear(Invoice inv, ZatcaSecrets secrets, Env env)
        {
            var privateKey = env == Env.Sandbox ? _sandboxPrivateKey : PrivateKey();
            var (securityToken, secret) = env == Env.Sandbox ?
                (_sandboxClearingUsername, _sandboxClearingPassword) :
                DecryptSecrets(secrets);

            if (env == Env.Sandbox && inv.Seller != null)
            {
                inv.Seller.VatNumber = "300075588700003"; // To match the certificate
            }

            // Create Invoice XML
            string certificateContent = Encoding.UTF8.GetString(Convert.FromBase64String(securityToken));
            var builder = new InvoiceXml(inv);
            var signatureInfo = builder.Build().Sign(certificateContent, privateKey);
            var xml = builder.GetXml();

            // Call the ZATCA API
            var credentials = new Credentials(username: securityToken, password: secret);
            var request = new ClearanceRequest
            {
                InvoiceHash = signatureInfo.InvoiceHash,
                Uuid = inv.UniqueInvoiceIdentifier,
                Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml))
            };

            var response = await GetZatcaClient(env).ClearInvoice(request, credentials);

            string invoiceXml = xml;
            string invoiceHash = signatureInfo.InvoiceHash;
            if (response.IsSuccess)
            {
                var result = response.ResultOrThrow();

                // Get the invoice XML returned by ZATCA API
                string? zatcaXmlEncoded = result.ClearedInvoice;
                if (!string.IsNullOrWhiteSpace(zatcaXmlEncoded))
                {
                    string zatcaXml = Encoding.UTF8.GetString(Convert.FromBase64String(zatcaXmlEncoded));
                    invoiceXml = zatcaXml;

                    // Find the invoice hash
                    var xdoc = XDocument.Parse(zatcaXml);
                    XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
                    string? zatcaHash = xdoc.Descendants(ds + "Reference")
                                        .FirstOrDefault(e => e.Attribute("Id")?.Value == "invoiceSignedData")?
                                        .Element(ds + "DigestValue")?
                                        .Value;

                    // When clearing, we use the xml
                    // and hash returned from ZATCA
                    // as the final version
                    if (!string.IsNullOrWhiteSpace(zatcaHash))
                    {
                        invoiceHash = zatcaHash;
                    }
                }
            }
            else if (response.Status == ResponseStatus.ClearanceDeactivated)
            {
                // TODO add it to the simplified invoice queue
                return await Report(inv, secrets, env);
            }

            // Return report
            return new(
                isSuccess: response.IsSuccess,
                hasWarnings: response.Status == ResponseStatus.SuccessWithWarnings,
                invoiceXml: invoiceXml,
                invoiceHash: invoiceHash,
                validationResults: response.Result?.ValidationResults);
        }

        private (string securityToken, string secret) DecryptSecrets(ZatcaSecrets secrets)
        {
            var encryptionKeysString = _options.EncryptionKeys ?? throw new ZatcaException($"The setting 'Zatca:{nameof(_options.EncryptionKeys)}' should be provided in a configuration provider.");
            var encryptionKeys = encryptionKeysString.Split(",");
            if (encryptionKeys.Length < secrets.EncryptionKeyIndex)
            {
                throw new ZatcaException($"The given key index {secrets.EncryptionKeyIndex} is outside the range of available keys in the 'Zatca:{nameof(_options.EncryptionKeys)}' config.");
            }

            var encryptionKey = encryptionKeys[secrets.EncryptionKeyIndex].Trim();

            string securityToken = CryptoUtil.Decrypt(secrets.EncryptedSecurityToken, encryptionKey);
            string secret = CryptoUtil.Decrypt(secrets.EncryptedSecret, encryptionKey);

            return (securityToken, secret);
        }

        private ZatcaClient GetZatcaClient(Env env)
        {
            return env switch
            {
                Env.Sandbox => _zatcaSandboxClient,
                Env.Simulation => _zatcaSimulationClient,
                Env.Production => _zatcaProductionClient,
                _ => throw new InvalidOperationException($"Unrecognized Env {env}"),
            };
        }
    }

    public class ZatcaOptions
    {
        /// <summary>
        /// The provider of the current solution.
        /// </summary>
        public string? ProviderName { get; set; }

        /// <summary>
        /// The domain where the current instance of Tellma is hosted.
        /// </summary>
        public string? CsrHostingDomain { get; set; }

        /// <summary>
        /// Used to encrypt and decrypt ZATCA secrets before storing in the DB.
        /// </summary>
        public string? EncryptionKeys { get; set; }

        /// <summary>
        /// The private key used to create the CSR and sign the invoice XML.
        /// This is the PEM content without the header and footer.
        /// </summary>
        public string? PrivateKey { get; set; }
    }

    public class ZatcaException(string msg) : ReportableException(msg)
    {
    }

    public class ZatcaSecrets(
        string encryptedSecurityToken,
        string encryptedSecret,
        int encryptionKeyIndex)
    {
        /// <summary>
        /// Returned by ZATCA Onboarding API.
        /// </summary>
        public string EncryptedSecurityToken { get; } = encryptedSecurityToken;

        /// <summary>
        /// Returned by ZATCA Onboarding API.
        /// </summary>
        public string EncryptedSecret { get; } = encryptedSecret;

        /// <summary>
        /// The index of the key used to encrypt the other properties.
        /// </summary>
        public int EncryptionKeyIndex { get; set; } = encryptionKeyIndex;
    }

    public class ClearanceReport(
        bool isSuccess,
        bool hasWarnings,
        string? invoiceXml,
        string? invoiceHash,
        ResponseValidationResults? validationResults)
    {
        public bool IsSuccess { get; } = isSuccess;
        public bool HasWarnings { get; } = hasWarnings;
        public string? InvoiceXml { get; } = invoiceXml;
        public string? InvoiceHash { get; } = invoiceHash;
        public ResponseValidationResults? ValidationResults { get; } = validationResults;

        [JsonIgnore]
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        public string ValidationResultsJson() => ValidationResults == null ? "" : JsonSerializer.Serialize(this, _jsonOptions);
    }

    /// <summary>
    /// Dependency that <see cref="ZatcaJob"/> and <see cref="ZatcaService"/> use.
    /// This will be in a separate "Contract" project.
    /// </summary>
    public interface IInvoiceQueue
    {
        // GetNextInvoice() -> invoice
        // InvoiceReported(invoice, warnings)
        // InvoiceFailedReporting(invoice, errors)
        // AddToQueue(invoice)
    }

    public interface ISecretsStore
    {
        // GetSecrets()
        // StoreSecrets()
    }

    public interface IInvoiceStore
    {
        // Store(invoiceXml) -> void
    }

    /// <summary>
    /// Reports invoices in the background.
    /// </summary>
    public class ZatcaJob
    {
    }

    public static class CryptoUtil
    {
        /// <summary>
        /// Encrypt <paramref name="plainText"/> using the symmetric <paramref name="key"/> and the Advanced Encryption Standard (AES).
        /// </summary>
        /// <returns>The encrypted text.</returns>
        public static string Encrypt(string plainText, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Decrypt <paramref name="cipherText"/> which was originally encrypted using the <see cref="Encrypt(string, string)"/> function and the same symmetric <paramref name="key"/>.
        /// </summary>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}
