using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.Utilities;
using Tellma.Utilities.Common;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _ApiAuthenticationExtensions
    {
        private const string SectionName = "ApiAuthentication";

        public static IServiceCollection AddApiAuthWithExternalIdentity(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Add the authentication schemes
            var configSection = config.GetSection(SectionName);
            var options = configSection.Get<ApiAuthenticationOptions>();
            var authorityUri = options.AuthorityUri?.WithoutTrailingSlash();

            var authBuilder = services.AddAuthentication();
            if (string.IsNullOrWhiteSpace(authorityUri))
            {
                throw new InvalidOperationException($"{nameof(GlobalOptions.EmbeddedIdentityServerEnabled)} is disabled, therefore {SectionName}:{nameof(options.AuthorityUri)} should be specified in a configuration provider.");
            }
            else
            {
                // Add the Bearer scheme for the API
                // This relies on tokens from an external identity server
                authBuilder.AddIdentityServerAuthentication(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    opt.Authority = authorityUri;
                    opt.ApiName = Constants.ApiResourceName;
                });
            }

            // Add helper service that provides access to the authenticated user's email and external Id 
            services.AddExternalUserAccessor();

            // return
            return services;
        }

        public static IServiceCollection AddApiAuthWithEmbeddedIdentity(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            // Add the bearer scheme for the local API
            // This relies on tokens from the embedded identity server
            services.AddAuthentication().AddLocalApi(JwtBearerDefaults.AuthenticationScheme, opt => { });

            // Add helper service that provides access to the authenticated user's email and external Id 
            services.AddExternalUserAccessor();

            // return
            return services;
        }

        private static IServiceCollection AddExternalUserAccessor(this IServiceCollection services)
        {
            // Add helper service that provides access to the authenticated user's email and external Id 
            return services.AddSingleton<IExternalUserAccessor, ExternalUserAccessor>();
        }
    }
}
