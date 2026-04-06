using Duende.IdentityServer.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Services.ClientProxy;
using Tellma.Utilities.Common;

namespace Tellma.Services.EmbeddedIdentityServer
{
    public class EmbeddedIdentityCorsPolicyService : ICorsPolicyService
    {
        private readonly ClientAppAddressResolver _resolver;
        private readonly HashSet<string> _mcpOrigins = [];

        public EmbeddedIdentityCorsPolicyService(
            ClientAppAddressResolver resolver,
            IOptions<ClientApplicationsOptions> options)
        {
            _resolver = resolver;

            // Build the set of allowed MCP origins from configuration
            var mcpUrls = options.Value?.McpClientUrls;
            if (mcpUrls != null && mcpUrls.Count > 0)
            {
                _mcpOrigins = new HashSet<string>(
                    mcpUrls.Select(e => !string.IsNullOrWhiteSpace(e.Origin)
                            ? e.Origin
                            : GetOrigin(e.RedirectUri))
                        .Where(e => e != null),
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            var webClientOrigin = _resolver.Resolve().WithoutTrailingSlash();
            var allowed = origin == webClientOrigin || _mcpOrigins.Contains(origin);
            return Task.FromResult(allowed);
        }

        private static string GetOrigin(string uri)
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
                return $"{parsed.Scheme}://{parsed.Host}";

            return null;
        }
    }
}
