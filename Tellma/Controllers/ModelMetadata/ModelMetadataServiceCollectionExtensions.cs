using Tellma.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModelMetadataServiceCollectionExtensions
    {
        public static IServiceCollection AddDefinitionsModelMetadata(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            //return services.AddSingleton<MetadataProvider>();

            return services
               // .AddSingleton<MetadataProvider>()
                .AddSingleton<IDefinitionsCache, DefinitionsCache>()
                .AddSingleton<ISettingsCache, SettingsCache>()

                // TODO: Delete
                .AddSingleton<IModelMetadataProvider, DefinitionsModelMetadataProvider>();
        }
    }
}
