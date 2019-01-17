using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.ModelMetadata
{
    public static class ModelMetadataServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamicModelMetadata(this IServiceCollection services)
        {
            return services.AddSingleton<IModelMetadataProvider, DynamicModelMetadataProvider>();
        }
    }
}
