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

            if (!string.IsNullOrWhiteSpace(authorityUri))
            {
                // Add the default scheme for the embedded IdentityServer
                services.AddAuthentication()

                    // Add the Bearer scheme for the API
                    .AddIdentityServerAuthentication(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        options.Authority = authorityUri;
                        options.ApiName = Constants.ApiResourceName;
                    });
            }
            else
            {
                // TODO: replace with https://bit.ly/2GQVbFG, as soon as IdentityServer4 version 3.0 stable is released 
                // IF Authority URI is not supplied assume the embedded identity server instance is enabled and used
                authorityUri = "https://localhost:44368";
            }

            // Add helper service that provides access to the authenticated user's email and external Id 
            services.AddSingleton<IExternalUserAccessor, ExternalUserAccessor>();

            // return
            return services;
        }
    }
}
