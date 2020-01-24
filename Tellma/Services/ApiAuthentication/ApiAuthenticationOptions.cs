namespace Tellma.Services.ApiAuthentication
{
    /// <summary>
    /// Binds to all the configurations needed by the API authentication
    /// This is following the options pattern recommended by microsoft: https://bit.ly/2GiJ19F
    /// </summary>
    public class ApiAuthenticationOptions
    {
        /// <summary>
        /// The URI of the OIDC that is observed by the API as the issuing authority of access tokens.
        /// When this is null the embedded identity server is used as issuing authority
        /// </summary>
        public string AuthorityUri { get; set; }
    }
}
