using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Tellma.Services.ClientApp;

namespace Microsoft.Extensions.DependencyInjection
{
    // TODO: Integrate with the rest of the folder

    public static class _ClientAppExtensions
    {
        public static IServiceCollection AddClientAppAddressResolver(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ClientAppAddressResolver>();

            return services;
        }
    }
}
