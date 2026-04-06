using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using System;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Extends the default strict redirect URI validation to allow localhost with any port
    /// for the MCP client. This supports Claude Code CLI which uses a random available port.
    /// </summary>
    public class McpRedirectUriValidator : StrictRedirectUriValidator
    {
        public override Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (client.ClientId == Constants.McpClientName && IsLocalhostUri(requestedUri))
            {
                return Task.FromResult(true);
            }

            return base.IsRedirectUriValidAsync(requestedUri, client);
        }

        public override Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (client.ClientId == Constants.McpClientName && IsLocalhostUri(requestedUri))
            {
                return Task.FromResult(true);
            }

            return base.IsPostLogoutRedirectUriValidAsync(requestedUri, client);
        }

        private static bool IsLocalhostUri(string uri)
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            {
                return (parsed.Host == "localhost" || parsed.Host == "127.0.0.1")
                    && (parsed.Scheme == "http" || parsed.Scheme == "https");
            }

            return false;
        }
    }
}
