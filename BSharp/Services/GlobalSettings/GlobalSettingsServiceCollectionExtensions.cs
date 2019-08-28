using BSharp.Services.GlobalSettings;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GlobalSettingsServiceCollectionExtensions
    {
        /// <summary>
        /// The global settings are application-wide, and they live in the admin database, in large SAAS installations
        /// the admin database will become a bottle-neck, so here we introduce memory caching to relieve the congestion
        /// </summary>
        public static IServiceCollection AddGlobalSettingsCache(this IServiceCollection services, IConfiguration configSection)
        {
            if(services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if(configSection != null)
            {
                services.Configure<GlobalSettingsCacheOptions>(configSection);
            }

            // Register DI service
            services.AddSingleton<IGlobalSettingsCache, GlobalSettingsCache>();
            return services;
        }
    }
}
