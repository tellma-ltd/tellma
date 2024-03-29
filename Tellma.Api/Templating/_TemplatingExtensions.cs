﻿using System;
using Tellma.Api.Templating;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _TemplatingExtensions
    {
        /// <summary>
        /// Registers the <see cref="TemplateService"/> which API services rely on to generate strings from templates.
        /// </summary>
        public static IServiceCollection AddTemplating(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton<TemplateService>();
        }
    }
}
