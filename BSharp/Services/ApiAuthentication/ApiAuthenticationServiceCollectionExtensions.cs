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
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration config)
        {
            // Add the authentication schemes
            var authorityUri = config["ApiAuthentication:AuthorityUri"].WithoutTrailingSlash();

            if (string.IsNullOrWhiteSpace(authorityUri))
            {
                // IF Authority URI is not supplied assume the embedded identity server instance is enabled and used
                authorityUri = "https://localhost:44339"; // TODO: Make this automatic somehow
            }

            // Add the default scheme for the embedded IdentityServer
            services.AddAuthentication()

                // Add the Bearer scheme for the API
                .AddIdentityServerAuthentication(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = authorityUri;
                    options.ApiName = Constants.ApiResourceName;
                    options.JwtValidationClockSkew = TimeSpan.FromSeconds(0);
                });

            // Add helper services
            services.AddSingleton<IUserProvider, UserProvider>();

            // return
            return services;
        }
    }
}
