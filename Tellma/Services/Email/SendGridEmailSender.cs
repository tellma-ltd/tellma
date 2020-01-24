using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Tellma.Services.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridOptions _config;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(SendGridOptions config, ILogger<SendGridEmailSender> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
        }

        public async Task SendEmailBulkAsync(List<string> tos, List<string> subjects, string htmlMessage, List<Dictionary<string, string>> substitutions, string fromEmail = null)
        {
            string fromName = "Tellma";
            fromEmail ??= _config.DefaultFromEmail;
            string sendGridApiKey = _config.ApiKey;

            var client = new SendGridClient(sendGridApiKey);
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

        public async Task SendEmailAsync(string email, string subject, string htmlMessage, string fromEmail = null)
        {
            await SendEmailBulkAsync(
                tos: new List<string> { email },
                subjects: new List<string> { subject },
                htmlMessage: htmlMessage,
                substitutions: new List<Dictionary<string, string>> { new Dictionary<string, string> { } },
                fromEmail: fromEmail
                );
        }
    }
}
