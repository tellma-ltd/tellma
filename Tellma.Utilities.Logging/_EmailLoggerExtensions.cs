using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using Tellma.Utilities.Logging;

namespace Microsoft.Extensions.Logging
{
    public static class _EmailLoggerExtensions
    {
        private const string SectionName = "Logging:Email";
        private static readonly EmailAddressAttribute att = new ();

        public static ILoggingBuilder AddEmail(this ILoggingBuilder builder, IConfiguration config)
        {
            // Bind TwilioSmsOptions
            var section = config.GetSection(SectionName);
            builder.Services.Configure<EmailLoggerOptions>(section);

            var opt = section.Get<EmailLoggerOptions>();
            if (!string.IsNullOrWhiteSpace(opt.EmailAddress))
            {
                if (!att.IsValid(opt.EmailAddress))
                {
                    throw new InvalidOperationException($"The configuration value {SectionName}:{nameof(opt.EmailAddress)} is not a valid email address.");
                }

                builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, EmailLoggerProvider>());
            }

            return builder;
        }
    }
}
