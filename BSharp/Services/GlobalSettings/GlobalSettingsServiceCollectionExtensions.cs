using BSharp.Services.GlobalSettings;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GlobalSettingsServiceCollectionExtensions
    {
        /// <summary>
        /// The global settings (e.g. active cultures) are application-wide, and they live in the manager database, in large installations
        /// the manager database will become a bottle-neck, so here we introduce memory caching to relieve the congestion
        /// </summary>
        public static IServiceCollection AddGlobalSettingsCache(this IServiceCollection services, IConfiguration config)
        {
            if(services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if(config != null)
            {
                services.Configure<GlobalSettingsCacheConfiguration>(config.GetSection("GlobalSettingsCache"));
            }

            // Register DI service
            services.AddSingleton<IGlobalSettingsCache, GlobalSettingsCache>();
            return services;
        }
    }
}
