using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Tellma.Utilities.Email;
using Tellma.Utilities.Common;

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

            // Create the email body
            var subjectText = $"{_provider.InstallationIdentifier ?? "Tellma ERP"}: Unhandled {exception.GetType().Name}: {exception.Message.Truncate(50, appendEllipses: true)}";
            var bodyText = $@"
{formatter(state, exception)}

--- Stack Trace ---

{exception}";

            // Create the email
            var email = new EmailToSend(_provider.EmailAddress)
            {
                Subject = subjectText,
                Body = Htmlify(bodyText),
            };

            // Fire and forget the email (no need to await email logging)
            var _ = TrySendEmail(email);
        }

        private static string Htmlify(string bodyText)
        {
            var bldr = new StringBuilder();
            bldr.Append(@"<span style=""font-family: 'Courier New', monospace; "">");
            foreach (var line in bodyText.Split(Environment.NewLine))
            {
                bldr.Append($"{HtmlEncoder.Default.Encode(line)}<br/>");
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
