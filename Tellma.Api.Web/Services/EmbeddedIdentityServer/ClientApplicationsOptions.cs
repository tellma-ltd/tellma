namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Binds to all the configurations needed by the client store of the embedded instance of 
    /// IdentityServer this is following the options pattern recommended by microsoft: https://bit.ly/2GiJ19F
    /// </summary>
    public class ClientApplicationsOptions
    {
        public const int DefaultAccessTokenLifetimeInDays = 3;

        public string WebClientUri { get; set; }

        public int WebClientAccessTokenLifetimeInDays { get; set; } = DefaultAccessTokenLifetimeInDays;

        public string MobileClientUri { get; set; }

        public int MobileClientAccessTokenLifetimeInDays { get; set; } = DefaultAccessTokenLifetimeInDays;
    }
}
