using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // MCP Connector Client (Authorization Code + PKCE for AI agents like Claude Code
            var mcpUrls = _config?.McpClientUrls;
            if (mcpUrls != null && mcpUrls.Count > 0)
            {
                yield return new Client
                {
                    ClientId = Constants.McpClientName,
                    ClientName = "MCP Connector",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    RequirePkce = true,

                    RedirectUris = mcpUrls
                        .Where(e => !string.IsNullOrWhiteSpace(e.RedirectUri))
                        .Select(e => e.RedirectUri)
                        .ToList(),

                    PostLogoutRedirectUris = mcpUrls
                        .Where(e => !string.IsNullOrWhiteSpace(e.PostLogoutRedirectUri))
                        .Select(e => e.PostLogoutRedirectUri)
                        .ToList(),

                    AllowedCorsOrigins = mcpUrls
                        .Select(e => !string.IsNullOrWhiteSpace(e.Origin)
                            ? e.Origin
                            : GetOrigin(e.RedirectUri))
                        .Where(e => e != null)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),

                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    SlidingRefreshTokenLifetime = 60 * 60 * 24 * (_config?.McpClientRefreshTokenLifetimeInDays ?? 30),
                    AbsoluteRefreshTokenLifetime = 60 * 60 * 24 * 365 * 10,
                    AccessTokenLifetime = 60 * 60 * 24 * (_config?.McpClientAccessTokenLifetimeInDays ?? ClientApplicationsOptions.DefaultAccessTokenLifetimeInDays),
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

        /// <summary>
        /// Extracts the origin (scheme + host) from a URI string.
        /// </summary>
        private static string GetOrigin(string uri)
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
                return $"{parsed.Scheme}://{parsed.Host}";

            return null;
        }
    }
}
