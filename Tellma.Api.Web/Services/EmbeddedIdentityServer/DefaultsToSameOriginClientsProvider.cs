using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Tellma.Services.ClientProxy;
using Tellma.Services.Utilities;
using Tellma.Utilities.Common;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// The embedded IdentityServer by default authorizes a single client (the Angular web app) 
    /// and assumes that this client is hosted in the same origin/domain as the identity server unless
    /// otherwise specified in a configuration provider.
    /// </summary>
    public class DefaultsToSameOriginClientsProvider : IUserClientsProvider
    {
        private readonly ClientAppAddressResolver _clientResolver;
        private readonly ClientApplicationsOptions _config;

        public DefaultsToSameOriginClientsProvider(
            ClientAppAddressResolver clientResolver, 
            IOptions<ClientApplicationsOptions> options)
        {
            _clientResolver = clientResolver;
            _config = options.Value;
        }

        // clients want to access resources (aka scopes)
        public IEnumerable<Client> UserClients()
        {
            // Determine the ClientApp's URI from the config file
            var webClientOrigin = _clientResolver.Resolve().WithoutTrailingSlash();

            // return the Application Client Web App
            yield return new Client
            {
                ClientId = Constants.WebClientName,
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,

                RedirectUris = { $"{webClientOrigin}/sign-in-callback", $"{webClientOrigin}/assets/silent-refresh-callback.html" },
                PostLogoutRedirectUris = { $"{webClientOrigin}/welcome" },
                AllowedCorsOrigins = { webClientOrigin },

                RequireConsent = false,
                AccessTokenLifetime = 60 * 60 * 24 * (_config?.WebClientAccessTokenLifetimeInDays ?? ClientApplicationsOptions.DefaultAccessTokenLifetimeInDays),
                AlwaysIncludeUserClaimsInIdToken = true,

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    Constants.ApiResourceName
                },
            };
        }
    }
}
