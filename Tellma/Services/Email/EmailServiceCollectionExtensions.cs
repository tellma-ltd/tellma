using Microsoft.Extensions.Configuration;
using System;
using Tellma.Services.Email;
using Tellma.Services.Utilities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EmailServiceCollectionExtensions
    {
        private const string SECTION_NAME = "Email";

        public static IServiceCollection AddEmail(this IServiceCollection services, bool enabled, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Add configuration
            var emailSection = config.GetSection(SECTION_NAME);

            // Some startup validation
            var opt = emailSection.Get<EmailOptions>();
            ValidateOptions(opt, enabled);


            // Bind SendGridOptions
            var sendGridSection = emailSection.GetSection(nameof(EmailOptions.SendGrid));
            services.Configure<SendGridOptions>(sendGridSection);

            // Register the services
            services.AddSingleton<SendGridEmailSender>();
            services.AddSingleton<IEmailSenderFactory, EmailSenderFactory>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<EmailTemplatesProvider>();

            return services;
        }

        private static void ValidateOptions(EmailOptions opt, bool enabled)
        {
            if (enabled)
            {
                // Scream for missing yet required stuff
                if (string.IsNullOrWhiteSpace(opt?.SendGrid?.ApiKey))
                {
                    throw new InvalidOperationException($"{nameof(GlobalOptions.EmailEnabled)} is set to true, therefore a SendGrid API Key must be in a configuration provider under the key '{SECTION_NAME}:{nameof(EmailOptions.SendGrid)}:{nameof(SendGridOptions.ApiKey)}', you can get a free key on https://sendgrid.com/");
                }
            }
        }
    }
}
