using BSharp.Services.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ODataServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the odata query factory that is used to translate odata web requests into SQL
        /// </summary>
        public static IServiceCollection AddOData(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton<IODataQueryFactory, ODataQueryFactory>();
        }
    }
}
