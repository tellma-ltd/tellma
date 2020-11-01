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

        public static IApplicationBuilder UseSmsCallback(this IApplicationBuilder bldr, IConfiguration config)
        {
            // Get the Twilio Section
            var section = config.GetSection(SECTION_NAME);
            var opt = section.Get<TwilioOptions>();

            // This configures a callback endpoint for Twilio event webhooks
            if (opt?.Sms?.CallbacksEnabled ?? false)
            {
                var twilioRequestValidator = new RequestValidator(opt.AuthToken);

                bldr = bldr.Map("/api/sms-callback", (app) =>
                {
                    app.Run(async ctx =>
                    {
                        var req = ctx.Request;
                        var res = ctx.Response;
                        var cancellation = ctx.RequestAborted;

                        var handler = ctx.RequestServices.GetService<ISmsCallbackHandler>();
                        if (handler == null)
                        {
                            // Helps during configuration for making sure the endpoint is working
                            res.StatusCode = StatusCodes.Status200OK;
                            await res.WriteAsync($"No implementation of {nameof(ISmsCallbackHandler)} was registered.", cancellation);
                        }
                        else if (req.Method == "GET")
                        {
                            // Helps during configuration for making sure the endpoint is accessible
                            res.StatusCode = StatusCodes.Status200OK;
                            await res.WriteAsync("Welcome to the sms callback endpoint for Twilio webhooks!", cancellation);
                        }
                        else if (req.Method != "POST")
                        {
                            // Twilio must POST to this endpoint
                            res.StatusCode = StatusCodes.Status405MethodNotAllowed;
                            await res.WriteAsync($"{req.Method} method is not supported.", cancellation);
                        }
                        else
                        {
                            var requestUrl = $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
                            var parameters = req.Form.ToDictionary(e => e.Key, e => req.Form[e.Key].ToString());
                            var signature = req.Headers["X-Twilio-Signature"];

                            if (signature == StringValues.Empty || !twilioRequestValidator.Validate(requestUrl, parameters, signature))
                            {
                                // Call is not coming from Twilio
                                res.StatusCode = StatusCodes.Status401Unauthorized;
                                await res.WriteAsync("Invalid signature.", cancellation);
                            }
                            else
                            {
                                // Extract all the needed info from the webhook event
                                var twilioStatus = req.Form["MessageStatus"].ToString();
                                var tenantIdString = req.Query[TwilioSmsSender.TenantIdParamName].ToString();
                                var messageIdString = req.Query[TwilioSmsSender.MessageIdParamName].ToString();

                                // Parsing and validation
                                if (string.IsNullOrWhiteSpace(messageIdString))
                                {
                                    // Callback is pointless without the message_id, but we return 200 OK
                                    return;
                                }

                                if (!int.TryParse(messageIdString, out int messageId))
                                {
                                    // Message Id should be an integer
                                    res.StatusCode = StatusCodes.Status400BadRequest;
                                    await res.WriteAsync($"Could not parse message ID {messageId} into a valid integer.", cancellation);
                                    return;
                                }

                                int? tenantId = null;
                                if (!string.IsNullOrWhiteSpace(tenantIdString))
                                {
                                    if (int.TryParse(tenantIdString, out int tenantIdInt))
                                    {
                                        tenantId = tenantIdInt;
                                    }
                                    else
                                    {
                                        // Tenant Id (if any) should be an integer
                                        res.StatusCode = StatusCodes.Status400BadRequest;
                                        await res.WriteAsync($"Could not parse tenant ID {tenantIdString} into a valid integer.", cancellation);
                                        return;
                                    }
                                }

                                // https://bit.ly/32K9o1j
                                SmsEvent type;
                                switch (twilioStatus)
                                {
                                    // Tracked
                                    case "sent": // Twilio sent it (treat as final after 72h)
                                        type = SmsEvent.Sent;
                                        break;
                                    case "failed": // Twilio failed to send it (no charges)
                                        type = SmsEvent.Failed;
                                        break;
                                    case "delivered": // The carrier delivered it
                                        type = SmsEvent.Delivered;
                                        break;
                                    case "undelivered": // The carrier failed to deliver it (charges apply)
                                        type = SmsEvent.Undelivered;
                                        break;

                                    // No point tracking those, TMI
                                    case "accepted": // Twilio accepted it
                                    case "queued": // Twilio assigned a from number and queued it
                                    case "sending": // Twilio started sending it

                                    // Never used
                                    case "receiving": // Only for inbound SMS
                                    case "received": // Only for inbound SMS
                                    case "read": // Only for whatsapp
                                    default:
                                        // Nothing to handle, return OK 200
                                        res.StatusCode = StatusCodes.Status200OK;
                                        return;
                                }

                                // Create the event and handle it with custom behavior
                                var smsEvent = new SmsEventNotification
                                {
                                    MessageId = messageId,
                                    TenantId = tenantId,
                                    Event = type,
                                    Timestamp = DateTimeOffset.Now
                                };

                                // Custom handler
                                await handler.HandleCallback(smsEvent, cancellation);

                                // Return 200 upon success
                                res.StatusCode = StatusCodes.Status200OK;
                            }
                        }
                    });
                });
            }

            return bldr;
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
