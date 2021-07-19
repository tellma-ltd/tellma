using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Common;
using Tellma.Utilities.Sms;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Tellma.Utilities.Twilio
{
    public class TwilioSmsSender : ISmsSender
    {
        public const string MessageIdParamName = "message_id";
        public const string TenantIdParamName = "tenant_id";

        private readonly TwilioSmsOptions _options;
        private readonly ILogger _logger;
        private readonly Random _rand = new Random();

        public TwilioSmsSender(IOptions<TwilioSmsOptions> options, ILogger<TwilioSmsSender> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public async Task SendAsync(SmsToSend sms, CancellationToken cancellation = default)
        {
            var error = SmsValidation.Validate(sms);
            if (error != null)
            {
                throw new InvalidOperationException(error);
            }

            var serviceSid = !string.IsNullOrWhiteSpace(_options.ServiceSid) ? _options.ServiceSid : throw new InvalidOperationException("ServiceSid is missing.");

            // Extract the values from the argument
            var to = new PhoneNumber(sms.ToPhoneNumber);
            var message = sms.Message;
            var messageId = sms.MessageId;
            var tenantId = sms.TenantId;

            // Calculate the callbackUri (if required)
            Uri callbackUri = null;
            if (messageId != 0)
            {
                string hostname = _options.CallbackHost?.WithoutTrailingSlash() ?? throw new InvalidOperationException("CallbackHost is missing.");
                string stringUri = $"{hostname}/api/sms-callback?{MessageIdParamName}={messageId}";
                if (tenantId != 0)
                {
                    stringUri += $"&{TenantIdParamName}={tenantId}";
                }

                callbackUri = new Uri(stringUri);
            }

            // Exponential backoff
            const int maxAttempts = 5;
            const int maxBackoff = 25000; // 25 Seconds
            const int minBackoff = 1000; // 1 Second
            const int deltaBackoff = 1000; // 1 Second

            int attemptsSoFar = 0;
            int backoff = minBackoff;
            while (attemptsSoFar < maxAttempts && !cancellation.IsCancellationRequested)
            {
                attemptsSoFar++;

                try
                {
                    // Send using Twilio's Messaging Service
                    await MessageResource.CreateAsync(
                        body: message,
                        messagingServiceSid: serviceSid,
                        to: to,
                        statusCallback: callbackUri
                    );

                    break; // Success
                }
                catch (ApiException ex) when (ex.Status == (int)HttpStatusCode.TooManyRequests || ex.Status >= (int)HttpStatusCode.InternalServerError)
                {
                    // Twilio imposes a maximum number of concurrent calls, and returns 429 when that maximum is reached
                    // Here we implement exponential backoff to retry the call few more times before giving up as
                    // recommended here https://bit.ly/2CWYrjQ
                    if (attemptsSoFar < maxAttempts)
                    {
                        // Log warning
                        string logMessage = $"Twilio: {ex.Status} response after {attemptsSoFar} attempts with exponential backoff. Response Message: {ex.Message}.";
                        _logger.LogWarning(logMessage);

                        var randomOffset = _rand.Next(0, deltaBackoff);
                        await Task.Delay(backoff + randomOffset, cancellation);

                        // Double the backoff for next attempt
                        backoff = Math.Min(backoff * 2, maxBackoff);
                    }
                    else
                    {
                        throw; // Reached maxAttempts => Give up
                    }
                }
            }
        }
    }
}
