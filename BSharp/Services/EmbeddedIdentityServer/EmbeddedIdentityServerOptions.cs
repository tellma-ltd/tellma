using System.ComponentModel.DataAnnotations;

namespace BSharp.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Binds to all the configurations needed by the embedded instance of identity server.
    /// This is following the options pattern recommended by microsoft: https://bit.ly/2GiJ19F
    /// </summary>
    public class EmbeddedIdentityServerOptions
    {
        /// <summary>
        /// The thumbpring of the certificate used to sign security tokens, this is required in
        /// a production environment. The certificate must be installed in the host environment.
        /// </summary>
        public string X509Certificate2Thumbprint { get; set; }

        /// <summary>
        /// Required connection string to the Identity Server database.
        /// The database is defined in the project BSharp.Database.Identity
        /// </summary>
        [Required]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Access tokens issued by IdentityServer for clients
        /// </summary>
        public int AccessTokenLifetimeInDays { get; set; } = 3;

        /// <summary>
        /// A persisted sign-in session with the embedded identity server
        /// should last this long, during which the server will happily issue
        /// identity and access tokens to the configured clients
        /// </summary>
        public int CookieSessionLifetimeInDays { get; set; } = 3;

        public GoogleOptions Google { get; set; }

        public MicrosoftOptions Microsoft { get; set; }

        public class GoogleOptions
        {
            [Required]
            public string ClientId { get; set; }

            [Required]
            public string ClientSecret { get; set; }
        }

        public class MicrosoftOptions
        {
            [Required]
            public string ClientId { get; set; }

            [Required]
            public string ClientSecret { get; set; }
        }
    }

}
