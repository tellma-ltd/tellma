using Tellma.Utilities.Blobs;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _BlobServiceExtensions
    {
        /// <summary>
        /// Registers the <see cref="IBlobService"/> which allows saving and retrieving of
        /// binary blobs either to Azure Blob Storage or to SQL Server table depending on configuration.
        /// </summary>
        public static IServiceCollection AddBlobService(this IServiceCollection services, IConfiguration config = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if(config != null)
            {
                services.Configure<BlobServiceOptions>(config);
            }

            services.AddSingleton<IBlobServiceFactory, BlobServiceFactory>();
            services.AddSingleton<IBlobService, BlobService>();

            return services;
        }
    }
}
