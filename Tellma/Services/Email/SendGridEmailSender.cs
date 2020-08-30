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
        private readonly HttpClient _httpClient = new HttpClient(); // Singleton HttpClient to avoid memroy leaks https://bit.ly/2EGUgte
        private readonly Random _rand = new Random();

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

            var client = new SendGridClient(_httpClient, sendGridApiKey);
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
        }

        private async Task SendEmailAsync(SendGridMessage msg, CancellationToken cancellation)
        {
            var client = new SendGridClient(_httpClient, _options.ApiKey); // Reuse the HttpClient to avoid a memory leak

            // Exponential backoff (There is a built-in implementation in SG library but it doesn't handle 429)
            const int maxAttempts = 5;
            const int maxBackoff = 25000; // 25 Seconds
            const int minBackoff = 1000; // 1 Second
            const int deltaBackoff = 1000; // 1 Second

            int attemptsSoFar = 0;
            int backoff = minBackoff;

            while (true)
            {
                Response response = await client.SendEmailAsync(msg, cancellation);
                if (response.StatusCode == HttpStatusCode.TooManyRequests || response.StatusCode >= HttpStatusCode.InternalServerError)
                {
                    // Here we implement exponential backoff attempts to retry the call few more times before giving up
                    // In the case of errors, as recommended here https://bit.ly/2CWYrjQ
                    if (attemptsSoFar < maxAttempts)
                    {
                        var randomOffset = _rand.Next(0, deltaBackoff);
                        await Task.Delay(backoff + randomOffset, cancellation);

                        // Double the backoff for next attempt
                        backoff = Math.Min(backoff * 2, maxBackoff);
                    }
                    else
                    {
                        // Log and throw exception
                        string body = await response.Body.ReadAsStringAsync();
                        _logger.LogError($"SendGrid: {response.StatusCode} response after {attemptsSoFar} attempts with exponential backoff: {body}");
                        
                        throw new EmailApiException($"Failed to send email(s) with status code {response.StatusCode}"); // Give up
                    }
                }
            }
            //try
            //{
            //    Response response = await client.SendEmailAsync(msg, cancellation);

            //    if (response.StatusCode == HttpStatusCode.TooManyRequests || response.StatusCode >= HttpStatusCode.InternalServerError)
            //    {

            //    }
                
            //    if (cancellation.IsCancellationRequested)
            //    {
            //        // Doesn't matter since the request was cancelled
            //        return "";
            //    }
            //    else
            //    {
            //        return response.Headers.GetValues("X-Message-Id").FirstOrDefault();
            //    }
            //} 
        }
    }
}
