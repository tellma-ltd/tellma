using System;
using Tellma.Repository.Application;
using Tellma.Utilities.Sharding;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationRepositoryExtensions
    {
        /// <summary>
        /// Registers the <see cref="IApplicationRepositoryFactory"/> providing access to tenant-specific
        /// application databases. This requires an implementation of <see cref="IShardResolver"/> to 
        /// be available in the DI since the repository acquires its connection string dynamically from it.
        /// </summary>
        public static IServiceCollection AddApplicationRepository(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Add services
            return services.AddSingleton<IApplicationRepositoryFactory, ApplicationRepositoryFactory>();
        }
    }
}
