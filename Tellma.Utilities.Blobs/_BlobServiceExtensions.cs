using Tellma.Utilities.Blobs;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _BlobServiceExtensions
    {
        private const string SectionName = "AzureBlobStorage";

        /// <summary>
        /// Registers the implementation of <see cref="IBlobService"/> which stores and retrieves
        /// binary blobs from the Azure Blob Storage.
        /// </summary>
        public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var section = config.GetSection(SectionName);
            services.Configure<AzureBlobStorageOptions>(section);

            // Some startup validation
            var opt = section.Get<AzureBlobStorageOptions>();
            ValidateOptions(opt);

            services.AddSingleton<IBlobService, AzureBlobStorageService>();

            return services;
        }

        /// <summary>
        /// Helper function.
        /// </summary>
        private static void ValidateOptions(AzureBlobStorageOptions opt)
        {
            // Scream for missing yet required stuff
            if (string.IsNullOrWhiteSpace(opt?.ConnectionString))
            {
                string key = $"{SectionName}:{nameof(AzureBlobStorageOptions.ConnectionString)}";
                throw new InvalidOperationException(
                    $"Azure Blob Storage is enabled, therefore a connection string must be in a configuration provider under the key '{key}'.");
            }
        }
    }
}
