using System;
using Tellma.Api.Base;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _ApiBaseExtensions
    {
        /// <summary>
        /// Registers all the services in the tellma API base and their dependencies:<br/>
        /// 1 - Metadata<br/>
        /// 2 - Import/Export<br/>
        /// 3 - Templating<br/>
        /// </summary>
        public static IServiceCollection AddTellmaApiBase(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // (1) Add infrastructure services
            services = services
                .AddMetadata()
                .AddImportExport()
                .AddTemplating();

            // (2) Add base service dependencies
            return services
                .AddScoped<FactServiceDependencies>()
                .AddScoped<CrudServiceDependencies>();
        }
    }
}