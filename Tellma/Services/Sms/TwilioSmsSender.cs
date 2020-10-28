using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Services.Utilities;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Tellma.Services.Sms
{
    public class TwilioSmsSender : ISmsSender
    {
        public const string MessageIdParamName = "message_id";
        public const string TenantIdParamName = "tenant_id";

        private readonly SmsOptions _options;
        private readonly ILogger _logger;
        private readonly Random _rand = new Random();

        public TwilioSmsSender(IOptions<SmsOptions> options, ILogger<TwilioSmsSender> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public async Task SendAsync(SmsMessage sms, CancellationToken cancellation = default)
        {
            var serviceSid = !string.IsNullOrWhiteSpace(_options.ServiceSid) ? _options.ServiceSid : throw new InvalidOperationException("ServiceSid is missing");

            // Extract the values from the argument
            var to = new PhoneNumber(sms.ToPhoneNumber);
            var message = sms.Message;
            var messageId = sms.MessageId;
            var tenantId = sms.TenantId;

            // Calculate the callbackUri (if required)
            Uri callbackUri = null;
            if (messageId != null)
            {
                string hostname = _options.CallbackHost?.WithoutTrailingSlash() ?? throw new InvalidOperationException("CallbackHost is missing");
                string stringUri = $"{hostname}/api/sms-callback?{MessageIdParamName}={messageId}";
                if (tenantId != null)
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
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    attemptsSoFar++;

                    // Send using Twilio's Messaging Service
                    await MessageResource.CreateAsync(
                        body: message,
                        messagingServiceSid: serviceSid,
                        to: to,
                        statusCallback: callbackUri
                    );
                }
                catch (ApiException ex) when (ex.Status == (int)HttpStatusCode.TooManyRequests || ex.Status >= 500)
                {
                    // Twilio imposes a maximum number of concurrent calls, and returns 429 when that maximum is reached
                    // Here we implement exponential backoff to retry the call few more times before giving up as
                    // recommended here https://bit.ly/2CWYrjQ
                    if (attemptsSoFar < maxAttempts)
                    {
                        var randomOffset = _rand.Next(0, deltaBackoff);
                        await Task.Delay(backoff + randomOffset, cancellation);

                        // Double the backoff for next attempt
                        backoff = Math.Min(backoff * 2, maxBackoff);
                    }
                    else
                    {
                        _logger.LogError($"Twilio: 429 Too Many Requests even after {attemptsSoFar} attempts with exponential backoff.");

                        throw; // Give up
                    }
                }
            }
        }

        #region Bulk SMS

        //public async Task<string> BulkSendAsync(IEnumerable<string> phoneNumbers, string sms, CancellationToken _)
        //{
        //    var serviceSid = !string.IsNullOrWhiteSpace(_smsOpt.ServiceSid) ? _smsOpt.ServiceSid : throw new InvalidOperationException("ServiceSid is missing");
        //    var binding = phoneNumbers.Select(to => $"{{\"binding_type\":\"sms\",\"address\":\"{to}\"}}").ToList();

        //    // Send the SMS through the Twilio API
        //    var notification = await Twilio.Rest.Notify.V1.Service.NotificationResource.CreateAsync(serviceSid, toBinding: binding, body: sms);

        //    return notification.Sid;
        //}

        #endregion
    }
}
