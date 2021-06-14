using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Tellma.Services.Utilities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClientAppServiceCollectionExtensions
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
