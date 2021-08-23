using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SendGrid.Helpers.EventWebhook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tellma.Utilities.Email;
using Tellma.Utilities.SendGrid;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SendGridExtensions
    {
        private const string SectionName = "Email:SendGrid";

        /// <summary>
        /// Registers the implementation of <see cref="IEmailSender"/> that sends emails through the SendGrid service.
        /// </summary>
        public static IServiceCollection AddSendGrid(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Add configuration
            var sgSection = config.GetSection(SectionName);
            services.Configure<SendGridOptions>(sgSection);

            // Some startup validation
            var opt = sgSection.Get<SendGridOptions>();
            ValidateOptions(opt);

            // Register the services
            services.AddSingleton<IEmailSender, SendGridEmailSender>();

            // For better management of HttpClients
            services.AddHttpClient();

            return services;
        }

        /// <summary>
        /// Helper function.
        /// </summary>
        private static void ValidateOptions(SendGridOptions opt)
        {
            // Scream for missing yet required stuff
            if (string.IsNullOrWhiteSpace(opt?.ApiKey))
            {
                throw new InvalidOperationException(
                    $"Email is enabled, therefore a SendGrid API Key must be in a configuration provider under the key '{SectionName}:{nameof(SendGridOptions.ApiKey)}', you can get a free key at https://sendgrid.com/.");
            }

            bool webhooksEnabled = opt?.CallbacksEnabled ?? false;
            if (webhooksEnabled)
            {
                if (string.IsNullOrWhiteSpace(opt?.VerificationKey))
                {
                    throw new InvalidOperationException(
                        $"{SectionName}:{nameof(SendGridOptions.CallbacksEnabled)} = true, therefore the webhook callback verification key must be in a configuration provider under the key '{SectionName}:{nameof(SendGridOptions.VerificationKey)}'.");
                }

                // Add the public key?
            }
        }

        /// <summary>
        /// Middleware that listens to the endpoint "/api/email-callback" for webhook events posted from SendGrid. <br/>
        /// In order to use this middleware you must first register an implementation of <see cref="IEmailCallbackHandler"/> in the DI container.
        /// </summary>
        public static IApplicationBuilder UseSendGridCallback(this IApplicationBuilder app, IConfiguration config)
        {
            // Get the Twilio Section
            var section = config.GetSection(SectionName);
            var opt = section.Get<SendGridOptions>();

            if (opt?.CallbacksEnabled ?? false)
            {
                var sgRequestValidator = new RequestValidator();
                var publicKey = sgRequestValidator.ConvertPublicKeyToECDSA(opt.VerificationKey);

                app = app.Map("/api/email-callback", (app) =>
                {
                    app.Run(async ctx =>
                    {
                        var req = ctx.Request;
                        var res = ctx.Response;
                        var cancellation = ctx.RequestAborted;

                        var handler = ctx.RequestServices.GetService<IEmailCallbackHandler>();
                        if (handler == null)
                        {
                            // Helps during configuration for making sure the endpoint is working
                            res.StatusCode = StatusCodes.Status200OK;
                            await res.WriteAsync($"No implementation of {nameof(IEmailCallbackHandler)} was registered.", cancellation);
                        }
                        else if (req.Method == "GET")
                        {
                            // Helps during configuration for making sure the endpoint is accessible
                            res.StatusCode = StatusCodes.Status200OK;
                            await res.WriteAsync("Welcome to the email callback endpoint for SendGrid webhooks!");
                        }
                        else if (req.Method != "POST")
                        {
                            // SendGrid will POST to this endpoint
                            res.StatusCode = StatusCodes.Status405MethodNotAllowed;
                            await res.WriteAsync($"{req.Method} method is not supported.");
                        }
                        else
                        {
                            // Get signature and timestamp from headers
                            var signature = req.Headers[RequestValidator.SIGNATURE_HEADER];
                            var timestamp = req.Headers[RequestValidator.TIMESTAMP_HEADER];

                            // Read the body
                            string body;
                            using (var sr = new StreamReader(req.Body))
                            {
                                body = await sr.ReadToEndAsync();
                            }

                            // Authenticate the source as SendGrid
                            if (signature == StringValues.Empty || timestamp == StringValues.Empty || !sgRequestValidator.VerifySignature(publicKey, body, signature, timestamp))
                            {
                                res.StatusCode = StatusCodes.Status401Unauthorized;
                                await res.WriteAsync("Invalid signature.", cancellation);
                            }
                            else
                            {
                                // Decode the webhook event into a list of DTOs
                                List<SendGridEventNotification> sgEventNotifications;
                                try
                                {
                                    sgEventNotifications = JsonConvert.DeserializeObject<List<SendGridEventNotification>>(body) ?? new List<SendGridEventNotification>();
                                }
                                catch
                                {
                                    res.StatusCode = StatusCodes.Status422UnprocessableEntity;
                                    await res.WriteAsync("Failed to parse the body contents.", cancellation);
                                    return;
                                }

                                try
                                {
                                    // Map the SendGrid events to EmailEventNotifications
                                    var emailEventNotifications = new List<EmailEventNotification>(sgEventNotifications.Count);
                                    foreach (var sgEventNotification in sgEventNotifications)
                                    {
                                        int emailId = sgEventNotification.EmailId;
                                        int? tenantId = sgEventNotification.TenantId;
                                        string error = sgEventNotification.Reason;
                                        DateTimeOffset eventTimestamp = sgEventNotification.Timestamp != 0 ? DateTimeOffset.FromUnixTimeSeconds(sgEventNotification.Timestamp) : DateTimeOffset.Now;

                                        EmailEvent emailEvent;
                                        var sgEvent = sgEventNotification.Event;

                                        // https://sendgrid.com/docs/for-developers/tracking-events/event/
                                        switch (sgEvent)
                                        {
                                            // Tracked
                                            case "dropped": // SG rejected it (spam, unsubscribe)
                                                emailEvent = EmailEvent.Dropped;
                                                break;
                                            case "delivered": // Recipient server accepted it
                                                emailEvent = EmailEvent.Delivered;
                                                break;
                                            case "bounce": // Recipient server rejected it (type = "bounce" if permanently or "blocked" if temporarily)
                                                emailEvent = EmailEvent.Bounce;
                                                break;

                                            // Engagement
                                            case "open": // User opened the email
                                                emailEvent = EmailEvent.Open;
                                                break;
                                            case "click": // User clicked a link in the email
                                                emailEvent = EmailEvent.Click;
                                                break;
                                            case "spamreport": // User marked email as spam
                                                emailEvent = EmailEvent.SpamReport;
                                                break;

                                            // No point tracking those, TMI
                                            case "processed": // SG accepted it
                                            case "deferred": // Recipient server temporary unavailable (SG retries up to 72h)

                                            // Never used
                                            case "unsubscribe": // Only when SG subscription mgmt features are enabled
                                            case "group_unsubscribe": // Only when SG subscription mgmt features are enabled
                                            case "group_resubscribe": // Only when SG subscription mgmt features are enabled

                                            default:
                                                // Nothing to handle
                                                continue;
                                        }

                                        emailEventNotifications.Add(new EmailEventNotification
                                        {
                                            Event = emailEvent,
                                            EmailId = emailId,
                                            TenantId = tenantId,
                                            Error = error,
                                            Timestamp = eventTimestamp
                                        });
                                    }

                                    if (emailEventNotifications.Any())
                                    {
                                        // Custom handler
                                        await handler.HandleCallback(emailEventNotifications, cancellation);
                                    }

                                    // Return 200 upon success
                                    res.StatusCode = StatusCodes.Status200OK;
                                }
                                catch (Exception)
                                {
                                    // Log the error
                                    res.StatusCode = StatusCodes.Status400BadRequest;
                                    await res.WriteAsync("Failed to process the events.");
                                }
                            }
                        }
                    });
                });
            }

            return app;
        }
    }
}
