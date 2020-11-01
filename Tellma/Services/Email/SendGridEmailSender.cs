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
using Tellma.Services.Utilities;
using HtmlAgilityPack;

namespace Tellma.Services.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        public const string EmailIdKey = "email_id";
        public const string TenantIdKey = "tenant_id";

        private const string Placeholder = "-ph-";

        private readonly SendGridOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SendGridEmailSender> _logger;
        private readonly Random _rand = new Random();

        public SendGridEmailSender(
            IOptions<SendGridOptions> options,
            IHttpClientFactory httpClientFactory,
            ILogger<SendGridEmailSender> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger;
        }

        public async Task SendBulkAsync(IEnumerable<Email> emails, string fromEmail = null, CancellationToken cancellation = default)
        {
            // Prepare the SendGridMessage
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(email: fromEmail ?? _options.DefaultFromEmail, name: _options.DefaultFromName));
            msg.AddContent(MimeType.Html, Placeholder);
            foreach (var (email, index) in emails.Select((e, i) => (e, i)))
            {
                msg.AddTo(new EmailAddress(email.ToEmail), index);
                msg.SetSubject(email.Subject, index);
                msg.AddSubstitutions(new Dictionary<string, string> { { Placeholder, email.Body } }, index);
                msg.AddCustomArg(EmailIdKey, email.EmailId.ToString(), index);
                msg.AddCustomArg(TenantIdKey, email.TenantId.ToString(), index);
            }

            // Send it to SendGrid using their official C# library
            await SendEmailAsync(msg, cancellation);
        }

        public async Task SendAsync(Email email, string fromEmail = null, CancellationToken cancellation = default)
        {
            // Prepare the SendGridMessage
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(email: fromEmail ?? _options.DefaultFromEmail, name: _options.DefaultFromName));
            msg.AddTo(new EmailAddress(email.ToEmail));
            msg.SetSubject(email.Subject);
            msg.AddContent(MimeType.Html, email.Body);
            msg.AddCustomArg(EmailIdKey, email.EmailId.ToString());
            msg.AddCustomArg(TenantIdKey, email.TenantId.ToString());

            // Send it to SendGrid using their official C# library
            await SendEmailAsync(msg, cancellation);
        }

        /// <summary>
        /// Helper method that dispatches a <see cref="SendGridMessage"/> using the official SendGrid C# library
        /// And applies recommended exponential backoff if certain error responses are returned
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
                    string body = (await response.Body.ReadAsStringAsync()).Truncate(500, appendEllipses: true);
                    string logMessage = $"SendGrid: {response.StatusCode} response after {attemptsSoFar} attempts with exponential backoff: {body}";

                    // Here we implement exponential backoff attempts to retry the call few more times before giving up
                    // In the case of errors, as recommended here https://bit.ly/2CWYrjQ
                    if (attemptsSoFar < maxAttempts)
                    {
                        var randomOffset = _rand.Next(0, deltaBackoff);
                        await Task.Delay(backoff + randomOffset, cancellation);

                        // Double the backoff for next attempt
                        backoff = Math.Min(backoff * 2, maxBackoff);

                        // Log warning
                        _logger.LogWarning(logMessage);
                    }
                    else
                    {
                        // Log and throw exception
                        _logger.LogError(logMessage);

                        throw new EmailApiException($"Failed to send email(s) with status code {response.StatusCode}"); // Give up
                    }
                }
                else if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    // Do not implement exponential backoff for 4xx errors (except 429 Too Many Requests)
                    string body = (await response.Body.ReadAsStringAsync()).Truncate(500, appendEllipses: true);
                    throw new EmailApiException($"Failed to send email(s) with status code {response.StatusCode}, body: {body}"); // Give up
                }
                else
                {
                    break; // Success, no need to loop again
                }
            }
        }

        /// <summary>
        /// Convert the HTML content to plain text
        /// </summary>
        /// <param name="html">The html content which is going to be converted</param>
        /// <returns>The plain text represenation</returns>
        public static string HtmlToPlainText(string html)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                return document.DocumentNode == null ? string.Empty : document.DocumentNode.InnerText;
            }
            catch
            {
                return ""; // Not worth it
            }
        }
    }
}
