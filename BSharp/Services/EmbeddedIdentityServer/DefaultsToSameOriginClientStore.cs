using BSharp.Services.Utilities;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// The embedded IdentityServer by default authorizes exactly 2 clients (web and mobile) 
    /// with authorized redirect URIs pointing to the same domain in which IdentityServer itself 
    /// is hosted, since by default the clients are hosted on the same domain, this reduces the 
    /// complexity of installing simple instances of the app for small companies. 
    /// Given that there is no reliable way to retrieve the domain that hosts IdentityServer at startup 
    /// time, we rely on a special implementation of <see cref="IClientStore"/> which retrieves the domain 
    /// from the request at run time and dynamically constructs a list of <see cref="Client"/>s with 
    /// redirect URIs based on that, one can still override this behaviour with a configuration provider
    /// </summary>
    public class DefaultsToSameOriginClientStore : IClientStore
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly ClientStoreConfiguration _config;

        public DefaultsToSameOriginClientStore(IHttpContextAccessor accessor, IOptions<ClientStoreConfiguration> options)
        {
            _accessor = accessor;
            _config = options.Value;
        }

        // clients want to access resources (aka scopes)
        private IEnumerable<Client> GetClients()
        {
            // Determine the ClientApp's URI from the config file
            var uri = _config.WebClientUri;
            if (string.IsNullOrWhiteSpace(uri))
            {
                // If it is not defined, then use the same origin as IdentityServer by default
                var request = _accessor?.HttpContext?.Request;
                uri = $"https://{request?.Host}/{request?.PathBase}";
            }

            // Return the Application Client Web App
            yield return new Client
            {
                ClientId = "WebClient",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,

                RedirectUris = { $"{uri}signin-callback", $"{uri}silent-refresh-callback" },
                PostLogoutRedirectUris = { $"{uri}landing-page" },
                RequireConsent = false,
                AccessTokenLifetime = 70,

                AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        Constants.ApiResourceName
                    }
            };

            //// TODO: Return the Mobile Client App 
            //yield return new Client {
            //    ClientId = "BSharpMobileClient",
            //    AllowedGrantTypes = GrantTypes.Code,
            //    RequirePkce = true,

            //};
        }

        // Implementation of IClientStore
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var query = from client in GetClients()
                        where client.ClientId == clientId
                        select client;

            return Task.FromResult(query.SingleOrDefault());
        }
    }
}
