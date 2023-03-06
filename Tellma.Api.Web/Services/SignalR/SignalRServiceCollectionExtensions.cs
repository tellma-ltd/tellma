using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System;
using Tellma.Api.Dto;
using Tellma.Services.SignalR;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SignalRServiceCollectionExtensions
    {
        private const string SectionName = "Azure:SignalR";
        public static IServiceCollection AddSignalRImplementation(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // For real-time notifications
            var bldr = services.AddSignalR(opt =>
            {
                // Configures SignalR to return the full error to the client in a development environment
                opt.EnableDetailedErrors = env.IsDevelopment();
            })
            .AddJsonProtocol(opt =>
            {
                // Keep property names unchanged when sending payloads
                JsonUtil.ConfigureOptionsForWeb(opt.PayloadSerializerOptions);
            });

            // Add azure service if a connection string is supplied
            var azureSignalRConnectionString = config?.GetSection(SectionName)?.GetValue<string>("ConnectionString");
            if (!string.IsNullOrWhiteSpace(azureSignalRConnectionString))
            {
                bldr.AddAzureSignalR(azureSignalRConnectionString);
            }

            // Retrieve the UserId from the JWT Subject claim, rather than the default
            services.AddSingleton<IUserIdProvider, SubjectBasedUserIdProvider>();

            return services;
        }

        /// <summary>
        /// SignalR cannot add the access token in the header during the WebSocket
        /// hand shake, it adds it in the query string instead. Here we fix this by
        /// intercepting the request, and moving the token from the query string back
        /// to the header IF the authorization header is null AND the request is targeting
        /// a SignalR hub AND there is an access_token in the query string
        /// </summary>
        public static IApplicationBuilder UseQueryStringToken(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                if (context.Request.Headers["Authorization"].Count == 0 &&
                    context.Request.Query.TryGetValue("access_token", out StringValues accessToken) &&
                    !string.IsNullOrWhiteSpace(accessToken))
                {
                    context.Request.Headers.Add("Authorization", $"Bearer {accessToken}");
                }

                await next.Invoke();
            });
        }
    }
}
