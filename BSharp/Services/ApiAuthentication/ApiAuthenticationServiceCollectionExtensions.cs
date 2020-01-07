using BSharp.Services.ApiAuthentication;
using BSharp.Services.Identity;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiAuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configSection)
        {
            // Add the authentication schemes
            var config = configSection.Get<ApiAuthenticationOptions>();
            var authorityUri = config.AuthorityUri?.WithoutTrailingSlash();

            var authBuilder = services.AddAuthentication();
            if (!string.IsNullOrWhiteSpace(authorityUri))
            {
                // Add the Bearer scheme for the API
                // This relies on tokens from an external identity server
                authBuilder.AddIdentityServerAuthentication(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = authorityUri;
                    options.ApiName = Constants.ApiResourceName;
                });
            }
            else
            {
                // Add the bearer scheme for the local API
                // This relies on tokens from the embedded identity server
                authBuilder.AddLocalApi(JwtBearerDefaults.AuthenticationScheme, opt => { });
            }

            // Add helper service that provides access to the authenticated user's email and external Id 
            services.AddSingleton<IExternalUserAccessor, ExternalUserAccessor>();

            // return
            return services;
        }
    }
}
