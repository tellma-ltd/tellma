using BSharp.Services.FilterParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FilterParserServiceCollectionExtensions
    {
        public static IServiceCollection AddFilterParser(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Add filter parser service
            services.AddSingleton<IFilterParser, FilterParser>();

            // return
            return services;
        }
    }
}
