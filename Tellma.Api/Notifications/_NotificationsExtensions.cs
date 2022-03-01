using Microsoft.Extensions.Configuration;
using System;
using Tellma.Utilities.Sms;
using Tellma.Api.Notifications;
using Tellma.Utilities.Email;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NotificationsExtensions
    {
        private const string SectionName = "Notifications";

        /// <summary>
        /// Registeres the services that manage notification queues for email, SMS and push, 
        /// allowing for a fire-and-forget style of dispatching notifications. <br/>
        /// Note: To use this library you should register implementations of <see cref="IEmailSender"/> 
        /// and <see cref="ISmsSender"/> in the DI container.
        /// </summary>
        public static IServiceCollection AddNotifications(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // A dependency for this library
            services.AddInstances(config);

            // Add configuration
            var instancesSection = config.GetSection(SectionName);
            services.Configure<NotificationsOptions>(instancesSection);
            var options = instancesSection.Get<NotificationsOptions>();

            // Register background jobs
            services = services
                .AddHostedService<EmailJob>()
                .AddHostedService<EmailPollingJob>()
                .AddHostedService<SmsJob>()
                .AddHostedService<SmsPollingJob>();

            // These are helper services that business services rely on
            services = services
                // Notifications
                .AddSingleton<EmailQueue>()
                .AddSingleton<IEmailCallbackHandler, EmailCallbackHandler>()
                .AddSingleton<SmsQueue>()
                .AddSingleton<ISmsCallbackHandler, SmsCallbackHandler>()
                .AddSingleton<PushNotificationQueue>()
                .AddSingleton<NotificationsQueue>();

            // Add placeholder services
            services.TryAddSingleton<IEmailSender, NullEmailSender>();
            services.TryAddSingleton<ISmsSender, NullSmsSender>();

            // Automatic notifications
            if (options.EnableAutomaticNotifications)
            {
                services.AddSingleton<SchedulesCache>()
                    .AddHostedService<ScheduledNotificationsJob>()
                    .AddHostedService<SchedulesUpdaterJob>();
            }

            // Return
            return services;
        }
    }
}
