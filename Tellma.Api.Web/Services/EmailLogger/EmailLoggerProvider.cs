using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Services.Email;
using Tellma.Services.Utilities;

namespace Tellma.Services.EmailLogger
{
    public class EmailLoggerProvider : ILoggerProvider
    {
        public IEmailSender EmailSender { get; }
        public string Email { get; }
        public string InstanceIdentifier { get; }
        public bool EmailEnabled { get; }

        public EmailLoggerProvider(IOptions<EmailLoggerOptions> options, IOptions<GlobalOptions> globalOptions, IEmailSender _emailSender)
        {
            EmailSender = _emailSender;
            Email = options.Value.Email;
            InstanceIdentifier = options.Value.InstanceIdentifier;
            EmailEnabled = globalOptions.Value.EmailEnabled;
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
