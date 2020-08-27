using Microsoft.Extensions.Configuration;
using System;
using Tellma.Services.Sms;
using Tellma.Services.Utilities;
using Twilio;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SmsServiceCollectionExtensions
    {
        private const string SECTION_NAME = "Twilio";

        public static IServiceCollection AddSms(this IServiceCollection services, bool enabled, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Get the Twilio Section
            var section = config.GetSection(SECTION_NAME);

            // Create TwilioOptions
            var opt = section.Get<TwilioOptions>();
            ValidateOptions(opt, enabled);

            // Set the Twilio Credentials
            TwilioClient.Init(opt.AccountSid, opt.AuthToken);

            // Bind SmsOptions
            services.Configure<SmsOptions>(section.GetSection(nameof(TwilioOptions.Sms)));

            // Register the services
            services
                .AddSingleton<TwilioSmsSender>()
                .AddSingleton<ISmsSenderFactory, SmsSenderFactory>()
                .AddSingleton<ISmsSender, SmsSender>();

            // REturn
            return services;
        }

        private static void ValidateOptions(TwilioOptions opt, bool enabled)
        {
            if(enabled)
            {
                // Perform some startup validation
                if (string.IsNullOrWhiteSpace(opt?.AccountSid))
                {
                    throw new InvalidOperationException($"{nameof(GlobalOptions.SmsEnabled)} = true, therefore a Twilio Account Sid must be in a configuration provider under the key '{SECTION_NAME}:{nameof(TwilioOptions.AccountSid)}'");
                }

                if (string.IsNullOrWhiteSpace(opt?.AuthToken))
                {
                    throw new InvalidOperationException($"{nameof(GlobalOptions.SmsEnabled)} = true, therefore a Twilio Auth Token must be in a configuration provider under the key '{SECTION_NAME}:{nameof(TwilioOptions.AuthToken)}'");
                }

                if (string.IsNullOrWhiteSpace(opt?.Sms?.ServiceSid))
                {
                    throw new InvalidOperationException($"{nameof(GlobalOptions.SmsEnabled)} = true, therefore a Twilio Messaging Service Sid must be in a configuration provider under the key '{SECTION_NAME}:{nameof(TwilioOptions.Sms)}:{nameof(SmsOptions.ServiceSid)}'");
                }
            }
        }
    }
}
