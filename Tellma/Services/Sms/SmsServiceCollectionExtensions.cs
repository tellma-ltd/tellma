using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using Tellma.Services.Sms;
using Tellma.Services.Utilities;
using Twilio;
using Twilio.Security;

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

        public static IApplicationBuilder UseSmsCallback(this IApplicationBuilder thisApp, IConfiguration config)
        {
            // Get the Twilio Section
            var section = config.GetSection(SECTION_NAME);
            var opt = section.Get<TwilioOptions>();

            var twilioRequestValidator = new RequestValidator(opt.AuthToken);

            // This configures a callback endpoint for Twilio event webhooks
            if (opt?.Sms?.CallbacksEnabled ?? false)
            {
                thisApp = thisApp.Map("/api/sms-callback", (app) =>
                {
                    app.Run(async ctx =>
                    {
                        var req = ctx.Request;
                        var res = ctx.Response;

                        var handler = ctx.RequestServices.GetService<ISmsCallbackHandler>();
                        if (handler == null)
                        {
                            // Helps during configuration for making sure the endpoint is accessible
                            res.StatusCode = StatusCodes.Status400BadRequest;
                            await res.WriteAsync($"No implementation of {nameof(ISmsCallbackHandler)} was registered");
                        }
                        else if (req.Method == "GET")
                        {
                            // Helps during configuration for making sure the endpoint is accessible
                            res.StatusCode = StatusCodes.Status200OK;
                            await res.WriteAsync("Welcome to the sms callback endpoint for Twilio webhooks!");
                        }
                        else if (req.Method != "POST")
                        {
                            // Twilio must POST to this endpoint
                            res.StatusCode = StatusCodes.Status405MethodNotAllowed;
                            await res.WriteAsync($"{req.Method} method is not supported.");
                        } 
                        else
                        {
                            var requestUrl = $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
                            var parameters = req.Form.ToDictionary(e => e.Key, e => req.Form[e.Key].ToString());
                            var signature = req.Headers["X-Twilio-Signature"];

                            if (signature == StringValues.Empty || !twilioRequestValidator.Validate(requestUrl, parameters, signature))
                            {
                                res.StatusCode = StatusCodes.Status401Unauthorized;
                                await res.WriteAsync("Invalid signature.");
                            }
                            else
                            {
                                // Pull out all the needed info from the webhook event
                                var twilioStatus = req.Form["MessageStatus"].ToString();
                                var tenantIdString = req.Query["tenant_id"].ToString();
                                var messageIdString = req.Query["message_id"].ToString();

                                // TODO: if messageId is null, don't continue

                                if (int.TryParse(tenantIdString, out int tenantId))
                                {
                                    res.StatusCode = StatusCodes.Status400BadRequest;
                                    await res.WriteAsync($"Could not parse tenant ID {tenantIdString} into a valid integer.");
                                }
                                else if (int.TryParse(messageIdString, out int messageId))
                                {
                                    res.StatusCode = StatusCodes.Status400BadRequest;
                                    await res.WriteAsync($"Could not parse message ID {messageId} into a valid integer.");
                                }

                                // https://www.twilio.com/docs/sms/api/message-resource#message-status-values
                                switch (twilioStatus)
                                {
                                    // No point tracking those, TMI
                                    case "accepted": // Twilio accepted it
                                    case "queued": // Twilio assigned a from number and queued it
                                    case "sending": // Twilio started sending it

                                    // Tracked
                                    case "sent": // Twilio sent it (treat as final after 72h)
                                    case "failed": // Twilio failed to send it (no charges)
                                    case "delivered": // The carrier delivered it
                                    case "undelivered": // The carrier failed to deliver it (charges)

                                    // Never used
                                    case "receiving": // Only for inbound SMS
                                    case "received": // Only for inbound SMS
                                    case "read": // Only for whatsapp

                                    default:
                                        break;
                                }
                            }
                        }
                    });
                });
            }

            return thisApp;
        }

        /// <summary>
        /// Helper method
        /// </summary>
        private static void ValidateOptions(TwilioOptions opt, bool enabled)
        {
            if (enabled)
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

                bool webhooksEnabled = opt?.Sms?.CallbacksEnabled ?? false;
                if (webhooksEnabled)
                {
                    if (string.IsNullOrWhiteSpace(opt?.Sms?.CallbackHost))
                    {
                        throw new InvalidOperationException($"{SECTION_NAME}:{nameof(TwilioOptions.Sms)}:{nameof(SmsOptions.CallbacksEnabled)} = true, therefore the host name of the system must be in a configuration provider under the key '{SECTION_NAME}:{nameof(TwilioOptions.Sms)}:{nameof(SmsOptions.CallbackHost)}'");
                    }
                }
            }
        }
    }
}
