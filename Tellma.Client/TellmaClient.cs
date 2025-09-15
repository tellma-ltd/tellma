using IdentityModel.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Client
{
    /// <summary>
    /// Provides managed access to all the web API of Tellma ERP.
    /// </summary>
    /// <remarks>Scope: Create at least one <see cref="TellmaClient"/> per client Id.</remarks>
    public class TellmaClient : IClientBehavior, IAccessTokenFactory, IHttpClientFactory
    {
        /// <summary>
        /// The universal <see cref="HttpClient"/> used when no override is supplied.
        /// </summary>
        private static HttpClient _httpClient = null;

        /// <summary>
        /// If supplied in one of the constructor overloads, use always.
        /// </summary>
        private readonly HttpClient _httpClientOverride;

        /// <summary>
        /// The base URL of the Tellma installation. <br/> Example: "https://web.tellma.com".
        /// </summary>
        private readonly string _baseUrl;

        /// <summary>
        /// The factory used to retrieve the access token, if an implementation is not provided
        /// a <see cref="DefaultAccessTokenFactory"/> is used.
        /// </summary>
        private readonly IAccessTokenFactory _accessTokenFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TellmaClient"/> class with the default <see cref="IAccessTokenFactory"/>.
        /// </summary>
        /// <param name="baseUrl">The base URL of the Tellma installation. <br/> Example: "https://web.tellma.com".</param>
        /// <param name="authorityUrl">The base URL of the OIDC identity server trusted by the Tellma API.</param>
        /// <param name="clientId">The client Id used to authenticate with the Tellma API.</param>
        /// <param name="clientSecret">The client secret used to authenticate with the tellma API.</param>
        public TellmaClient(string baseUrl, string authorityUrl, string clientId, string clientSecret)
        {
            _baseUrl = baseUrl;
            _accessTokenFactory = new DefaultAccessTokenFactory(authorityUrl, clientId, clientSecret, this);
        }

        public TellmaClient(HttpClient httpClient, IAccessTokenFactory accessTokenFactory)
        {
            _baseUrl = "https://localhost";
            _httpClientOverride = httpClient;
            _accessTokenFactory = accessTokenFactory;
        }

        #region HttpClient

        public HttpClient CreateClient(string _)
        {
            return _httpClientOverride ?? (_httpClient ??= new HttpClient());
        }

        #endregion

        #region Access Token

        public Task<string> GetValidAccessToken(CancellationToken cancellation = default)
            => _accessTokenFactory.GetValidAccessToken(cancellation);

        private class DefaultAccessTokenFactory : IAccessTokenFactory
        {
            /// <summary>
            /// The base URL of the OIDC identity server trusted by the Tellma API.
            /// </summary>
            private readonly string _authorityUrl = string.Empty;

            /// <summary>
            /// The client Id used to authenticate with the Tellma API.
            /// </summary>
            private readonly string _clientId;

            /// <summary>
            /// The client secret used to authenticate with the tellma API.
            /// </summary>
            private readonly string _clientSecret;

            /// <summary>
            /// Used to retrieve the <see cref="HttpClient"/>.
            /// </summary>
            private readonly IHttpClientFactory _clientFactory;

            /// <summary>
            /// Initializes a new instance of the <see cref="DefaultAccessTokenFactory"/> class.
            /// </summary>
            /// <param name="authorityUrl">The base URL of the OIDC identity server trusted by the Tellma API.</param>
            /// <param name="clientId">The client Id used to authenticate with the Tellma API.</param>
            /// <param name="clientSecret">The client secret used to authenticate with the tellma API.</param>
            /// <param name="clientFactory">Used to retrieve the <see cref="HttpClient"/>.</param>
            public DefaultAccessTokenFactory(string authorityUrl, string clientId, string clientSecret, IHttpClientFactory clientFactory)
            {
                _authorityUrl = authorityUrl;
                _clientId = clientId;
                _clientSecret = clientSecret;
                _clientFactory = clientFactory;
            }

            /// <summary>
            /// The current access token.
            /// </summary>
            private string _accessToken;

            /// <summary>
            /// The expiry <see cref="DateTimeOffset"/> of the current access token.
            /// </summary>
            private DateTimeOffset _accessTokenExpiry = DateTimeOffset.MinValue;

            /// <summary>
            /// Caches the discovery document of the identity server.
            /// </summary>
            private static DiscoveryCache _discoveryCache = null;

            /// <summary>
            /// True if the access token has expired, False otherwise.
            /// </summary>
            private bool AccessTokenHasExpired => _accessTokenExpiry < DateTimeOffset.Now;

            /// <summary>
            /// Returns the current access token, refreshing it first if it's expired.
            /// </summary>
            /// <param name="cancellation">The cancellation instruction.</param>
            /// <returns>The <see cref="Task"/> object representing the asynchronous operation.</returns>
            /// <remarks>This method is thread safe.</remarks>
            public async Task<string> GetValidAccessToken(CancellationToken cancellation = default)
            {
                // If expired, grab a new one
                await RefreshAccessTokenIfExpiredAsync(cancellation);
                return _accessToken;
            }

            /// <summary>
            /// Refreshes the current access token from the identity server if it is expired.
            /// </summary>
            /// <param name="cancellation">The cancellation instruction.</param>
            /// <returns>The <see cref="Task"/> object representing the asynchronous operation.</returns>
            /// <remarks>This method is thread safe.</remarks>
            private async Task RefreshAccessTokenIfExpiredAsync(CancellationToken cancellation = default)
            {
                // If expired, grab a new one
                if (AccessTokenHasExpired)
                {
                    using var _ = await SingleThreadedScope.Create(nameof(_accessToken), cancellation);

                    // Second OCD check inside the single-threaded scope
                    if (AccessTokenHasExpired)
                    {
                        await RefreshAccessTokenImpl(cancellation);
                    }
                }
            }

            /// <summary>
            /// Refreshes <see cref="_accessToken"/> from the identity server.
            /// </summary>
            /// <param name="cancellation">The cancellation instruction.</param>
            /// <returns>The <see cref="Task"/> object representing the asynchronous operation.</returns>
            /// <remarks>This method is NOT thread safe.</remarks>
            private async Task RefreshAccessTokenImpl(CancellationToken cancellation = default)
            {
                if (string.IsNullOrWhiteSpace(_authorityUrl))
                {
                    throw new InvalidOperationException();
                }

                if (string.IsNullOrWhiteSpace(_clientId))
                {
                    throw new InvalidOperationException();
                }

                if (string.IsNullOrWhiteSpace(_clientSecret))
                {
                    throw new InvalidOperationException();
                }

                // Load the discovery document and get the token endpoint
                _discoveryCache ??= new DiscoveryCache(_authorityUrl, () => GetHttpClient());
                var discoveryDoc = await _discoveryCache.GetAsync();
                if (discoveryDoc.IsError)
                {
                    throw new TellmaException(discoveryDoc.Error ?? "An error occurred while loading the discovery document.");
                }
                var tokenEndpoint = discoveryDoc.TokenEndpoint;

                // Get the access token from the token endpoint
                var tokenRequest = new ClientCredentialsTokenRequest
                {
                    Address = tokenEndpoint,
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                };

                var tokenResponse = await GetHttpClient()
                    .RequestClientCredentialsTokenAsync(tokenRequest, cancellation)
                    .ConfigureAwait(false);

                if (tokenResponse.IsError)
                {
                    throw new TellmaException(tokenResponse.Error ?? "An error occurred while loading the access token.");
                }

                // Set the access token and access token expiry date (subtract 15 seconds for robustness)
                _accessToken = tokenResponse.AccessToken;
                _accessTokenExpiry = DateTimeOffset.Now.AddSeconds(tokenResponse.ExpiresIn - 15);
            }

            /// <summary>
            /// Helper method.
            /// </summary>
            /// <returns>The <see cref="HttpClient"/> from the <see cref="IHttpClientFactory"/>.</returns>
            private HttpClient GetHttpClient()
            {
                return _clientFactory.CreateClient(null);
            }
        }

        #endregion

        #region API URL

        public IEnumerable<string> GetBaseUrlSteps()
        {
            yield return _baseUrl;
            yield return "api";
        }

        #endregion

        #region Clients

        private readonly ConcurrentDictionary<int, ApplicationClientBehavior> _appClients =
            new ConcurrentDictionary<int, ApplicationClientBehavior>();

        public ApplicationClientBehavior Application(int tenantId) =>
            _appClients.GetOrAdd(tenantId, _ => new ApplicationClientBehavior(tenantId, this, this, this));

        private AdminClientBehavior _adminClient = null;

        public AdminClientBehavior Admin => _adminClient ??= new AdminClientBehavior(this, this, this);

        #endregion

        #region Root API

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, Request request = null, CancellationToken cancellation = default)
        {                // To prevent null reference exceptions
            request ??= Request.Default;

            // Add access token
            string token = await GetValidAccessToken(cancellation);
            msg.SetBearerToken(token);

            // Add headers
            msg.Headers.Add(RequestHeaders.Today, DateTime.Today.ToString("yyyy-MM-dd"));
            msg.Headers.Add(RequestHeaders.ApiVersion, "1.0");

            // Add query parameters
            var cultureString = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var uriBldr = new UriBuilder(msg.RequestUri);
            uriBldr.AddQueryParameter("ui-culture", cultureString);
            msg.RequestUri = uriBldr.Uri;

            // Send request
            HttpClient client = CreateClient("");
            var responseMsg = await client.SendAsync(msg, cancellation);

            // Return response
            return responseMsg;
        }

        public async Task<CompaniesForClient> MyCompanies(CancellationToken cancellation = default)
        {
            // Prepare the URL
            var url = string.Join('/', GetBaseUrlSteps().Concat(new string[] { "companies", "client" }));

            // Prepare the message
            var method = HttpMethod.Get;
            using var msg = new HttpRequestMessage(method, url);

            // Send the message
            using var httpResponse = await SendAsync(msg).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
          //   var text = await httpResponse.Content.ReadAsStringAsync();
            var result = await httpResponse.Content
                .ReadAsAsync<CompaniesForClient>(cancellation)
                .ConfigureAwait(false);

            return result;
        }

        #endregion

        public class ApplicationClientBehavior : IClientBehavior
        {
            private readonly int _tenantId;
            private readonly IAccessTokenFactory _tokenFactory;
            private readonly IHttpClientFactory _clientFactory;
            private readonly IBaseUrlAccessor _baseUrlAccessor;

            internal ApplicationClientBehavior(int tenantId, IAccessTokenFactory tokenFactory, IHttpClientFactory clientFactory, IBaseUrlAccessor baseUrlAccessor)
            {
                _tenantId = tenantId;
                _tokenFactory = tokenFactory;
                _clientFactory = clientFactory;
                _baseUrlAccessor = baseUrlAccessor;
            }

            public IEnumerable<string> GetBaseUrlSteps()
            {
                foreach (var step in _baseUrlAccessor.GetBaseUrlSteps())
                {
                    yield return step;
                }

                // yield return _tenantId.ToString();
            }

            public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, Request request, CancellationToken cancellation = default)
            {
                // To prevent null reference exceptions
                request ??= Request.Default;

                // Add access token
                string token = await _tokenFactory.GetValidAccessToken(cancellation);
                msg.SetBearerToken(token);

                // Add headers
                msg.Headers.Add(RequestHeaders.TenantId, _tenantId.ToString());
                msg.Headers.Add(RequestHeaders.Today, DateTime.Today.ToString("yyyy-MM-dd"));
                msg.Headers.Add(RequestHeaders.Calendar, request.Calendar.ToString());
                msg.Headers.Add(RequestHeaders.ApiVersion, "1.0");

                // Add query parameters
                var cultureString = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var tenantId = _tenantId.ToString();

                var uriBldr = new UriBuilder(msg.RequestUri);
                uriBldr.AddQueryParameter("ui-culture", cultureString);
                uriBldr.AddQueryParameter("tenant-id", tenantId);
                msg.RequestUri = uriBldr.Uri;

                // Send request
                HttpClient client = _clientFactory.CreateClient();
                var responseMsg = await client.SendAsync(msg, cancellation);

                // Return response
                return responseMsg;
            }

            #region Clients

            private AccountsClient _accounts;
            public AccountsClient Accounts => _accounts ??= new AccountsClient(this);

            private AccountTypesClient _accountTypes;
            public AccountTypesClient AccountTypes => _accountTypes ??= new AccountTypesClient(this);

            private AdminUsersClient _adminUsers;
            public AdminUsersClient AdminUsers => _adminUsers ??= new AdminUsersClient(this);

            private AgentDefinitionsClient _agentDefinitions;
            public AgentDefinitionsClient AgentDefinitions => _agentDefinitions ??= new AgentDefinitionsClient(this);

            private readonly ConcurrentDictionary<int, AgentsClient> _agents = new ConcurrentDictionary<int, AgentsClient>();
            public AgentsClient Agents(int definitionId) => _agents.GetOrAdd(definitionId, defId => new AgentsClient(defId, this));

            private AgentsGenericClient _agentsGeneric;
            public AgentsGenericClient AgentsGeneric => _agentsGeneric ??= new AgentsGenericClient(this);

            private CurrenciesClient _currencies;
            public CurrenciesClient Currencies => _currencies ??= new CurrenciesClient(this);

            private CentersClient _centers;
            public CentersClient Centers => _centers ??= new CentersClient(this);

            private DashboardDefinitionsClient _dashboardDefinitions;
            public DashboardDefinitionsClient DashboardDefinitions => _dashboardDefinitions ??= new DashboardDefinitionsClient(this);

            private DetailsEntriesClient _detailsEntries;
            public DetailsEntriesClient DetailsEntries => _detailsEntries ??= new DetailsEntriesClient(this);

            private DocumentDefinitionsClient _documentDefinitions;
            public DocumentDefinitionsClient DocumentDefinitions => _documentDefinitions ??= new DocumentDefinitionsClient(this);

            private readonly ConcurrentDictionary<int, DocumentsClient> _documents = new ConcurrentDictionary<int, DocumentsClient>();
            public DocumentsClient Documents(int definitionId) => _documents.GetOrAdd(definitionId, defId => new DocumentsClient(defId, this));

            private DocumentsGenericClient _documentsGeneric;
            public DocumentsGenericClient DocumentsGeneric => _documentsGeneric ??= new DocumentsGenericClient(this);

            private EmailsClient _emails;
            public EmailsClient Emails => _emails ??= new EmailsClient(this);

            private EntryTypesClient _entryTypes;
            public EntryTypesClient EntryTypes => _entryTypes ??= new EntryTypesClient(this);

            private ExchangeRatesClient _exchangeRates;
            public ExchangeRatesClient ExchangeRates => _exchangeRates ??= new ExchangeRatesClient(this);

            private IdentityServerClientsClient _identityServerClients;
            public IdentityServerClientsClient IdentityServerClients => _identityServerClients ??= new IdentityServerClientsClient(this);

            private IdentityServerUsersClient _identityServerUsers;
            public IdentityServerUsersClient IdentityServerUsers => _identityServerUsers ??= new IdentityServerUsersClient(this);

            private IfrsConceptsClient _ifrsConcepts;
            public IfrsConceptsClient IfrsConcepts => _ifrsConcepts ??= new IfrsConceptsClient(this);

            private LineDefinitionsClient _lineDefinitions;
            public LineDefinitionsClient LineDefinitions => _lineDefinitions ??= new LineDefinitionsClient(this);

            private LookupDefinitionsClient _lookupDefinitions;
            public LookupDefinitionsClient LookupDefinitions => _lookupDefinitions ??= new LookupDefinitionsClient(this);

            private readonly ConcurrentDictionary<int, LookupsClient> _lookups = new ConcurrentDictionary<int, LookupsClient>();
            public LookupsClient Lookups(int definitionId) => _lookups.GetOrAdd(definitionId, defId => new LookupsClient(defId, this));

            private LookupsGenericClient _lookupsGeneric;
            public LookupsGenericClient LookupsGeneric => _lookupsGeneric ??= new LookupsGenericClient(this);

            private PrintingTemplatesClient _printingTemplates;
            public PrintingTemplatesClient PrintingTemplates => _printingTemplates ??= new PrintingTemplatesClient(this);

            private OutboxClient _outbox;
            public OutboxClient Outbox => _outbox ??= new OutboxClient(this);

            private ReportDefinitionsClient _reportDefinitions;
            public ReportDefinitionsClient ReportDefinitions => _reportDefinitions ??= new ReportDefinitionsClient(this);

            private ResourceDefinitionsClient _resourceDefinitions;
            public ResourceDefinitionsClient ResourceDefinitions => _resourceDefinitions ??= new ResourceDefinitionsClient(this);

            private readonly ConcurrentDictionary<int, ResourcesClient> _resources = new ConcurrentDictionary<int, ResourcesClient>();
            public ResourcesClient Resources(int definitionId) => _resources.GetOrAdd(definitionId, defId => new ResourcesClient(defId, this));

            private ResourcesGenericClient _resourcesGeneric;
            public ResourcesGenericClient ResourcesGeneric => _resourcesGeneric ??= new ResourcesGenericClient(this);

            private RolesClient _roles;
            public RolesClient Roles => _roles ??= new RolesClient(this);

            private SmsMessagesClient _smsMessages;
            public SmsMessagesClient SmsMessages => _smsMessages ??= new SmsMessagesClient(this);

            private UnitsClient _units;
            public UnitsClient Units => _units ??= new UnitsClient(this);

            private UsersClient _users;
            public UsersClient Users => _users ??= new UsersClient(this);

            private DefinitionsClient _definitions;
            public DefinitionsClient Definitions => _definitions ??= new DefinitionsClient(this);

            private GeneralSettingsClient _generalSettings;
            public GeneralSettingsClient GeneralSettings => _generalSettings ??= new GeneralSettingsClient(this);

            private FinancialSettingsClient _financialSettings;
            public FinancialSettingsClient FinancialSettings => _financialSettings ??= new FinancialSettingsClient(this);

            #endregion
        }

        public class AdminClientBehavior : IClientBehavior
        {
            private readonly IAccessTokenFactory _tokenFactory;
            private readonly IHttpClientFactory _clientFactory;
            private readonly IBaseUrlAccessor _baseUrlAccessor;

            internal AdminClientBehavior(IAccessTokenFactory tokenFactory, IHttpClientFactory clientFactory, IBaseUrlAccessor baseUrlAccessor)
            {
                _tokenFactory = tokenFactory;
                _clientFactory = clientFactory;
                _baseUrlAccessor = baseUrlAccessor;
            }

            public IEnumerable<string> GetBaseUrlSteps()
            {
                foreach (var step in _baseUrlAccessor.GetBaseUrlSteps())
                {
                    yield return step;
                }
            }

            public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, Request request, CancellationToken cancellation = default)
            {               
                // Add access token
                string token = await _tokenFactory.GetValidAccessToken(cancellation);
                msg.SetBearerToken(token);

                // Add headers
                msg.Headers.Add(RequestHeaders.Today, DateTime.Today.ToString("yyyy-MM-dd"));
                msg.Headers.Add(RequestHeaders.ApiVersion, "1.0");

                // Add query parameters
                var cultureString = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                var uriBldr = new UriBuilder(msg.RequestUri);
                uriBldr.AddQueryParameter("ui-culture", cultureString);
                msg.RequestUri = uriBldr.Uri;

                // Send request
                HttpClient client = _clientFactory.CreateClient();
                var responseMsg = await client.SendAsync(msg, cancellation);

                // Return response
                return responseMsg;
            }
        }
    }

    #region Handlers

    public interface IGlobalVersionsHandler
    {
        /// <summary>
        /// The version of the cached <see cref="GlobalSettingsForClient"/> (Optional).
        /// </summary>
        string GlobalSettingsVersion { get; }
        Task OnStaleGlobalSettings();
    }

    public interface IApplicationVersionsHandler
    {
        string SettingsVersion { get; }
        string DefinitionsVersion { get; }
        string UserSettingsVersion { get; }
        string PermissionsVersion { get; }

        Task OnStaleSettings(int tenantId);
        Task OnStaleDefinitions(int tenantId);
        Task OnStaleUserSettings(int tenantId);
        Task OnStalePermissions(int tenantId);
    }

    public interface IAdminVersionsHandler
    {
        string AdminPermissionsVersion { get; }
        string AdminUserSettingsVersion { get; }
        Task OnStaleAdminPermissions();
        Task OnStaleAdminUserSettings();
    }

    public class NullVersionsHandler : IApplicationVersionsHandler, IAdminVersionsHandler, IGlobalVersionsHandler
    {
        public string SettingsVersion => null;

        public string DefinitionsVersion => null;

        public string UserSettingsVersion => null;

        public string PermissionsVersion => null;

        public string GlobalSettingsVersion => null;

        public string AdminPermissionsVersion => null;

        public string AdminUserSettingsVersion => null;

        public Task OnStaleSettings(int tenantId) => Task.CompletedTask;

        public Task OnStaleDefinitions(int tenantId) => Task.CompletedTask;

        public Task OnStaleUserSettings(int tenantId) => Task.CompletedTask;

        public Task OnStalePermissions(int tenantId) => Task.CompletedTask;

        public Task OnStaleGlobalSettings() => Task.CompletedTask;

        public Task OnStaleAdminPermissions() => Task.CompletedTask;

        public Task OnStaleAdminUserSettings() => Task.CompletedTask;
    }

    #endregion
}
