using BSharp.Services.Email;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EmailServiceCollectionExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, Action<EmailOptions> action)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            services.Configure(action);
            return services.AddEmail(config: null);
        }

        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration config = null)
        {
            throw new Exception("Your ConfigureServicesError trick works!");

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config != null)
            {
                // Add configuration
                services.Configure<EmailOptions>(config);
            }

            // Register the services
            services.AddSingleton<IEmailSenderFactory, EmailSenderFactory>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<EmailTemplatesProvider>();

            return services;
        }
    }
}
