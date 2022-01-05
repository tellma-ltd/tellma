using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Common;
using Tellma.Utilities.Email;

namespace Tellma.Utilities.SendGrid
{
    /// <summary>
    /// An implementation of <see cref="IEmailSender"/> that sends emails through the SendGrid service.
    /// </summary>
    public class SendGridEmailSender : IEmailSender
    {
        public const string EmailIdKey = "email_id";
        public const string TenantIdKey = "tenant_id";

        private readonly SendGridOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SendGridEmailSender> _logger;
        private readonly Random _rand = new();

        public SendGridEmailSender(
            IOptions<SendGridOptions> options,
            IHttpClientFactory httpClientFactory,
            ILogger<SendGridEmailSender> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger;
        }

        public async Task SendBulkAsync(IEnumerable<EmailToSend> emails, string fromEmail = null, CancellationToken cancellation = default)
        {
            // Send the emails in chunks in order not to choke the network
            int skip = 0;
            int chunkSize = _options.BatchSize;
            while (true)
            {
                var chunk = emails.Skip(skip).Take(chunkSize);
                if (chunk.Any())
                {
                    await Task.WhenAll(chunk.Select(email => SendAsync(email, fromEmail, cancellation)));
                    skip += chunkSize;
                }
                else
                {
                    break;
                }
            }            
        }

        public async Task SendAsync(EmailToSend email, string fromEmail = null, CancellationToken cancellation = default)
        {            // Prepare the SendGridMessage
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(email: fromEmail ?? _options.DefaultFromEmail, name: _options.DefaultFromName));

            // Recipients
            foreach (var to in (email.To ?? new List<string>()).Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                msg.AddTo(new EmailAddress(to));
            }

            foreach (var cc in (email.Cc ?? new List<string>()).Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                msg.AddCc(new EmailAddress(cc));
            }

            foreach (var bcc in (email.Bcc ?? new List<string>()).Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                msg.AddBcc(new EmailAddress(bcc));
            }

            // Subject and body
            if (!string.IsNullOrWhiteSpace(email.Subject))
            {
                msg.SetSubject(email.Subject);
            }

            if (string.IsNullOrWhiteSpace(email.Body))
            {
                msg.AddContent(MimeType.Html, "<p></p>");
            }
            else if (!string.IsNullOrWhiteSpace(email.Body))
            {
                msg.AddContent(MimeType.Html, email.Body);
            }

            // Custom Args
            msg.AddCustomArg(EmailIdKey, email.EmailId.ToString());
            msg.AddCustomArg(TenantIdKey, email.TenantId.ToString());

            // Attachments
            foreach (var att in email.Attachments)
            {
                if (att.Contents != null && att.Contents.LongLength > 0L)
                {
                    var name = att.Name;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = "Attachment"; // The caller will typically avoid this
                    }

                    var base64 = Convert.ToBase64String(att.Contents);
                    msg.AddAttachment(name, base64);
                }
            }

            // Send it to SendGrid using their official C# library
            await SendEmailAsync(msg, cancellation);
        }

        /// <summary>
        /// Helper method that dispatches a <see cref="SendGridMessage"/> using the official SendGrid C# library
        /// And applies recommended exponential backoff if certain error responses are returned.
        /// </summary>
        private async Task SendEmailAsync(SendGridMessage msg, CancellationToken cancellation)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var client = new SendGridClient(httpClient, _options.ApiKey); // Reuse the HttpClient to avoid a memory leak

            // Exponential backoff (There is a built-in implementation in SG library but it doesn't handle 429)
            const int maxAttempts = 5;
            const int maxBackoff = 25000; // 25 Seconds
            const int minBackoff = 1000; // 1 Second
            const int deltaBackoff = 1000; // 1 Second

            int attemptsSoFar = 0;
            int backoff = minBackoff;

            while (attemptsSoFar < maxAttempts && !cancellation.IsCancellationRequested)
            {
                attemptsSoFar++;
                Response response = await client.SendEmailAsync(msg, cancellation);

                if (response.StatusCode == HttpStatusCode.TooManyRequests || response.StatusCode >= HttpStatusCode.InternalServerError)
                {
                    string body = (await response.Body.ReadAsStringAsync(cancellation)).Truncate(500, appendEllipses: true);

                    // Here we implement exponential backoff attempts to retry the call few more times before giving up
                    // In the case of errors, as recommended here https://bit.ly/2CWYrjQ
                    if (attemptsSoFar < maxAttempts)
                    {
                        // Log warning
                        string logMessage = $"SendGrid: {response.StatusCode} response after {attemptsSoFar} attempts with exponential backoff. Response Body: {body}";
                        _logger.LogWarning(logMessage);

                        var randomOffset = _rand.Next(0, deltaBackoff);
                        await Task.Delay(backoff + randomOffset, cancellation);

                        // Double the backoff for next attempt
                        backoff = Math.Min(backoff * 2, maxBackoff);
                    }
                    else
                    {
                        // Reached maxAttempts => Give up
                        throw new EmailApiException($"Failed to send email(s) with status code {response.StatusCode}. Response Body: {body}"); // Give up
                    }
                }
                else if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    // Do not implement exponential backoff for 4xx errors (except 429 Too Many Requests), those errors are not transient.
                    string body = (await response.Body.ReadAsStringAsync(cancellation)).Truncate(500, appendEllipses: true);
                    throw new EmailApiException($"Failed to send email(s) with status code {response.StatusCode}. Response Body: {body}"); // Give up
                }
                else
                {
                    break; // Success, no need to loop again
                }
            }
        }
    }
}
