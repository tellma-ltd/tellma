using System;
using Tellma.Repository.Application;
using Tellma.Utilities.Sharding;
using Tellma.Utilities.Blobs;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationRepositoryExtensions
    {
        /// <summary>
        /// Registers the <see cref="IApplicationRepositoryFactory"/> providing access to tenant-specific
        /// application databases. This requires an implementation of <see cref="IShardResolver"/> to 
        /// be available in the DI since the repository acquires its connection string dynamically from it. <br/>
        /// This also adds a default implementation of <see cref="IBlobService"/> which reads and writes blobs from
        /// a table in the application database. This can be overridden with a different implementation.
        /// </summary>
        public static IServiceCollection AddApplicationRepository(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Add services
            services.AddSingleton<IApplicationRepositoryFactory, ApplicationRepositoryFactory>();
            services.TryAddSingleton<IBlobService, SqlBlobService>();

            return services;
        }
    }
}
