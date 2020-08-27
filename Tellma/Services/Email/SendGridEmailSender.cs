using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Reliability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridOptions _options;
        private readonly ILogger<SendGridEmailSender> _logger;
        private readonly HttpClient _client = new HttpClient(); // Singleton HttpClient to avoid memroy leaks https://bit.ly/2EGUgte

        public SendGridEmailSender(IOptions<SendGridOptions> options, ILogger<SendGridEmailSender> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public async Task SendBulkAsync(List<string> tos, List<string> subjects, string htmlMessage, List<Dictionary<string, string>> substitutions, string fromEmail = null)
        {
            string fromName = "Tellma";
            fromEmail ??= _options.DefaultFromEmail;
            string sendGridApiKey = _options.ApiKey;

            var client = new SendGridClient(_client, sendGridApiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var toAddresses = tos.Select(e => new EmailAddress(e)).ToList();


            var msg = MailHelper.CreateMultipleEmailsToMultipleRecipients(
                from, toAddresses, subjects, "", htmlMessage, substitutions);

            var response = await client.SendEmailAsync(msg);

            // Handle returned errors
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // SendGrid has a quota depending on your subscription, on a free account you only get 100 emails per day
                throw new InvalidOperationException("The SendGrid subscription configured in the system has reached its limit, please contact support");
            }

            if (response.StatusCode >= HttpStatusCode.BadRequest)
            {
                string responseMessage = await response.Body.ReadAsStringAsync();
                _logger.LogError($"Error sending email through SendGrid, Status Code: {response.StatusCode}, Message: {responseMessage}");

                throw new InvalidOperationException($"The SendGrid API returned an unknown error {response.StatusCode} when trying to send the email through it");
            }
        }

        public async Task SendAsync(string email, string subject, string htmlMessage, string fromEmail = null)
        {
            var from = new EmailAddress(fromEmail ?? _options.DefaultFromEmail, "Tellma ERP");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            await SendEmailAsync(msg, default);

            //await SendEmailBulkAsync(
            //    tos: new List<string> { email },
            //    subjects: new List<string> { subject },
            //    htmlMessage: htmlMessage,
            //    substitutions: new List<Dictionary<string, string>> { new Dictionary<string, string> { } },
            //    fromEmail: fromEmail
            //    );
        }

        private async Task<string> SendEmailAsync(SendGridMessage msg, CancellationToken cancellation)
        {
            var options = new SendGridClientOptions
            {
                 ApiKey = _options.ApiKey,
                 ReliabilitySettings = new ReliabilitySettings(
                     maximumNumberOfRetries: 5, 
                     minimumBackoff: TimeSpan.FromSeconds(1), 
                     maximumBackOff: TimeSpan.FromSeconds(25),
                     deltaBackOff: TimeSpan.FromSeconds(1))
            };

            var client = new SendGridClient(_client, options);
            Response response = await client.SendEmailAsync(msg, cancellation);

            if (cancellation.IsCancellationRequested)
            {
                // Doesn't matter since the request was cancelled
                return "";
            }
            else
            {
                return response.Headers.GetValues("X-Message-Id").FirstOrDefault();
            }
        }
    }
}
