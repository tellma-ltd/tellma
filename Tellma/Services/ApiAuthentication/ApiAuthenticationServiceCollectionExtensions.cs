using Tellma.Services.ApiAuthentication;
using Tellma.Services.Identity;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.OAuth2Introspection;
using System.Diagnostics;

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
                authBuilder.AddIdentityServerAuthentication(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    opt.Authority = authorityUri;
                    opt.ApiName = Constants.ApiResourceName;

                    // TODO
                    //opt.JwtBearerEvents = new JwtBearerEvents
                    //{
                    //    OnMessageReceived = context =>
                    //    {
                    //        // If the request is for the SignalR hub, use the token in the query string
                    //        var accessToken = context.Request.Query["access_token"];
                    //        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/api/hubs/"))
                    //        {
                    //            // Read the token out of the query string
                    //            context.Token = accessToken;
                    //        }

                    //        return Task.CompletedTask;
                    //    }
                    //};
                });
            }
            else
            {
                // Add the bearer scheme for the local API
                // This relies on tokens from the embedded identity server
                authBuilder.AddLocalApi(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    // TODO: How to get the token from the query string?
                });
            }

            // Add helper service that provides access to the authenticated user's email and external Id 
            services.AddSingleton<IExternalUserAccessor, ExternalUserAccessor>();

            // return
            return services;
        }
    }
}
