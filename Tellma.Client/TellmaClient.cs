using IdentityModel.Client;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Model.Common;

namespace Tellma.Client
{
    /*
        [Remaining Tasks]
- Organize the Response classes
- Organize the Client project
- Test saving entities with attachments
- Add remaining API methods
- Add method documentation
- Add project version 
- Publish on NuGet
     */

    public interface IAccessTokenFactory
    {
        Task<string> GetValidAccessToken(CancellationToken cancellation = default);
    }

    public interface IBaseUrlAccessor
    {
        IEnumerable<string> GetBaseUrlSteps();
    }

    public interface IHttpRequestSender
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, Request req, CancellationToken cancellation = default);
    }

    public interface IClientBehavior : IBaseUrlAccessor, IHttpRequestSender
    {
    }

    /// <summary>
    /// Provides managed access to all the web API of Tellma ERP.
    /// </summary>
    /// <remarks>Scope: Create at least one <see cref="TellmaClient"/> per client Id.</remarks>
    public class TellmaClient : IBaseUrlAccessor, IAccessTokenFactory, IHttpClientFactory
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

        #region TenantId

        private readonly object _defaultTenantIdLock = new object();
        private int _defaultTenantId;

        public int DefaultTenantId
        {
            get
            {
                lock (_defaultTenantIdLock)
                {
                    return _defaultTenantId;
                }
            }
            set
            {
                lock (_defaultTenantIdLock)
                {
                    _defaultTenantId = value;
                }
            }
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
            {                // To prevent null reference exceptions
                request ??= Request.Default;

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

    /// <summary>
    /// Base class for all controller clients.
    /// </summary>
    public abstract class ClientBase
    {
        private readonly IClientBehavior _behavior;

        internal ClientBase(IClientBehavior behavior)
        {
            _behavior = behavior;
        }

        protected virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, Request req, CancellationToken cancellation = default)
            => await _behavior.SendAsync(msg, req, cancellation);

        protected abstract string ControllerPath { get; }

        private IEnumerable<string> GetBaseUrlSteps()
        {
            foreach (var step in _behavior.GetBaseUrlSteps())
            {
                yield return step;
            }

            yield return ControllerPath;
        }

        protected UriBuilder GetActionUrlBuilder(params string[] actionPath)
        {
            var steps = GetBaseUrlSteps().Concat(actionPath);
            var url = string.Join('/', steps);

            return new UriBuilder(url);
        }
    }

    public abstract class FactClientBase<TEntity> : ClientBase where TEntity : Entity
    {
        #region Lifecycle

        internal FactClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntitiesResult<TEntity>> GetEntities(Request<GetArguments> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder();

            // Add query parameters
            var args = request?.Arguments ?? new GetArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            urlBldr.AddQueryParameter(nameof(args.OrderBy), args.OrderBy);
            urlBldr.AddQueryParameter(nameof(args.Search), args.Search);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());
            urlBldr.AddQueryParameter(nameof(args.CountEntities), args.CountEntities.ToString());

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content.ReadAsAsync<GetResponse<TEntity>>(cancellation).ConfigureAwait(false);

            var entities = response.Result.ToList();
            var totalCount = response.TotalCount;
            var relatedEntities = response.RelatedEntities;

            Unflatten(entities, relatedEntities, cancellation);

            var result = new EntitiesResult<TEntity>(entities, totalCount);
            return result;
        }

        public virtual async Task<FactResult> GetFact(Request<FactArguments> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("fact");

            // Add query parameters
            var args = request?.Arguments ?? new FactArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.OrderBy), args.OrderBy);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());
            urlBldr.AddQueryParameter(nameof(args.CountEntities), args.CountEntities.ToString());

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<GetFactResponse>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result.ToList();
            var totalCount = response.TotalCount;

            var result = new FactResult(entities, totalCount);
            return result;
        }

        public virtual async Task<AggregateResult> GetAggregate(Request<GetAggregateArguments> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("aggregate");

            // Add query parameters
            var args = request?.Arguments ?? new GetAggregateArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.OrderBy), args.OrderBy);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.Having), args.Having);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<GetAggregateResponse>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result.ToList();
            var ancestors = response.DimensionAncestors.Select(e => new DimensionAncestorsResult(e.Result, e.IdIndex, e.MinIndex));

            var result = new AggregateResult(entities, ancestors);
            return result;
        }

        // TODO: Print API

        #endregion

        #region Helpers

        protected void Unflatten(IEnumerable<TEntity> resultEntities, RelatedEntities relatedEntities, CancellationToken cancellation)
        {
            if (resultEntities == null || !resultEntities.Any())
            {
                return;
            }

            relatedEntities ??= new RelatedEntities();

            // Cache related entities in a fast-to-query data structure
            // Mapping: Collection -> Id -> Entity
            var lookup = new Dictionary<string, Dictionary<object, EntityWithKey>>();
            bool TryGetEntity(string collection, object id, out EntityWithKey result)
            {
                // This function populates lookup with entityes of type in a lazy fashion only when requested
                if (!lookup.TryGetValue(collection, out Dictionary<object, EntityWithKey> entitiesOfType))
                {
                    // Id -> Entity
                    entitiesOfType = new Dictionary<object, EntityWithKey>();

                    // Cache related entities in this collection
                    foreach (var entity in relatedEntities.GetEntities(collection))
                    {
                        entitiesOfType.Add(entity.GetId(), entity);
                    }

                    // Cache the main entities if they are from the same collection
                    if (typeof(TEntity).Name == collection)
                    {
                        // If it's a nav entity then we can safely cast it
                        foreach (var entity in resultEntities.Cast<EntityWithKey>())
                        {
                            entitiesOfType.Add(entity.GetId(), entity);
                        }
                    }

                    lookup.Add(collection, entitiesOfType);
                }

                return entitiesOfType.TryGetValue(id, out result);
            }

            // Recursive function
            void UnflattenInner(Entity entity, TypeDescriptor typeDesc)
            {
                if (entity.EntityMetadata.Flattened)
                {
                    // This has already been unflattened before
                    return;
                }

                entity.EntityMetadata.Flattened = true;

                // Recursively go over the nav properties
                foreach (var prop in typeDesc.NavigationProperties)
                {
                    var navDesc = prop.TypeDescriptor;
                    var navCollection = navDesc.Name;
                    var fkValue = prop.ForeignKey.GetValue(entity);

                    if (fkValue != null && TryGetEntity(navCollection, fkValue, out EntityWithKey relatedEntity))
                    {
                        prop.SetValue(entity, relatedEntity);
                        UnflattenInner(relatedEntity, navDesc);
                    }
                }

                // Recursively go over every entity in the nav collection properties
                foreach (var prop in typeDesc.CollectionProperties)
                {
                    var collectionType = prop.CollectionTypeDescriptor;
                    if (prop.GetValue(entity) is IList collection)
                    {
                        foreach (var obj in collection)
                        {
                            if (obj is Entity relatedEntity)
                            {
                                UnflattenInner(relatedEntity, collectionType);
                            }
                        }
                    }
                }
            }

            // Unflatten every entity in the main list
            var typeDesc = TypeDescriptor.Get<TEntity>();
            foreach (var entity in resultEntities)
            {
                if (entity != null)
                {
                    UnflattenInner(entity, typeDesc);
                    cancellation.ThrowIfCancellationRequested();
                }
            }
        }

        #endregion
    }

    public abstract class FactWithIdClientBase<TEntity, TKey> : FactClientBase<TEntity>
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        internal FactWithIdClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntitiesResult<TEntity>> GetByIds(Request<GetByIdsArguments<TKey>> request, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("by-ids");

            // Add query parameters
            var args = request?.Arguments ?? new GetByIdsArguments<TKey>();
            if (args.I == null || !args.I.Any())
            {
                // Not Ids, no entities
                return new EntitiesResult<TEntity>(new List<TEntity>(), 0);
            }

            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            foreach (var id in args.I)
            {
                urlBldr.AddQueryParameter(nameof(args.I), id?.ToString());
            }

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<TEntity>>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result.ToList();
            var relatedEntities = response.RelatedEntities;

            Unflatten(entities, relatedEntities, cancellation);

            var result = new EntitiesResult<TEntity>(entities, entities.Count);
            return result;
        }

        #endregion
    }

    public abstract class FactGetByIdClientBase<TEntity, TKey> : FactWithIdClientBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>

    {
        #region Lifecycle

        internal FactGetByIdClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntityResult<TEntity>> GetById(TKey id, Request<GetByIdArguments> request = null, CancellationToken cancellation = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder(id.ToString());

            // Add query parameters
            var args = request?.Arguments ?? new GetByIdArguments();

            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<GetByIdResponse<TEntity>>(cancellation)
                .ConfigureAwait(false);

            var entity = response.Result;
            var relatedEntities = response.RelatedEntities;

            var singleton = new List<TEntity> { entity };
            Unflatten(singleton, relatedEntities, cancellation);

            var result = new EntityResult<TEntity>(entity);
            return result;
        }

        public virtual async Task<Stream> PrintById(TKey id, int templateId, Request<PrintEntityByIdArguments> request, CancellationToken cancellation = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder($"{id}/print/{templateId}");

            // Add query parameters
            var args = request?.Arguments ?? new PrintEntityByIdArguments();

            urlBldr.AddQueryParameter(nameof(args.Culture), args.Culture);

            // Prepare the message
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var stream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return stream;
        }

        #endregion
    }

    public abstract class CrudClientBase<TEntityForSave, TEntity, TKey> : FactGetByIdClientBase<TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        internal CrudClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        public virtual async Task<EntitiesResult<TEntity>> Save(List<TEntityForSave> entitiesForSave, Request<SaveArguments> request = null, CancellationToken cancellation = default)
        {
            // Common scenario to load entities, modify them and then save them,
            // Many TEntity types actually inherit from TEntityForSave (e.g. Unit)
            // This ensures that if a TEntity is passed in the list it is transformed
            // to TEntityForSave before deserialization
            for (int i = 0; i < entitiesForSave.Count; i++)
            {
                if (entitiesForSave[i] is TEntity entity)
                {
                    entitiesForSave[i] = MapToEntityToSave(entity);
                }
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder();

            // Add query parameters
            var args = request?.Arguments ?? new SaveArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            urlBldr.AddQueryParameter(nameof(args.ReturnEntities), args.ReturnEntities?.ToString());

            // Prepare the message
            var method = HttpMethod.Post;
            var msg = new HttpRequestMessage(method, urlBldr.Uri)
            {
                Content = ToJsonContent(entitiesForSave)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            EntitiesResult<TEntity> result;
            if (args.ReturnEntities ?? false)
            {
                // Extract the response
                var response = await httpResponse.Content
                    .ReadAsAsync<EntitiesResponse<TEntity>>(cancellation)
                    .ConfigureAwait(false);

                var entities = response.Result?.ToList();
                var relatedEntities = response.RelatedEntities;

                Unflatten(entities, relatedEntities, cancellation);

                result = new EntitiesResult<TEntity>(entities, entities?.Count);
            }
            else
            {
                result = EntitiesResult<TEntity>.Empty();
            }

            return result;
        }

        public virtual async Task DeleteByIds(List<TKey> ids, Request request = null, CancellationToken cancellation = default)
        {
            if (ids == null || ids.Count == 0)
            {
                return;
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder();

            // Add query parameters
            foreach (var id in ids)
            {
                urlBldr.AddQueryParameter("I", id?.ToString());
            }

            // Prepare the message
            var method = HttpMethod.Delete;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);
        }

        public virtual async Task DeleteById(TKey id, Request request = null, CancellationToken cancellation = default)
        {
            if (id == null)
            {
                return;
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder(id.ToString());

            // Prepare the message
            var method = HttpMethod.Delete;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);
        }

        #endregion

        #region Helpers

        private string _expandForSave;
        public virtual string ExpandForSave
            => _expandForSave ??= ClientUtil.ExpandForSave<TEntityForSave>();

        private HttpContent ToJsonContent(object payload)
        {
            return JsonContent.Create(payload, options: new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        private TEntityForSave MapToEntityToSave(TEntity entity)
        {
            return entity as TEntityForSave; // TODO
        }

        protected async Task<EntitiesResult<TEntity>> ActivateImpl(List<TKey> ids, Request<ActivateArguments> request, CancellationToken cancellation = default)
        {
            var req = request == null ? null : new Request<ActionArguments>()
            {
                Arguments = request.Arguments,
                Calendar = request.Calendar,
                IsSilent = request.IsSilent
            };

            return await SetIsActive("activate", ids, req, cancellation);
        }

        protected async Task<EntitiesResult<TEntity>> DeactivateImpl(List<TKey> ids, Request<DeactivateArguments> request, CancellationToken cancellation = default)
        {
            var req = request == null ? null : new Request<ActionArguments>()
            {
                Arguments = request.Arguments,
                Calendar = request.Calendar,
                IsSilent = request.IsSilent
            };

            return await SetIsActive("deactivate", ids, req, cancellation);
        }

        private async Task<EntitiesResult<TEntity>> SetIsActive(string action, List<TKey> ids, Request<ActionArguments> request, CancellationToken cancellation = default)
        {
            if (ids == null || !ids.Any())
            {
                return EntitiesResult<TEntity>.Empty();
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder(action);

            // Add query parameters
            var args = request?.Arguments ?? new ActivateArguments();
            urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
            urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);
            urlBldr.AddQueryParameter(nameof(args.ReturnEntities), args.ReturnEntities?.ToString());

            // Prepare the message
            var method = HttpMethod.Put;
            var msg = new HttpRequestMessage(method, urlBldr.Uri)
            {
                Content = ToJsonContent(ids)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            EntitiesResult<TEntity> result;
            if (args.ReturnEntities ?? false)
            {
                // Extract the response
                var response = await httpResponse.Content
                    .ReadAsAsync<EntitiesResponse<TEntity>>(cancellation)
                    .ConfigureAwait(false);

                var entities = response.Result?.ToList();
                var relatedEntities = response.RelatedEntities;

                Unflatten(entities, relatedEntities, cancellation);

                result = new EntitiesResult<TEntity>(entities, entities?.Count);
            }
            else
            {
                result = EntitiesResult<TEntity>.Empty();
            }

            return result;
        }

        #endregion
    }

    public static class ClientUtil
    {
        /// <summary>
        /// Extension method that retrieves a single entity for save using its Id.
        /// The function uses <see cref="ExpandForSave{TEntityForSave}"/> to calculate
        /// the appropriate expand string and uses it to retrieve the entity with 
        /// <paramref name="id"/> which it then maps using <see cref="MapToEntityForSave{TEntityForSave, TEntity}(TEntity)"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id">The id of the entity to get.</param>
        /// <param name="request">The get parameters. <see cref="GetByIdArguments"/> Select and Expand will be overridden.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The retrieved entity after mapping it to <typeparamref name="TEntityForSave"/>.</returns>
        public static async Task<TEntityForSave> GetByIdForSave<TEntityForSave, TEntity, TKey>(this CrudClientBase<TEntityForSave, TEntity, TKey> client, TKey id, Request<GetByIdArguments> request = null, CancellationToken cancellation = default)
            where TEntityForSave : EntityWithKey<TKey>
            where TEntity : EntityWithKey<TKey>
        {
            request ??= new GetByIdArguments();
            request.Arguments.Expand = client.ExpandForSave;
            request.Arguments.Select = null;

            var result = await client.GetById(id, request, cancellation);

            return MapToEntityForSave<TEntityForSave, TEntity>(result.Entity);
        }

        /// <summary>
        /// Returns the expand string that you must use when retreiving an entity
        /// for the intention of modifying and saving it. This expand string 
        /// guarantees that no weak related entities will be deleted upon saving.
        /// </summary>
        /// <typeparam name="TEntityForSave">The type of the entity that will be saved.</typeparam>
        /// <remarks>
        /// Some entities like <see cref="User"/> have a weak collection attached to it like
        /// its roles. If you retrieve the <see cref="User"/> alone, modify it and then save
        /// it, the API will interpret the lack of roles in he submitted <see cref="User"/> 
        /// as you wishing to delete all existing roles on that <see cref="User"/>. 
        /// This is probably not the intended behavior, so you should always include the
        /// weak collections when retrieving an entity for modification and saving.
        /// </remarks>
        public static string ExpandForSave<TEntityForSave>() where TEntityForSave : EntityWithKey
        {
            static IEnumerable<string> CollectionAtoms(TypeDescriptor desc, HashSet<Type> processedAlready)
            {
                if (processedAlready.Add(desc.Type))
                {
                    foreach (var collProp in desc.CollectionProperties)
                    {
                        // For every collection navigation property
                        // 1 - Either return its name if it has no collection properties of its own
                        // 2 - Or return its name appended to the same of each one of its collection properties.
                        var collDesc = collProp.CollectionTypeDescriptor;
                        var collAtoms = CollectionAtoms(collDesc, processedAlready).ToList();
                        if (collAtoms.Count > 0)
                        {
                            foreach (var expand in collAtoms)
                            {
                                yield return $"{collProp.Name}.{expand}";
                            }
                        }
                        else
                        {
                            yield return collProp.Name;
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"The type {typeof(TEntityForSave).Name} cannot be used with {nameof(ExpandForSave)} since it causes infinite recursion.");
                }
            }

            var types = new HashSet<Type>();
            var desc = TypeDescriptor.Get<TEntityForSave>();
            return string.Join(',', CollectionAtoms(desc, types));
        }

        /// <summary>
        /// Maps an entity to its "ForSave" version, for example maps <see cref="User"/> to
        /// a <see cref="UserForSave"/> copying all the properties and weak collections across.
        /// </summary>
        /// <typeparam name="TEntityForSave">The "ForSave" type to map <paramref name="entity"/> to.</typeparam>
        /// <typeparam name="TEntity">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to map</param>
        /// <returns></returns>
        public static TEntityForSave MapToEntityForSave<TEntityForSave, TEntity>(TEntity entity)
            where TEntityForSave : EntityWithKey
            where TEntity : EntityWithKey
        {
            var desc = TypeDescriptor.Get<TEntity>();
            var descForSave = TypeDescriptor.Get<TEntityForSave>();

            return MapInner(entity, desc, descForSave) as TEntityForSave;
        }

        /// <summary>
        /// Helper function.
        /// </summary>
        private static EntityWithKey MapInner(EntityWithKey entity, TypeDescriptor desc, TypeDescriptor descForSave)
        {
            if (entity == null)
            {
                return null;
            }

            var entityForSave = descForSave.Create() as EntityWithKey;

            if (descForSave.NavigationProperties.Any())
            {
                var navProp = descForSave.NavigationProperties.FirstOrDefault();
                throw new InvalidOperationException($"Navigation properties on source types (such as {navProp.Name} on type {descForSave.Name}) are not supported.");
            }

            foreach (var propForSave in descForSave.SimpleProperties)
            {
                var prop = desc.Property(propForSave.Name);
                if (prop == null)
                {
                    throw new InvalidOperationException($"Property {propForSave.Name} on source type {descForSave.Name} has no matching property on target type {desc.Name}.");
                }
                else if (propForSave.Type != prop.Type)
                {
                    throw new InvalidOperationException($"Property {propForSave.Name} on source type {descForSave.Name} has a matching property on target type {desc.Name} but with a different type.");
                }

                var value = prop.GetValue(entity);
                propForSave.SetValue(entityForSave, value);
            }

            foreach (var collPropForSave in descForSave.CollectionProperties)
            {
                var collProp = desc.CollectionProperty(collPropForSave.Name);
                if (collProp == null)
                {
                    throw new InvalidOperationException($"Property {collPropForSave.Name} on source type {descForSave.Name} has no matching property on target type {desc.Name}.");
                }

                var value = collProp.GetValue(entity);
                if (value != null)
                {
                    if (value is IList list)
                    {
                        var listForSave = collPropForSave.CollectionTypeDescriptor.CreateList();

                        foreach (var obj in list)
                        {
                            if (obj == null)
                            {
                                listForSave.Add(null);
                            }
                            else if (obj is EntityWithKey collEntity)
                            {
                                var collEntityForSave = MapInner(collEntity, collProp.CollectionTypeDescriptor, collPropForSave.CollectionTypeDescriptor);
                                listForSave.Add(collEntityForSave);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Collection {collPropForSave.Name} on {descForSave.Name} contains an entity that does not inherit from {nameof(EntityWithKey)}.");
                            }
                        }

                        collPropForSave.SetValue(entityForSave, listForSave);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Property {collPropForSave.Name} on source type {descForSave.Name} has a matching property on target type {desc.Name} that is not a list.");
                    }
                }
            }

            return entityForSave;
        }
    }

    #region Exceptions

    public class TellmaException : Exception
    {
        public TellmaException(string msg) : base(msg)
        {
        }
    }

    public class InternalServerException : TellmaException
    {
        public InternalServerException(string traceIdentifier) :
            base($"An unknown error occurred on the server." + traceIdentifier != null ? $" Trace Identier {traceIdentifier}" : "")
        {
            TraceIdentifier = traceIdentifier;
        }

        public string TraceIdentifier { get; }

        public override string ToString()
        {
            return @$"{base.ToString()}

--- Trace Identifier ---
{TraceIdentifier}";
        }
    }

    public class AuthenticationException : TellmaException
    {
        public AuthenticationException() : base("Failed to authenticate with the Tellma server.")
        {
        }
    }

    public class AuthorizationException : TellmaException
    {
        public AuthorizationException() : base("Your account does not have sufficient permissions to complete this request.")
        {
        }
    }

    public class TellmaOfflineException : TellmaException
    {
        public TellmaOfflineException() : base("You are currently offline.")
        {
        }
    }

    public class NotFoundException : TellmaException
    {
        public NotFoundException(IEnumerable<object> ids) : base("Could not find the supplied Id(s).")
        {
            Ids = ids;
        }

        public IEnumerable<object> Ids { get; }

        public override string ToString()
        {
            var stringifiedIds = string.Join(", ", Ids);

            return @$"{base.ToString()}

--- Ids ---
{stringifiedIds}";
        }
    }

    public class ValidationException : TellmaException
    {
        public ValidationException(ReadonlyValidationErrors errors) : base("The request payload did not pass validation.")
        {
            Errors = errors;
        }

        public ReadonlyValidationErrors Errors { get; }

        public override string ToString()
        {
            ;
            var errorMessages = Errors.SelectMany(pair => pair.Value.Select(msg => $"{pair.Key}: {msg}"));
            var stringifiedErrors = string.Join(Environment.NewLine, errorMessages);

            return @$"{base.ToString()}

--- Validation Errors ---
{stringifiedErrors}";
        }
    }

    #endregion

    internal static class ResponseExtensions
    {
        internal static Response ToResponse(this HttpResponseMessage msg)
        {
            return new Response(msg.ServerTime());
        }

        internal static Response<TResult> ToResponse<TResult>(this HttpResponseMessage msg, TResult result)
        {
            return new Response<TResult>(result, msg.ServerTime());
        }

        internal static async Task EnsureSuccess(this HttpResponseMessage msg, CancellationToken cancellation)
        {
            // Handle all known status codes that tellma may return
            switch (msg.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new AuthenticationException();

                case HttpStatusCode.Forbidden:
                    throw new AuthorizationException();

                case HttpStatusCode.NotFound:
                    var ids = await msg.Content.ReadAsAsync<List<object>>(cancellation);
                    throw new NotFoundException(ids);

                case HttpStatusCode.UnprocessableEntity:
                    var errors = await msg.Content.ReadAsAsync<ValidationErrors>(cancellation);
                    var readonlyErros = new ReadonlyValidationErrors(errors);
                    throw new ValidationException(readonlyErros);

                case HttpStatusCode.BadRequest:
                    var errorMsg = await msg.Content.ReadAsStringAsync();
                    throw new TellmaException(errorMsg);

                case HttpStatusCode.InternalServerError:
                    var traceIdSpy = await msg.Content.ReadAsAsync<TraceIdentifierSpy>(cancellation);
                    throw new InternalServerException(traceIdSpy?.TraceIdentifier);

                case 0:
                    throw new TellmaOfflineException();
            }

            if (!msg.IsSuccessStatusCode)
            {
                // Future proofing
                throw new TellmaException($"Unhandled status code {msg.StatusCode}.");
            }
        }

        /// <summary>
        /// Extracts the server time value from the <see cref="HttpResponseMessage"/> headers.
        /// </summary>
        private static DateTimeOffset ServerTime(this HttpResponseMessage msg)
        {
            if (!(msg.Headers.TryGetValues(ResponseHeaders.ServerTime, out IEnumerable<string> values) &&
                values.Any() && DateTimeOffset.TryParse(values.First(), out DateTimeOffset serverTime)))
            {
                serverTime = DateTimeOffset.UtcNow;
            }

            return serverTime;
        }

        private class TraceIdentifierSpy
        {
            public string TraceIdentifier { get; set; }
        }
    }

    /// <summary>
    /// The result of a Tellma API request.
    /// </summary>
    public class Response
    {
        public Response(DateTimeOffset serverTime)
        {
            ServerTime = serverTime;
        }

        public DateTimeOffset ServerTime { get; }
    }

    /// <summary>
    /// The result of a Tellma API request that returns data.
    /// </summary>
    /// <typeparam name="TResult">The type of the returned data.</typeparam>
    public class Response<TResult> : Response
    {
        public Response(TResult result, DateTimeOffset serverTime) : base(serverTime)
        {
            Result = result;
        }

        public TResult Result { get; }

        /// <summary>
        /// Implicit conversion to <typeparamref name="TResult"/>.
        /// </summary>
        public static implicit operator TResult(Response<TResult> response) => response.Result;
    }

    /// <summary>
    /// Base class of all requests to the Tellma server.
    /// </summary>
    public class Request
    {
        private static readonly Request _default = new Request();

        internal static Request Default => _default;

        /// <summary>
        /// If set to True, the <see cref="User.LastAccess"/> property of the user is not touched.
        /// </summary>
        /// <remarks> 
        /// By default when a request is made by a user, Tellma updates that user's 
        /// <see cref="User.LastAccess"/> property in the database to the time of the request.
        /// </remarks>
        public bool IsSilent { get; set; }

        /// <summary>
        /// The request calendar. Defaults to <see cref="Calendar.GC"/>.
        /// </summary>
        public Calendar Calendar { get; set; } = Calendar.GC;
    }

    /// <summary>
    /// Base class of all requests to the Tellma server that carry arguments.
    /// </summary>
    public class Request<T> : Request
    {
        /// <summary>
        /// The arguments to send with the request.
        /// </summary>
        public T Arguments { get; set; }

        /// <summary>
        /// Implicit conversion.
        /// </summary>
        public static implicit operator Request<T>(T args) => new Request<T> { Arguments = args };
    }

    public enum Calendar
    {
        /// <summary>
        /// Gregorian.
        /// </summary>
        GC = 0,

        /// <summary>
        /// Umm Al-Qura.
        /// </summary>
        UQ = 1,

        /// <summary>
        /// Ethiopian
        /// </summary>
        ET = 2
    }

    internal static class UrlBuilderExtensions
    {
        /// <summary>
        /// Adds a new parameter to the query in the <see cref="UriBuilder"/>,
        /// with the given <paramref name="name"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="bldr">The <see cref="UriBuilder"/> whose query to modify.</param>
        /// <param name="name">The name of the query parameter.</param>
        /// <param name="value">The value of the query parameter.</param>
        /// <remarks>
        /// This function URL-encodes <paramref name="name"/> and <paramref name="value"/> before
        /// adding them to the query. Also if either <see cref="name"/> or <see cref="value"/> are
        /// null or whitespace nothing is added.
        /// </remarks>
        internal static void AddQueryParameter(this UriBuilder bldr, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            // Original query
            var query = bldr.Query.Trim();

            // Separator
            string s = "";
            if (string.IsNullOrEmpty(query))
            {
                s = "?";
            }
            else if (query != "?")
            {
                s = "&";
            }

            // Name + Value
            name = UrlEncoder.Default.Encode(name);
            value = UrlEncoder.Default.Encode(value);

            // Set the final result
            bldr.Query = $"{query}{s}{name}={value}";
        }
    }

    internal static class HttpContentExtensions
    {
        internal static async Task<T> ReadAsAsync<T>(this HttpContent content, CancellationToken cancellation)
        {
            return await content.ReadFromJsonAsync<T>(
                options: _options,
                cancellationToken: cancellation);
        }

        private static readonly JsonSerializerOptions _options =
            JsonUtil.ConfigureOptionsForWeb(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }


    #region Playground

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
