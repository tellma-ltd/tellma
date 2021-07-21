using Microsoft.Extensions.Configuration;
using System;
using Tellma.Api;
using Tellma.Services.ClientApp;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _AngularClientProxyExtensions
    {
        private const string SectionName = "ClientApplications";

        public static IServiceCollection AddAngularClientProxy(this IServiceCollection services, IConfiguration config)
        {
            // Add configuration
            var clientAppsSection = config.GetSection(SectionName);
            services.Configure<AngularClientOptions>(clientAppsSection);

            var options = clientAppsSection.Get<AngularClientOptions>();
            if (string.IsNullOrWhiteSpace(options.WebClientUri))
            {
                throw new InvalidOperationException($"Configuration value {SectionName}:{nameof(AngularClientOptions.WebClientUri)} was not provided.");
            }

            services.AddSingleton<IClientProxy, AngularClientProxy>();

            return services;
        }
    }
}
