using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System;
using Tellma.Services.SignalR;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SignalRServiceCollectionExtensions
    {

        public static IServiceCollection AddSignalRImplementation(this IServiceCollection services, IWebHostEnvironment env)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // For real-time notifications
            services.AddSignalR(opt =>
            {
                // Configures SignalR to return the full error to the client in a development environment
                opt.EnableDetailedErrors = env.IsDevelopment();
            })
            .AddJsonProtocol(opt =>
            {
                // Keep property names unchanged when sending payloads
                opt.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });

            // Retrieve the UserId from the JWT Subject claim, rather than the default
            services.AddSingleton<IUserIdProvider, SubjectBasedUserIdProvider>();

            return services;
        }

        /// <summary>
        /// SignalR cannot add the access token in the header during the WebSocket
        /// hand shake, it adds it in the query string instead.Here we fix this by
        /// intercepting the request, and moving the token from the query string back
        /// to the header IF the authorization header is null AND the request is targeting
        /// a SignalR hub AND there is an access_token in the query string
        /// </summary>
        public static IApplicationBuilder UseQueryStringToken(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {

                if (context.Request.Path.StartsWithSegments("/api/hubs") &&
                    context.Request.Headers["Authorization"].Count == 0 &&
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
