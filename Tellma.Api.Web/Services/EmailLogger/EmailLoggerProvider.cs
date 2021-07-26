using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tellma.Services.Utilities;
using Tellma.Utilities.Email;

namespace Tellma.Services.EmailLogger
{
    public class EmailLoggerProvider : ILoggerProvider
    {
        public IEmailSender EmailSender { get; }
        public string Email { get; }
        public string InstanceIdentifier { get; }

        public EmailLoggerProvider(IOptions<EmailLoggerOptions> options, IEmailSender _emailSender)
        {
            EmailSender = _emailSender;
            Email = options.Value.Email;
            InstanceIdentifier = options.Value.InstanceIdentifier;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new EmailLogger(this);
        }

        public void Dispose()
        {
        }
    }
}
