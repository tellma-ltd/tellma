using System;
using Tellma.Api.ImportExport;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _ImportExportExtensions
    {
        /// <summary>
        /// Registers <see cref="DataParser"/> and <see cref="DataComposer"/> which API services
        /// rely on to import and export data from CSV and Excel files.
        /// </summary>
        public static IServiceCollection AddImportExport(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddScoped<DataParser>()
                .AddScoped<DataComposer>();
        }
    }
}
