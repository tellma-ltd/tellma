using Tellma.Services.Utilities;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Binds to all the configurations needed by the client store of the embedded instance of 
    /// IdentityServer this is following the options pattern recommended by microsoft: https://bit.ly/2GiJ19F
    /// </summary>
    public class ClientApplicationsOptions : WebClientOptions
    {
        public static readonly int DEFAULT_ACCESS_TOKEN_LIFETIME_IN_DAYS = 3;

        public int WebClientAccessTokenLifetimeInDays { get; set; } = DEFAULT_ACCESS_TOKEN_LIFETIME_IN_DAYS;

        public string MobileClientUri { get; set; }

        public int MobileClientAccessTokenLifetimeInDays { get; set; } = DEFAULT_ACCESS_TOKEN_LIFETIME_IN_DAYS;
    }
}
