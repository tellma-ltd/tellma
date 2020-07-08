using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Tellma.Services;
using Tellma.Services.Instrumentation;
using Tellma.Services.Utilities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InstrumentationServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the service that performs built-in performance instrumentation
        /// </summary>
        public static IServiceCollection AddInstrumentation(this IServiceCollection services, bool enabled, IConfiguration configSection = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (enabled)
            {
                if (configSection != null)
                {
                    // Add configuration
                    services.Configure<InstrumentationOptions>(configSection);
                }

                services.AddScoped<IInstrumentationService, InstrumentationService>();
            }
            else
            {
                services.AddScoped<IInstrumentationService, DoNothingService>();
            }

            return services;
        }

        /// <summary>
        /// Starts the instrumentation process, and adds the results in the response headers
        /// </summary>
        public static IApplicationBuilder UseInstrumentation(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                // Resolving this scoped service starts the instrumentation measurement
                var instrumentation = context.RequestServices.GetRequiredService<IInstrumentationService>();

                context.Response.OnStarting(() =>
                {
                    // The report contains the overall duration of the request as well an optional breakdown
                    var report = instrumentation.GetReport();
                    var serializedReport = JsonConvert.SerializeObject(report, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    context.Response.Headers.Add("x-instrumentation", serializedReport);

                    // return
                    return Task.CompletedTask;
                });

                await next.Invoke();
            });
        }

        /// <summary>
        /// Instruments how long the previous part of the middleware took, since the last call to <see cref="UseMiddlewareInstrumentation(IApplicationBuilder, string)"/>
        /// </summary>
        public static IApplicationBuilder UseMiddlewareInstrumentation(this IApplicationBuilder app, string name)
        {
            return app.Use(async (context, next) =>
            {
                var instrumentation = context.RequestServices.GetRequiredService<IInstrumentationService>();
                instrumentation.NextMiddleware(name);

                await next.Invoke();
            });
        }
    }
}
