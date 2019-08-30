using BSharp.Services.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MultiTenancyServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the services that retrieve the tenant Id from the 
        /// request headers
        /// </summary>
        public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddHttpContextAccessor()
                .AddSingleton<ITenantIdAccessor, TenantIdAccessor>();

            return services;
        }
    }
}
