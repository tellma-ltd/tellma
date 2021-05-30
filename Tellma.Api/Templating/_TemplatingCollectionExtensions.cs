using System;
using Tellma.Api.Templating;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TemplatingCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="TemplateService"/> which controllers rely on to generate markup from templates.
        /// </summary>
        public static IServiceCollection AddMarkupTemplates(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddScoped<TemplateService>();
        }
    }
}
