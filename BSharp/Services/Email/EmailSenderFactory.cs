using System;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BSharp.Services.Email
{
    public class EmailSenderFactory : IEmailSenderFactory
    {
        private readonly EmailConfiguration _config;
        private readonly GlobalConfiguration _globalConfig;
        private readonly ILogger<SendGridEmailSender> _logger;

        public EmailSenderFactory(IOptions<EmailConfiguration> options, IOptions<GlobalConfiguration> globalOptions, ILogger<SendGridEmailSender> logger)
        {
            _config = options.Value;
            _globalConfig = globalOptions.Value;
            _logger = logger;
        }

        public IEmailSender Create()
        {
            if(_globalConfig.Online)
            {
                // Scream for missing yet required stuff
                if (string.IsNullOrWhiteSpace(_config.SendGrid.ApiKey))
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
