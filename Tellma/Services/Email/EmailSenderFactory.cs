using System;
using Tellma.Services.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tellma.Services.Email
{
    public class EmailSenderFactory : IEmailSenderFactory
    {
        private readonly EmailOptions _config;
        private readonly GlobalOptions _globalConfig;
        private readonly ILogger<SendGridEmailSender> _logger;

        public EmailSenderFactory(IOptions<EmailOptions> options, IOptions<GlobalOptions> globalOptions, ILogger<SendGridEmailSender> logger)
        {
            _config = options.Value;
            _globalConfig = globalOptions.Value;
            _logger = logger;
        }

        public IEmailSender Create()
        {
            if(_globalConfig.EmailEnabled)
            {
                // Scream for missing yet required stuff
                if (string.IsNullOrWhiteSpace(_config?.SendGrid?.ApiKey))
                {
                    throw new InvalidOperationException(
                        $"A SendGrid API Key must be in a configuration provider under the key 'Email:SendGrid:ApiKey', you can get a free key on https://sendgrid.com/");
                }

                return new SendGridEmailSender(_config.SendGrid, _logger);
            }
            else
            {
                return new OfflineEmailSender();
            }
        }
    }
}
