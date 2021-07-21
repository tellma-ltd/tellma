using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.Utilities;

namespace Tellma.Services.ClientApp
{
    // TODO: Integrate with the rest of the folder

    public class ClientAppAddressResolver
    {
        private readonly GlobalOptions _options;
        private readonly IHttpContextAccessor _accessor;

        public ClientAppAddressResolver(IOptions<GlobalOptions> options, IHttpContextAccessor accessor)
        {
            _options = options.Value;
            _accessor = accessor;
        }

        /// <summary>
        /// Determines the web address of the client application (e.g. https://web.tellma.com/).
        /// Takes into account whether the embedded client app is enabled or not.
        /// IMPORTANT: Calls to this service are only allowed from within the scope of a web request.
        /// </summary>
        public string Resolve()
        {
            string result;
            if (_options.EmbeddedClientApplicationEnabled)
            {
                // IF the embedded client app is enabled, use the same origin as the embedded IdentityServer
                var request = _accessor.HttpContext?.Request ?? throw new InvalidOperationException($"Calls to {nameof(ClientAppAddressResolver)}.{nameof(ClientAppAddressResolver.Resolve)} should only be made from within the scope of a web request.");
                result = $"https://{request?.Host}/{request?.PathBase}";
            }
            else
            {
                // IF the client app is hosted elsewhere, use the the WebClientUri value
                // in the config (validation in Startup.cs ensures this value isn't null)
                result = _options.ClientApplications?.WebClientUri ?? throw new InvalidOperationException($"Missing from configuration: {nameof(ClientApplicationsOptions.WebClientUri)}");
            }

            return result;
        }
    }
}
