using BSharp.Controllers;
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

            return services
                .AddSingleton<IDefinitionsCache, DefinitionsCache>()
                .AddSingleton<IModelMetadataProvider, DefinitionsModelMetadataProvider>();
        }
    }
}
