using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Email;

namespace Tellma.Utilities.Logging
{
    public class EmailLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _provider;
        private readonly EmailLoggerOptions _opt;

        public EmailLoggerProvider(IOptions<EmailLoggerOptions> options, IServiceProvider provider)
        {
            _provider = provider;
            _opt = options.Value;
        }

        /// <summary>
        /// The <see cref="IEmailSender"/> to send the logged exceptions through.
        /// </summary>
        /// <remarks>
        /// We lazy-load the <see cref="IEmailSender"/> otherwise dependency injection 
        /// complains of circular dependency: Logger -> Email -> Logger
        /// </remarks>
        public IEmailSender EmailSender() => _provider.GetService<IEmailSender>() ?? new DisabledEmailSender();

        /// <summary>
        /// The email address to send logged exceptions to.
        /// </summary>
        public string EmailAddress => _opt.EmailAddress;

        /// <summary>
        /// An identifier of the system installation, in case the same email address
        /// will receive logged exceptions from multiple system installations.
        /// </summary>
        public string InstallationIdentifier => _opt.InstallationIdentifier;

        public ILogger CreateLogger(string categoryName)
        {
            return new EmailLogger(this);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// In case there was no <see cref="IEmailSender"/> implementation registered.
        /// </summary>
        private class DisabledEmailSender : IEmailSender
        {
            public bool IsEnabled => false;

            public Task SendAsync(EmailToSend email, string fromEmail = null, CancellationToken cancellation = default)
                => Task.CompletedTask;

            public Task SendBulkAsync(IEnumerable<EmailToSend> emails, string fromEmail = null, CancellationToken cancellation = default)
                => Task.CompletedTask;

        }
    }
}
