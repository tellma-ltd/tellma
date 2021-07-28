using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using Tellma.Api;
using Tellma.Services.ClientProxy;
using Tellma.Utilities.Common;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _ClientProxyExtensions
    {
        private const string SectionName = "ClientApplications";

        public static IServiceCollection AddClientAppProxy(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            
            services
                .AddNotifications(config) // Required dependency
                .AddClientAppAddressResolver(config) // Required dependency
                .AddSingleton<IClientProxy, ClientAppProxy>();

            // Job and Queue for fire-and-forget style of dispatching inbox notifications
            services = services
                .AddHostedService<InboxNotificationsJob>()
                .AddSingleton<InboxNotificationsQueue>();

            return services;
        }

        public static IServiceCollection AddClientAppAddressResolver(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Bind configuration
            var clientAppsSection = config.GetSection(SectionName);
            services.Configure<ClientProxyOptions>(clientAppsSection);

            var options = clientAppsSection.Get<ClientProxyOptions>();
            if (string.IsNullOrWhiteSpace(options.WebClientUri))
            {
                throw new InvalidOperationException($"Configuration value {SectionName}:{nameof(ClientProxyOptions.WebClientUri)} was not provided.");
            }

            // Allows various parts of the application to access the client app address
            services.AddSingleton<ClientAppAddressResolver>();

            return services;
        }

        /// <summary>
        ///Configure CORS for the origin where the client app is hosted.
        /// </summary>
        /// <remarks>This is only required if the embedded client app is NOT enabled, and the client app is hosted elsewhere.</remarks>
        public static IApplicationBuilder UseCorsForNonEmbeddedClientApp(this IApplicationBuilder app, IConfiguration config)
        {
            var clientAppsSection = config.GetSection(SectionName);
            var options = clientAppsSection.Get<ClientProxyOptions>();

            string webClientUri = options.WebClientUri?.WithoutTrailingSlash();
            if (string.IsNullOrWhiteSpace(webClientUri))
            {
                throw new Exception($"The configuration value {SectionName}:{nameof(ClientProxyOptions.WebClientUri)} is required when EmbeddedClientApplicationEnabled is not set to true.");
            }

            // If a web client is listed in the configurations, add it to CORS
            return app.UseCors(builder =>
            {
                builder.WithOrigins(webClientUri)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders(
                    "x-image-id",
                    "x-settings-version",
                    "x-permissions-version",
                    "x-definitions-version",
                    "x-user-settings-version",
                    "x-admin-settings-version",
                    "x-admin-permissions-version",
                    "x-admin-user-settings-version",
                    "x-global-settings-version"
                );
            });
        }
    }
}
