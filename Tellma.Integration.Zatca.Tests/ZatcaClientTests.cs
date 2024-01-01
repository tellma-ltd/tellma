using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Reflection;

namespace Tellma.Integration.Zatca.Tests
{
    public partial class ZatcaClientTests
    {
        private static readonly string _sectionName = "ZatcaSandbox";
        private static readonly string _zatcaSandboxBaseUri = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal";

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

        [Fact(DisplayName = "Reporting a single invoice with warnings")]
        public async Task ReportingSingleWithWarnings()
        {
            // Arrange
            var credsFactory = new ConstantCredentialsFactory(_options.Username, _options.Password);
            var client = new ZatcaClient(_zatcaSandboxBaseUri, credsFactory);

            // Act
            var request = await GetFileContent<ReportingRequest>("SimplifiedInvoiceRequest_ValidWithWarnings.json");
            var response = await client.ReportSingle(request);

            // Assert
            Assert.Equal(Constants.ReportingStatus.REPORTED, response.ReportingStatus);
            
            var validationResults = response.ValidationResults;
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

        [Fact(DisplayName = "Reporting a single invoice with errors")]
        public async Task ReportingSingleWithErrors()
        {
            // Arrange
            var credsFactory = new ConstantCredentialsFactory(_options.Username, _options.Password);
            var client = new ZatcaClient(_zatcaSandboxBaseUri, credsFactory);

            // Act
            var request = await GetFileContent<ReportingRequest>("SimplifiedInvoiceRequest_Invalid.json");
            var response = await client.ReportSingle(request);

            // Assert
            Assert.Equal(Constants.ReportingStatus.NOT_REPORTED, response.ReportingStatus);

            var validationResults = response.ValidationResults;
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

        [Fact(DisplayName = "Reporting a single invoice with invalid credenials")]
        public async Task ReportingSingleWithInvalidCredentials()
        {
            // Arrange
            var credsFactory = new ConstantCredentialsFactory("Invalid_username", "Invalid_password");
            var client = new ZatcaClient(_zatcaSandboxBaseUri, credsFactory);

            // Assert
            await Assert.ThrowsAsync<ZatcaAuthenticationException>(async () =>
            {
                // Act
                var request = await GetFileContent<ReportingRequest>("SimplifiedInvoiceRequest_ValidWithWarnings.json");
                await client.ReportSingle(request);
            });
        }

        [Fact(DisplayName = "Clearing a single invoice")]
        public async Task ClearingSingle()
        {
            // Arrange
            var credsFactory = new ConstantCredentialsFactory(_options.Username, _options.Password);
            var client = new ZatcaClient(_zatcaSandboxBaseUri, credsFactory);

            // Act
            var request = await GetFileContent<ClearanceRequest>("StandardInvoiceRequest_Valid.json");
            var response = await client.ClearSingle(request);

            // Assert
            Assert.Equal(Constants.ClearanceStatus.CLEARED, response.ClearanceStatus);
            Assert.NotNull(response.ClearedInvoice);

            var validationResults = response.ValidationResults;
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

        [Fact(DisplayName = "Clearing a single invoice with inactive clearance")]
        public async Task ClearingSingleWithInactiveClearance()
        {
            // Arrange
            var credsFactory = new ConstantCredentialsFactory(_options.Username, _options.Password);
            var client = new ZatcaClient(_zatcaSandboxBaseUri, credsFactory);

            // Assert
            await Assert.ThrowsAsync<ZatcaClearanceDeactivatedException>(async () =>
            {
                // Act
                var request = await GetFileContent<ClearanceRequest>("StandardInvoiceRequest_Valid.json");
                var response = await client.ClearSingle(request, activeClearance: false);
            });
        }

        private static async Task<T> GetFileContent<T>(string fileName)
        {
            string fileContent = await File.ReadAllTextAsync($@"Resources\{fileName}");
            return JsonConvert.DeserializeObject<T>(fileContent);
        }

        #region Credentials Factory

        public class ConstantCredentialsFactory : ICredentialsFactory
        {
            private readonly string _username;
            private readonly string _password;

            public ConstantCredentialsFactory(string? username, string? password)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new InvalidOperationException($"Running {nameof(ZatcaClientTests)} requires a property '{nameof(ZatcaSandboxOptions.Username)}' in the '{_sectionName}' section in the Test project's user secrets");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException($"Running {nameof(ZatcaClientTests)} requires a property '{nameof(ZatcaSandboxOptions.Password)}' in the '{_sectionName}' section in the Test project's user secrets");

                }

                _username = username;
                _password = password;
            }

            public Task<(string username, string password)> GetCredentials(CancellationToken cancellation)
            {
                return Task.FromResult((_username, _password));
            }
        }

        #endregion
    }
}