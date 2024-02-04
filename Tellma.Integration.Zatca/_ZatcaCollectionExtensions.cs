using Microsoft.Extensions.Configuration;
using Tellma.Integration.Zatca;

namespace Microsoft.Extensions.DependencyInjection
{ 
    public static class ZatcaCollectionExtensions
    {
        private const string SectionName = "Zatca";

        /// <summary>
        /// Registers the <see cref="ZatcaService"/> providing access to ZATCA e-invoice integration functionality.
        /// </summary>
        public static IServiceCollection AddZatca(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var zatcaSection = config.GetSection(SectionName);
            services.Configure<ZatcaOptions>(zatcaSection);

            // Add services
            services.AddHttpClient();
            services.AddSingleton<ZatcaService>();

            return services;
        }
    }
}
