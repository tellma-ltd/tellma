using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.BlobStorage
{
    public static class BlobServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, Action<AzureBlobStorageConfiguration> action)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Options pattern
            services.Configure(action);
            return services.AddScoped<IBlobService, AzureBlobStorageService>();
        }

        public static IServiceCollection AddSqlTableBlobStorage(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Options pattern
            return services.AddScoped<IBlobService, SqlTableBlobService>();
        }
    }
}
