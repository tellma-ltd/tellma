using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BSharp.Services.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridConfiguration _config;

        public SendGridEmailSender(SendGridConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage, string fromEmail = null)
        {
            // Read from the configuration provider
            string fromName = "BSharp ERP";
            fromEmail = fromEmail ?? _config.DefaultFromEmail;
            string sendGridApiKey = _config.ApiKey;


            // Prepare the message
            var client = new SendGridClient(sendGridApiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(email);
            SendGridMessage message =
                MailHelper.CreateSingleEmail(
                            from: from,
                            to: to,
                            subject: subject,
                            plainTextContent: "",
                            htmlContent: htmlMessage);

            // Send the message
            var response = await client.SendEmailAsync(message);

            // Handle returned errors
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // SendGrid has a quota depending on your subscription, on a free account you only get 100 emails per day
                throw new InvalidOperationException("The SendGrid subscription configured in the system has reached its limit, please contact support");
            }

            if (response.StatusCode >= HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"The SendGrid API returned an unknown error {response.StatusCode} when trying to send the email through it");
            }

            else return;

        }
    }
}
