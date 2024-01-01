using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Tellma.Integration.Zatca
{
    public class ZatcaClient
    {
        #region HttpClient

        /// <summary>
        /// The universal <see cref="HttpClient"/> used to call the Zatca API
        /// </summary>
        private static HttpClient? _httpClient;

        private static readonly object _httpClientLock = new();

        /// <summary>
        /// Initializes the universal <see cref="HttpClient"/> if not already initialized and returns it.
        /// </summary>
        private static HttpClient GetClient()
        {
            if (_httpClient == null)
            {
                lock (_httpClientLock)
                {
                    _httpClient ??= new HttpClient();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _httpClient.DefaultRequestHeaders.Add("Accept-Version", "V2");
                }
            }

            return _httpClient;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// The base URL of Zatca API.
        /// </summary>
        private readonly string _baseUrl;

        /// <summary>
        /// The factory used to retrieve the username and password, if an implementation is not provided
        /// a <see cref="DefaultCredentialsFactory"/> is used.
        /// </summary>
        private readonly ICredentialsFactory _credentialsFactory;

        public ZatcaClient(string baseUrl, ICredentialsFactory credentialsFactory)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException($"'{nameof(baseUrl)}' cannot be null or whitespace.", nameof(baseUrl));
            }

            _baseUrl = baseUrl;
            _credentialsFactory = credentialsFactory ?? throw new ArgumentNullException(nameof(credentialsFactory));
        }

        public ZatcaClient()
        {
            _baseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal"; // TODO: Replace with production URL
            _credentialsFactory = new DefaultCredentialsFactory(); // 
        }

        #endregion

        #region API

        public async Task<ReportingResponse> ReportSingle(ReportingRequest request, CancellationToken cancellation = default)
        {
            string url = _baseUrl + "/invoices/reporting/single";

            // Prepare the message
            var method = HttpMethod.Post;
            var msg = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(request)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, cancellation).ConfigureAwait(false);
            await EnsureSuccess(httpResponse);

            var result = await httpResponse.Content
                .ReadFromJsonAsync<ReportingResponse>(cancellationToken: cancellation)
                .ConfigureAwait(false);

            return result ??
                // Should not hit this error if we did our error handling correctly
                throw new InvalidOperationException($"Could not deserialize the response, status code {httpResponse.StatusCode}.");
        }

        public async Task<ClearanceResponse> ClearSingle(ClearanceRequest request, bool activeClearance = true, CancellationToken cancellation = default)
        {
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
            using var httpResponse = await SendAsync(msg, cancellation).ConfigureAwait(false);
            await EnsureSuccess(httpResponse);

            var result = await httpResponse.Content
                .ReadFromJsonAsync<ClearanceResponse>(cancellationToken: cancellation)
                .ConfigureAwait(false);

            return result ??
                // Should not hit this error if we did our error handling correctly
                throw new InvalidOperationException($"Could not deserialize the response, status code {httpResponse.StatusCode}.");
        }

        #endregion

        #region Helpers

        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            // Handle all known status codes that Zatca returns
            switch (response.StatusCode)
            {
                case HttpStatusCode.RedirectMethod:
                    throw new ZatcaClearanceDeactivatedException();

                case HttpStatusCode.Unauthorized:
                    throw new ZatcaAuthenticationException();

                case HttpStatusCode.InternalServerError:
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    throw new ZatcaInternalException(errorMsg);
            }

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.BadRequest) // 400 will indicate failure in the response body
            {
                // Future proofing
                throw new ZatcaException($"Unhandled status code from Zatca {response.StatusCode}.", isTransient: true);
            }
        }

        private Task<(string username, string password)> GetCredentials(CancellationToken cancellation) => _credentialsFactory.GetCredentials(cancellation);

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, CancellationToken cancellation = default)
        {
            // Add basic authentication header
            var (username, password) = await GetCredentials(cancellation);
            msg.Headers.Authorization = new BasicAuthenticationHeaderValue(username, password);

            // Add language header
            var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            lang = lang == "ar" ? lang : "en"; // Either Arabic or English (defaults to English)
            msg.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(lang));

            // Send request
            HttpClient client = GetClient();
            var responseMsg = await client.SendAsync(msg, cancellation);

            // Return response
            return responseMsg;
        }

        #endregion
    }
}