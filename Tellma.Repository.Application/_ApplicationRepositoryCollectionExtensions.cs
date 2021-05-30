using System;
using Tellma.Repository.Application;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationRepositoryCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="IApplicationRepositoryFactory"/> providing access to application databases.
        /// </summary>
        public static IServiceCollection AddAdminRepository(this IServiceCollection services)
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
