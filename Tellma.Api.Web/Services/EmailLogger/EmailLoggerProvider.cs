using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Tellma.Utilities.Email;

namespace Tellma.Services.EmailLogger
{
    public class EmailLoggerProvider : ILoggerProvider
    {
        public EmailLoggerProvider(IOptions<EmailLoggerOptions> options, IEmailSender _emailSender)
        {
            EmailSender = _emailSender;
            Email = options.Value.EmailAddress;
            InstallationIdentifier = options.Value.InstallationIdentifier;
        }

        /// <summary>
        /// The <see cref="IEmailSender"/> to send the logged exceptions through.
        /// </summary>
        public IEmailSender EmailSender { get; }

        /// <summary>
        /// The email address to send logged exceptions to.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// An identifier of the system installation, in case the same email address
        /// will receive logged exceptions from multiple system installations.
        /// </summary>
        public string InstallationIdentifier { get; }

        public ILogger CreateLogger(string categoryName)
        {
            return new EmailLogger(this);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
