using IdentityModel.Client;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Model.Common;

namespace Tellma.Client
{
    public class TellmaException : Exception
    {
        public TellmaException(string msg) : base(msg)
        {
        }
        public TellmaException(string msg, Exception inner) : base(msg, inner)
        {
        }
    }

    public interface IAccessTokenFactory
    {
        Task<string> GetAccessToken(CancellationToken cancellation = default);
    }

    public interface IApiUrlAccessor
    {
        string GetApiUrl();
    }

    public interface IClientContextAccessor : IAccessTokenFactory, IHttpClientFactory, IApiUrlAccessor
    {

    }

    /// <summary>
    /// Provides managed access to all the Tellma API.
    /// </summary>
    /// <remarks>Scope: create one <see cref="TellmaClient"/> per client ID.</remarks>
    public class TellmaClient : IClientContextAccessor
    {
        private static HttpClient _httpClient = null;
        private static DiscoveryCache _discoveryCache = null;

        private readonly string _apiUrl = "";
        private readonly string _authorityUrl = "";
        private readonly string _clientId;
        private readonly string _clientSecret;

        private SemaphoreSlim _accessTokenSemaphore = new SemaphoreSlim(1);
        private string _accessToken;
        private DateTimeOffset _accessTokenExpiry = DateTimeOffset.MinValue;

        /// <summary>
        /// If supplied in one of the constructor overloads, use always.
        /// </summary>
        private readonly HttpClient _httpClientOverride;
        private int? _defaultTenantId;

        public TellmaClient SetDefaultTenantId(int tenantId)
        {
            _defaultTenantId = tenantId;
            return this;
        }

        public TellmaClient(string apiUrl, string authorityUrl, string clientId, string clientSecret)
        {
            _apiUrl = apiUrl;
            _authorityUrl = authorityUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public TellmaClient(HttpClient httpClient, string accessToken)
        {
            _httpClientOverride = httpClient;
            _accessToken = accessToken;
            _accessTokenExpiry = DateTimeOffset.MaxValue; // Never expires
        }

        private HttpClient GetHttpClient()
        {
            return _httpClientOverride;
        }

        public bool AccessTokenHasExpired => _accessTokenExpiry < DateTimeOffset.Now;

        public async Task<string> GetAccessToken(CancellationToken cancellation = default)
        {
            // If expired, grab a new one
            if (AccessTokenHasExpired)
            {
                await RefreshAccessToken(cancellation);
            }

            return _accessToken;
        }

        public async Task RefreshAccessToken(CancellationToken cancellation = default)
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

        public string GetApiUrl()
        {
            return _apiUrl;
        }

        public HttpClient CreateClient(string _)
        {
            return _httpClientOverride ?? (_httpClient ??= new HttpClient());
        }


        private GeneralSettingsClient _generalSettings;
        public GeneralSettingsClient GeneralSettings => _generalSettings ??= new GeneralSettingsClient(this);
    }

    public class ClientBase
    {

    }

    public abstract class FactClientBase<TEntity> where TEntity : Entity
    {
        public virtual Task<GetResponse<TEntity>> GetEntities(GetArguments args, CancellationToken cancellation = default)
        {
            const string ActionPath = "";

            throw new NotImplementedException();
        }

        protected abstract string ControllerPath { get; }
    }

    public class GeneralSettingsClient : ClientBase
    {
        private const string ControllerPath = "api/general-settings";

        private readonly IClientContextAccessor _accessor;

        public GeneralSettingsClient(IClientContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public async Task<ResponseBase> PingResponse(ApplicationRequest req, CancellationToken cancellation = default)
        {
            const string ActionPath = "ping";

            // Get the basics
            string token = await _accessor.GetAccessToken().ConfigureAwait(false);
            HttpClient client = _accessor.CreateClient();
            int tenantId = req.TenantId;

            // Prepare the request
            var url = $"{_accessor.GetApiUrl()}/{ControllerPath}/{ActionPath}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("X-Tenant-Id", tenantId.ToString());

            // Send the request
            using var response = await client.SendAsync(request, cancellation).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync();

            // Return the response
            return new ApplicationResponse<bool>
            {
                StatusCode = response.StatusCode,
                Content = true
            };
        }

        public async Task Ping(ApplicationRequest req, CancellationToken cancellation = default)
        {
            var response = await PingResponse(req, cancellation).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
    }

    public class ResponseBase
    {
        public HttpStatusCode StatusCode { get; set; }

        public bool IsError => StatusCode >= HttpStatusCode.BadRequest;

        public void EnsureSuccessStatusCode()
        {
            if (IsError)
            {
                throw new TellmaException("Error");
            }
        }

        public Freshness? GlobalSettingsFreshness { get; set; }
    }

    public class ApplicationResponse<TEntity> : ResponseBase
    {
        public Freshness? SettingsFreshness { get; set; }
        public Freshness? DefinitionsFreshness { get; set; }
        public Freshness? UserSettingsFreshness { get; set; }
        public Freshness? PermissionsFreshness { get; set; }
        public TEntity Content { get; set; }
    }

    public class RequestBase
    {
        public string Culture { get; set; }
        public bool IsSilent { get; set; }
        public string GlobalSettingsVersion { get; set; }
    }

    public class ApplicationRequest : RequestBase
    {
        public int TenantId { get; set; }
        public string SettingsVersion { get; set; }
        public string DefinitionsVersion { get; set; }
        public string UserSettingsVersion { get; set; }
        public string PermissionsVersion { get; set; }
    }

    public enum Freshness
    {
        Fresh,
        Stale
    }
}
