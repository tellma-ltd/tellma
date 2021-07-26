using Microsoft.Extensions.Logging;
using System;
using Tellma.Utilities.Email;

namespace Tellma.Services.EmailLogger
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
            return logLevel == LogLevel.Error && _provider.EmailSender.IsEnabled && !string.IsNullOrWhiteSpace(_provider.Email);
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
                Subject = $"Unhandled Error on: {_provider.InstanceIdentifier ?? "Tellma"} - Id: {eventId.Id}",
                Body = formatter(state, exception),
            };

            // Fire and forget (no need to await email logging)
            _provider.EmailSender.SendAsync(email);
        }
    }
}
