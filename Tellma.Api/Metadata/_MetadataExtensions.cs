using System;
using Tellma.Api.Metadata;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _MetadataExtensions
    {
        /// <summary>
        /// Registers <see cref="MetadataProvider"/> that API services rely on to
        /// cache and retrieve the metadata of entities and properties in the model
        /// such as display values, validation, parsing and formatting into strings
        /// and user keys all of which can be different from tenant to tenant or from
        /// definitionId to definitionId.
        /// </summary>
        public static IServiceCollection AddMetadata(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddScoped<MetadataProvider>();
        }
    }
}
