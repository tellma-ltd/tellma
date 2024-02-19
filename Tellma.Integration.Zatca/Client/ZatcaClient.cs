using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tellma.Integration.Zatca
{
    public class ZatcaClient
    {
        #region Constants

        const string PRODUCTION_BASE_URL = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal"; // TODO: Replace with production URL
        const string SANDBOX_BASE_URL = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal";

        #endregion

        #region Constructor

        /// <summary>
        /// The base URL of ZATCA API.
        /// </summary>
        private readonly string _baseUrl;

        private readonly IHttpClientFactory _httpClientFactory;

        public ZatcaClient(bool useSandbox, IHttpClientFactory httpClientFactory)
        {
            _baseUrl = useSandbox ? SANDBOX_BASE_URL : PRODUCTION_BASE_URL;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public ZatcaClient(IHttpClientFactory httpClientFactory) : this(useSandbox: false, httpClientFactory)
        {
        }

        public ZatcaClient(bool useSandbox, HttpClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            _baseUrl = useSandbox ? SANDBOX_BASE_URL : PRODUCTION_BASE_URL;
            _httpClientFactory = new StaticHttpClientFactory(client);
        }

        public ZatcaClient(HttpClient client) : this(useSandbox: false, client)
        {
        }

        private class StaticHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;

            public StaticHttpClientFactory(HttpClient client) => _client = client;

            public HttpClient CreateClient(string name) => _client;
        }

        #endregion

        #region API

        /// <summary>
        /// Calls the e-Invoice reporting API.
        /// <para/>
        /// Note: A 400 response will not throw an exception.
        /// </summary>
        public async Task<Response<ReportingResponse>> ReportInvoice(ReportingRequest request, Credentials creds, CancellationToken cancellation = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (creds is null)
            {
                throw new ArgumentNullException(nameof(creds));
            }

            string url = _baseUrl + "/invoices/reporting/single";

            // Prepare the message
            var method = HttpMethod.Post;
            var msg = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(request)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, creds, cancellation).ConfigureAwait(false);

            // Return the Response
            var status = (ResponseStatus)httpResponse.StatusCode;
            var result = await TryParseResult<ReportingResponse>(httpResponse, cancellation);

            return new(status, result);
        }

        /// <summary>
        /// Calls the e-Invoice clearance API.
        /// <para/>
        /// Note: A 400 response will not throw an exception.
        /// </summary>
        public async Task<Response<ClearanceResponse>> ClearInvoice(ClearanceRequest request, Credentials creds, bool activeClearance = true, CancellationToken cancellation = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (creds is null)
            {
                throw new ArgumentNullException(nameof(creds));
            }

            string url = _baseUrl + "/invoices/clearance/single";

            // Prepare the message
            var method = HttpMethod.Post;
            var msg = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(request)
            };

            // Add clearance header
            msg.Headers.Add("Clearance-Status", activeClearance ? "1" : "0");

            // Send the message
            using var httpResponse = await SendAsync(msg, creds, cancellation).ConfigureAwait(false);

            // Return the Response
            var status = (ResponseStatus)httpResponse.StatusCode;
            var result = await TryParseResult<ClearanceResponse>(httpResponse, cancellation);

            return new(status, result);
        }

        /// <summary>
        /// Calls the e-Invoice Compliance CSID API.
        /// </summary>
        public async Task<Response<CsidResponse>> CreateComplianceCsid(CsrRequest request, string otp, CancellationToken cancellation = default)
        {
            // Validate arguments
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentException($"'{nameof(otp)}' cannot be null or whitespace.", nameof(otp));

            string url = _baseUrl + "/compliance";

            // Prepare the message
            var method = HttpMethod.Post;
            var msg = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(request)
            };

            // Add OTP header
            msg.Headers.Add("Otp", otp);

            // Send the message
            using var httpResponse = await SendAsync(msg, null, cancellation).ConfigureAwait(false);

            // Return the Response
            var status = (ResponseStatus)httpResponse.StatusCode;
            var result = await TryParseResult<CsidResponse>(httpResponse, cancellation);

            return new(status, result);
        }

        /// <summary>
        /// Calls the e-Invoice Compliance Invoice API.
        /// </summary>
        public async Task<Response<ComplianceCheckResponse>> CheckInvoiceCompliance(ComplianceCheckRequest request, Credentials creds, CancellationToken cancellation = default)
        {
            // Validate arguments
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (creds is null)
                throw new ArgumentNullException(nameof(creds));

            // Base URL
            string url = _baseUrl + "/compliance/invoices";

            // Prepare the message
            var method = HttpMethod.Post;
            var msg = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(request)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, creds, cancellation).ConfigureAwait(false);
            
            // Return the Response
            var status = (ResponseStatus)httpResponse.StatusCode;
            var result = await TryParseResult<ComplianceCheckResponse>(httpResponse, cancellation);

            return new(status, result);
        }

        /// <summary>
        /// Calls the Production CSID (Onboarding) API.
        /// </summary>
        public async Task<Response<CsidResponse>> CreateProductionCsid(CreateProductionCsidRequest request, Credentials creds, CancellationToken cancellation = default)
        {
            // Validate arguments
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (creds is null)
                throw new ArgumentNullException(nameof(creds));

            // Base URL
            string url = _baseUrl + "/production/csids";

            // Prepare the message
            var method = HttpMethod.Post;
            var msg = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(request)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, creds, cancellation).ConfigureAwait(false);

            // Return the Response
            var status = (ResponseStatus)httpResponse.StatusCode;
            var result = await TryParseResult<CsidResponse>(httpResponse, cancellation);

            return new(status, result);
        }

        /// <summary>
        /// Calls the Production CSID (Renewal) API.
        /// </summary>
        public async Task<Response<CsidResponse>> RenewComplianceCsid(CsrRequest request, Credentials creds, string otp, CancellationToken cancellation = default)
        {
            // Validate arguments
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (creds is null)
                throw new ArgumentNullException(nameof(creds));

            string url = _baseUrl + "/production/csids";

            // Prepare the message
            var method = HttpMethod.Patch;
            var msg = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(request)
            };

            // Add OTP header
            msg.Headers.Add("Otp", otp);

            // Send the message
            using var httpResponse = await SendAsync(msg, creds, cancellation).ConfigureAwait(false);

            // Return the Response
            var status = (ResponseStatus)httpResponse.StatusCode;
            var result = await TryParseResult<CsidResponse>(httpResponse, cancellation);

            return new(status, result);
        }

        #endregion

        #region Helpers

        private static async Task<T?> TryParseResult<T>(HttpResponseMessage msg, CancellationToken cancellation) where T : class
        {
            if (msg.IsSuccessStatusCode || msg.StatusCode == HttpStatusCode.BadRequest)
            {
                try
                {
                    return await msg.Content
                        .ReadFromJsonAsync<T>(cancellationToken: cancellation)
                        .ConfigureAwait(false);
                }
                catch (JsonException) { }
            }

            return null;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, Credentials? creds, CancellationToken cancellation = default)
        {
            // Add basic authentication header
            if (creds != null)
                msg.Headers.Authorization = new BasicAuthenticationHeaderValue(creds.Username, creds.Password);

            // Add standard headers
            msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            msg.Headers.Add("Accept-Version", "V2");

            // Add language header
            var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            lang = lang == "ar" ? lang : "en"; // Either Arabic or English (defaults to English)
            msg.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(lang));

            // Send request
            HttpClient client = _httpClientFactory.CreateClient(string.Empty);
            var responseMsg = await client.SendAsync(msg, cancellation);

            // Return response
            return responseMsg;
        }

        #endregion
    }
}