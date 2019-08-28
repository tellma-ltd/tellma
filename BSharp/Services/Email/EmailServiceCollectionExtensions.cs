using BSharp.Services.Email;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EmailServiceCollectionExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configSection = null)
        {

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configSection != null)
            {
                // Add configuration
                services.Configure<EmailOptions>(configSection);
            }

            // Register the services
            services.AddSingleton<IEmailSenderFactory, EmailSenderFactory>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<EmailTemplatesProvider>();

            return services;
        }
    }
}
