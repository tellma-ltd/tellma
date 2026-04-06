using System.Collections.Generic;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Binds to all the configurations needed by the client store of the embedded instance of
    /// IdentityServer this is following the options pattern recommended by microsoft: https://bit.ly/2GiJ19F.
    /// </summary>
    public class ClientApplicationsOptions
    {
        public const int DefaultAccessTokenLifetimeInDays = 3;

        public string WebClientUri { get; set; }

        public int WebClientAccessTokenLifetimeInDays { get; set; } = DefaultAccessTokenLifetimeInDays;

        public int McpClientAccessTokenLifetimeInDays { get; set; } = DefaultAccessTokenLifetimeInDays;

        public int McpClientRefreshTokenLifetimeInDays { get; set; } = 90;

        /// <summary>
        /// List of MCP client configurations. Each entry represents an AI agent
        /// (e.g., Claude, Cursor, Windsurf) that connects via MCP OAuth 2.1.
        /// Each entry should have: RedirectUri, PostLogoutRedirectUri, and Origin.
        /// </summary>
        public List<McpClientOptions> McpClientUrls { get; set; } = new();
    }

    /// <summary>
    /// Configuration for a single MCP client origin (an AI agent platform).
    /// </summary>
    public class McpClientOptions
    {
        /// <summary>
        /// The redirect URI for the OAuth callback (e.g., "https://claude.ai/api/mcp/auth_callback").
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// The post-logout redirect URI (e.g., "https://claude.ai").
        /// </summary>
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// The CORS origin (e.g., "https://claude.ai"). If not specified, derived from RedirectUri.
        /// </summary>
        public string Origin { get; set; }
    }
}
