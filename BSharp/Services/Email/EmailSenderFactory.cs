using System;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Options;

namespace BSharp.Services.Email
{
    public class EmailSenderFactory : IEmailSenderFactory
    {
        private readonly EmailConfiguration _config;
        private readonly GlobalConfiguration _globalConfig;

        public EmailSenderFactory(IOptions<EmailConfiguration> options, IOptions<GlobalConfiguration> globalOptions)
        {
            _config = options.Value;
            _globalConfig = globalOptions.Value;
        }

        public IEmailSender Create()
        {
            if(_globalConfig.IsOnline)
            {
                // Scream for missing yet required stuff
                if (string.IsNullOrWhiteSpace(_config.SendGrid.ApiKey))
                {
                    throw new InvalidOperationException(
                        $"A SendGrid API Key must be in a configuration provider under the key 'Email:SendGrid:ApiKey', you can get a free key on https://sendgrid.com/");
                }

                return new SendGridEmailSender(_config.SendGrid);
            }
            else
            {
                return new OfflineEmailSender();
            }
        }
    }
}
