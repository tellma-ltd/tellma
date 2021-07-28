using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tellma.Utilities.Email;

namespace Tellma.Services.EmailLogger
{
    public class EmailLogger : ILogger
    {
        private readonly EmailLoggerProvider _provider;
        private readonly bool _canSend;

        public EmailLogger(EmailLoggerProvider provider)
        {
            _provider = provider;
            _canSend = _provider.EmailSender.IsEnabled && !string.IsNullOrWhiteSpace(_provider.Email);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == LogLevel.Error && _canSend;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Prepare the email
            var email = new EmailToSend(_provider.Email)
            {
                Subject = $"Unhandled Exception on: {_provider.InstallationIdentifier ?? "Tellma ERP"} - Id: {eventId.Id}",
                Body = formatter(state, exception),
            };

            // Fire and forget (no need to await email logging)
            var _ = TrySendEmail(email);
        }

        private async Task TrySendEmail(EmailToSend email)
        {
            try
            {
                await _provider.EmailSender.SendAsync(email);
            }
            // If we attempt to email an exception and the email sender itself throws
            // an exception then we just throw our hands up and forget about it.
            catch { }
        }
    }
}
