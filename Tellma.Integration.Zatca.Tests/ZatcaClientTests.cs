using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Tellma.Integration.Zatca.Tests
{
    public partial class ZatcaClientTests
    {
        private const string _sectionName = "ZatcaSandbox";
        private static readonly HttpClient _httpClient = new();

        readonly ZatcaSandboxOptions _options;

        public ZatcaClientTests()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<ZatcaClientTests>();

            _options = builder
                .Build()
                .GetSection(_sectionName)
                .Get<ZatcaSandboxOptions>()
                ?? throw new InvalidOperationException($"Running {nameof(ZatcaClientTests)} requires a '{_sectionName}' section in the Test project user secrets");
        }

        [Fact(DisplayName = "Report an invoice with warnings")]
        public async Task ReportingSingleWithWarnings()
        {
            // Arrange
            var creds = GetReportingCredentials();
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<ReportingRequest>(@"Resources\Requests\ReportingRequest_ValidWithWarnings.json");
            var response = await client.ReportInvoice(request, creds);

            // Assert
            Assert.Equal(ResponseStatus.SuccessWithWarnings, response.Status);
            Assert.True(response.IsSuccess);

            var result = response.Result;
            Assert.NotNull(result);
            Assert.Equal(Constants.ReportingStatus.REPORTED, result.ReportingStatus);

            var validationResults = result.ValidationResults;
            Assert.NotNull(validationResults);
            Assert.Equal(Constants.ValidationStatus.WARNING, validationResults.Status);

            // There is one info message with populated properties
            var info = Assert.Single(validationResults.InfoMessages);
            Assert.Equal(Constants.ValidationStatus.PASS, info.Status);
            Assert.Equal(Constants.ValidationType.INFO, info.Type);
            Assert.NotNull(info.Message);
            Assert.NotNull(info.Category);
            Assert.NotNull(info.Code);

            // There is one or more warning messages with populated properties
            Assert.NotEmpty(validationResults.WarningMessages);
            Assert.All(validationResults.WarningMessages, (warning) =>
            {
                Assert.Equal(Constants.ValidationStatus.WARNING, warning.Status);
                Assert.Equal(Constants.ValidationType.WARNING, warning.Type);
                Assert.NotNull(warning.Message);
                Assert.NotNull(warning.Category);
                Assert.NotNull(warning.Code);
            });

            // There are no error messages
            Assert.Empty(validationResults.ErrorMessages);
        }

        [Fact(DisplayName = "Report an invoice with errors")]
        public async Task ReportingSingleWithErrors()
        {
            // Arrange
            var creds = GetReportingCredentials();
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<ReportingRequest>(@"Resources\Requests\ReportingRequest_Invalid.json");
            var response = await client.ReportInvoice(request, creds);

            // Assert
            Assert.Equal(ResponseStatus.InvalidRequest, response.Status);
            Assert.False(response.IsSuccess);

            var result = response.Result;
            Assert.NotNull(result);
            Assert.Equal(Constants.ReportingStatus.NOT_REPORTED, result.ReportingStatus);

            var validationResults = result.ValidationResults;
            Assert.NotNull(validationResults);
            Assert.Equal(Constants.ValidationStatus.ERROR, validationResults.Status);

            // There are no info or warning messages
            Assert.Empty(validationResults.InfoMessages);
            Assert.Empty(validationResults.WarningMessages);

            // There is one error message with populated properties
            Assert.NotEmpty(validationResults.ErrorMessages);
            Assert.All(validationResults.ErrorMessages, (error) =>
            {
                Assert.Equal(Constants.ValidationStatus.ERROR, error.Status);
                Assert.Equal(Constants.ValidationType.ERROR, error.Type);
                Assert.NotNull(error.Message);
                Assert.NotNull(error.Category);
                Assert.NotNull(error.Code);
            });
        }

        [Fact(DisplayName = "Report an invoice with invalid credenials")]
        public async Task ReportingSingleWithInvalidCredentials()
        {
            // Arrange
            var invalidCreds = new Credentials(username: "foo", password: "bar");
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<ReportingRequest>(@"Resources\Requests\ReportingRequest_ValidWithWarnings.json");
            var response = await client.ReportInvoice(request, invalidCreds);

            Assert.Equal(ResponseStatus.InvalidCredentials, response.Status);
            Assert.False(response.IsSuccess);
        }

        [Fact(DisplayName = "Clear a valid invoice")]
        public async Task ClearingSingle()
        {
            // Arrange
            var creds = GetClearanceCredentials();
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<ClearanceRequest>(@"Resources\Requests\ClearanceRequest_Valid.json");
            var response = await client.ClearInvoice(request, creds);

            // Assert
            Assert.Equal(ResponseStatus.Success, response.Status);
            Assert.True(response.IsSuccess);

            var result = response.Result;
            Assert.NotNull(result);
            Assert.Equal(Constants.ClearanceStatus.CLEARED, result.ClearanceStatus);
            Assert.NotNull(result.ClearedInvoice);

            var validationResults = result.ValidationResults;
            Assert.NotNull(validationResults);
            Assert.Equal(Constants.ValidationStatus.PASS, validationResults.Status);

            // There is one info message with populated properties
            var info = Assert.Single(validationResults.InfoMessages);
            Assert.Equal(Constants.ValidationStatus.PASS, info.Status);
            Assert.Equal(Constants.ValidationType.INFO, info.Type);
            Assert.NotNull(info.Message);
            Assert.NotNull(info.Category);
            Assert.NotNull(info.Code);

            // There are no warning or error messages
            Assert.Empty(validationResults.WarningMessages);
            Assert.Empty(validationResults.ErrorMessages);
        }

        [Fact(DisplayName = "Clear an invoice with inactive clearance")]
        public async Task ClearingSingleWithInactiveClearance()
        {
            // Arrange
            var creds = GetClearanceCredentials();
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<ClearanceRequest>(@"Resources\Requests\ClearanceRequest_Valid.json");
            var response = await client.ClearInvoice(request, creds, activeClearance: false);

            // Assert
            Assert.Equal(ResponseStatus.ClearanceDeactivated, response.Status);
            Assert.False(response.IsSuccess);
        }

        [Fact(DisplayName = "Create Compliance CSID with valid CSR")]
        public async Task ComplianceCsidValid()
        {
            // Arrange
            const string validOtp = "123345";
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<CsrRequest>(@"Resources\Requests\CsrRequest_Valid.json");
            var response = await client.CreateComplianceCsid(request, validOtp);

            // Assert
            Assert.Equal(ResponseStatus.Success, response.Status);
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Result);
            var result = response.Result;

            Assert.Equal(Constants.Disposition.ISSUED, result.DispositionMessage);
            Assert.True(result.RequestId > 0, "The requestID was not a positive long.");
            Assert.NotNull(result.BinarySecurityToken);
            Assert.NotNull(result.Secret);
            Assert.Null(result.Errors);
        }

        [Fact(DisplayName = "Create Compliance CSID with invalid CSR")]
        public async Task ComplianceCsidInvalid()
        {
            // Arrange
            const string validOtp = "123345";
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<CsrRequest>(@"Resources\Requests\CsrRequest_Invalid.json");
            var response = await client.CreateComplianceCsid(request, validOtp);

            // Assert
            Assert.Equal(ResponseStatus.InvalidRequest, response.Status);
            Assert.False(response.IsSuccess);

            var result = response.Result;
            if (result != null)
            {
                Assert.Equal(Constants.Disposition.NOT_COMPLIANT, result.DispositionMessage);
                Assert.NotNull(result.Errors);
                Assert.NotEmpty(result.Errors);
            }
        }

        [Fact(DisplayName = "Create Compliance CSID with invalid OTP")]
        public async Task ComplianceCsidInvalidOtp()
        {
            // Arrange
            const string invalidOtp = "111111";
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<CsrRequest>(@"Resources\Requests\CsrRequest_Valid.json");
            var response = await client.CreateComplianceCsid(request, invalidOtp);

            // Assert
            Assert.Equal(ResponseStatus.InvalidRequest, response.Status);
            Assert.False(response.IsSuccess);
        }

        [Fact(DisplayName = "Create Compliance CSID with expired OTP")]
        public async Task ComplianceCsidExpiredOtp()
        {
            // Arrange
            const string expiredOtp = "222222";
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<CsrRequest>(@"Resources\Requests\CsrRequest_Valid.json");
            var response = await client.CreateComplianceCsid(request, expiredOtp);

            // Assert
            Assert.Equal(ResponseStatus.InvalidRequest, response.Status);
            Assert.False(response.IsSuccess);
        }

        [Fact(DisplayName = "Check Compliance with valid invoice")]
        public async Task CheckCompliance()
        {
            // Arrange
            var creds = GetComplianceCredentials();
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<ComplianceCheckRequest>(@"Resources\Requests\ComplianceRequest_Valid.json");
            var response = await client.CheckInvoiceCompliance(request, creds);

            Assert.Equal(ResponseStatus.Success, response.Status);
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Result);
            var result = response.Result;

            // Assert
            Assert.Equal(Constants.ClearanceStatus.CLEARED, result.ClearanceStatus);

            var validationResults = result.ValidationResults;
            Assert.NotNull(validationResults);
            Assert.Equal(Constants.ValidationStatus.PASS, validationResults.Status);

            // There is one info message with populated properties
            var info = Assert.Single(validationResults.InfoMessages);
            Assert.Equal(Constants.ValidationStatus.PASS, info.Status);
            Assert.Equal(Constants.ValidationType.INFO, info.Type);
            Assert.NotNull(info.Message);
            Assert.NotNull(info.Category);
            Assert.NotNull(info.Code);

            // There are no warning or error messages
            Assert.Empty(validationResults.WarningMessages);
            Assert.Empty(validationResults.ErrorMessages);
        }

        [Fact(DisplayName = "Check Compliance with invalid invoice")]
        public async Task CheckComplianceInvalid()
        {
            // Arrange
            var creds = GetComplianceCredentials();
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<ComplianceCheckRequest>(@"Resources\Requests\ComplianceRequest_Invalid.json");
            var response = await client.CheckInvoiceCompliance(request, creds);

            // Assert
            Assert.Equal(ResponseStatus.InvalidRequest, response.Status);
            Assert.False(response.IsSuccess);

            if (response.Result != null) // Sometimes ZATCA does not return a result
            {
                var result = response.Result;

                Assert.Equal(Constants.ReportingStatus.NOT_REPORTED, result.ReportingStatus);

                var validationResults = result.ValidationResults;
                Assert.NotNull(validationResults);
                Assert.Equal(Constants.ValidationStatus.ERROR, validationResults.Status);

                // There are error messages with populated properties
                Assert.NotEmpty(validationResults.ErrorMessages);
                Assert.All(validationResults.ErrorMessages, (error) =>
                {
                    Assert.Equal(Constants.ValidationStatus.ERROR, error.Status);
                    Assert.Equal(Constants.ValidationType.ERROR, error.Type);
                    Assert.NotNull(error.Message);
                    Assert.NotNull(error.Category);
                    Assert.NotNull(error.Code);
                });
            }
        }

        [Fact(DisplayName = "Create Production CSID with valid request Id")]
        public async Task OnboardingValid()
        {
            // Arrange
            var client = GetZatcaClient();
            var creds = GetOnboardingCredentials();
            var request = new CreateProductionCsidRequest
            {
                ComplianceRequestId = "1234567890123" // Valid
            };

            // Act
            var response = await client.CreateProductionCsid(request, creds);

            // Assert
            Assert.Equal(ResponseStatus.Success, response.Status);
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Result);
            var result = response.Result;

            Assert.Equal(Constants.Disposition.ISSUED, result.DispositionMessage);
            Assert.True(result.RequestId > 0, "The requestID was not a positive long.");
            Assert.NotNull(result.BinarySecurityToken);
            Assert.NotNull(result.Secret);
            Assert.Null(result.Errors);
        }

        [Fact(DisplayName = "Create Production CSID with invalid request Id")]
        public async Task OnboardingInvalid()
        {
            // Arrange
            var client = GetZatcaClient();
            var creds = GetOnboardingCredentials();
            var request = new CreateProductionCsidRequest
            {
                ComplianceRequestId = "5006007008009" // Invalid
            };

            // Act
            var response = await client.CreateProductionCsid(request, creds);

            // Assert
            Assert.Equal(ResponseStatus.InvalidRequest, response.Status);
            Assert.False(response.IsSuccess);
        }

        [Fact(DisplayName = "Renew Production CSID with valid CSR")]
        public async Task RenewalCsidValid()
        {
            // Arrange
            const string validOtp = "123345";
            var creds = GetRenewalCredentials();
            var client = GetZatcaClient();

            // Act
            var request = await GetFileContent<CsrRequest>(@"Resources\Requests\CsrRequest_Valid.json");
            var response = await client.RenewComplianceCsid(request, creds, validOtp);

            // Assert
            Assert.Equal(ResponseStatus.Success, response.Status);
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Result);
            var result = response.Result;

            Assert.Equal(Constants.Disposition.ISSUED, result.DispositionMessage);
            Assert.True(result.RequestId > 0, "The requestID was not a positive long.");
            Assert.NotNull(result.BinarySecurityToken);
            Assert.NotNull(result.Secret);
            Assert.Null(result.Errors);
        }

        #region Helpers

        private Credentials GetReportingCredentials() =>
            new(_options.Reporting?.Username ?? _options.Default?.Username ?? "",
                _options.Reporting?.Password ?? _options.Default?.Password ?? "");

        private Credentials GetClearanceCredentials() =>
            new(_options.Clearance?.Username ?? _options.Default?.Username ?? "",
                _options.Clearance?.Password ?? _options.Default?.Password ?? "");

        private Credentials GetComplianceCredentials() =>
            new(_options.Compliance?.Username ?? _options.Default?.Username ?? "",
                _options.Compliance?.Password ?? _options.Default?.Password ?? "");

        private Credentials GetOnboardingCredentials() =>
            new(_options.Onboarding?.Username ?? _options.Default?.Username ?? "",
                _options.Onboarding?.Password ?? _options.Default?.Password ?? "");

        private Credentials GetRenewalCredentials() =>
            new(_options.Renewal?.Username ?? _options.Default?.Username ?? "",
                _options.Renewal?.Password ?? _options.Default?.Password ?? "");

        private static ZatcaClient GetZatcaClient() => new(env:  Env.Sandbox, _httpClient);

        private static async Task<T> GetFileContent<T>(string fileName)
        {
            string fileContent = await File.ReadAllTextAsync(fileName);
            return JsonConvert.DeserializeObject<T>(fileContent);
        }

        #endregion
    }
}
