using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Tellma.Utilities.Email;
using Tellma.Utilities.Common;
using System.Linq;

namespace Tellma.Utilities.Logging
{
    public class EmailLogger : ILogger
    {
        private readonly EmailLoggerProvider _provider;

        public EmailLogger(EmailLoggerProvider provider)
        {
            _provider = provider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (logLevel == LogLevel.Error || logLevel == LogLevel.Critical) && // Should be an error
                !string.IsNullOrWhiteSpace(_provider.EmailAddress) && // Email address should be in settings
                _provider.EmailSender().IsEnabled; // Email should be enabled in this installation
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (exception == null)
            {
                // This isn't actionable
                return;
            }

            // Create the email body
            var exceptionType = exception?.GetType()?.Name ?? "Error";
            var truncatedMsg = exception?.Message?.Truncate(50, appendEllipses: true);
            var subjectText = $"{_provider.InstallationIdentifier ?? "Tellma ERP"}: Unhandled {exceptionType}: {truncatedMsg}";
            var bodyText = $@"
{formatter(state, exception)}

--- Stack Trace ---

{exception}";

            // Create the email
            var email = new EmailToSend(_provider.EmailAddress)
            {
                Subject = subjectText,
                Body = Htmlify(bodyText, "'Courrier New', monospace"),
            };

            // Fire and forget the email (no need to await email logging)
            var _ = TrySendEmail(email);
        }

        public static string Htmlify(string bodyText, string font)
        {
            var bldr = new StringBuilder();
            bldr.Append(@$"<span style=""font-family: {font}; "">");
            foreach (var line in bodyText.Split(Environment.NewLine).Select(e => e ?? ""))
            {
                int i = 0;
                while (i < line.Length && char.IsWhiteSpace(line[i]))
                {
                    bldr.Append("&nbsp;");
                    i++;
                }
                
                bldr.Append($"{HtmlEncoder.Default.Encode(line.Trim())}<br/>");
            }
            bldr.Append(@"</span>");

            return bldr.ToString(); ;
        }

        private async Task TrySendEmail(EmailToSend email)
        {
            try
            {
                await _provider.EmailSender().SendAsync(email);
            }
            // If we attempt to email an exception and the email sender itself throws
            // an exception then we just throw our hands up and forget about it.
            catch { }
        }
    }
}
