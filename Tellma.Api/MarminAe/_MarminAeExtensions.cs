using Microsoft.Extensions.Configuration;
using System;
using Tellma.Api.MarminAe;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Registers the Marmin UAE e-invoicing integration. Mirrors <c>AddZatca</c>.
    /// </summary>
    /// <remarks>
    /// The glue lives here in <c>Tellma.Api</c> rather than inside
    /// <c>Tellma.Integration.MarminAe</c> on purpose: that project was copied in from another
    /// repository and is deliberately dependency-free, so keeping it free of DI, options and
    /// ASP.NET packages is what lets it stay diffable against its upstream original.
    /// </remarks>
    public static class MarminAeCollectionExtensions
    {
        private const string SectionName = "MarminAe";

        /// <summary>
        /// Registers <see cref="MarminAeService"/>, which provides access to the Marmin UAE
        /// e-invoicing API.
        /// </summary>
        public static IServiceCollection AddMarminAe(this IServiceCollection services, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(config);

            services.Configure<MarminAeOptions>(config.GetSection(SectionName));

            // A singleton, so the per-tenant client cache (and with it the bearer tokens those
            // clients hold) survives across requests. The vendor rate limits the token endpoint.
            services.AddHttpClient();
            services.AddSingleton<MarminAeService>();

            // Scoped, because it resolves a per-tenant repository per delivery. The webhook
            // middleware pulls it from ctx.RequestServices.
            services.AddScoped<IMarminAeCallbackHandler, MarminAeCallbackHandler>();

            return services;
        }
    }
}
