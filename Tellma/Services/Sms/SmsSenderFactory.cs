using Microsoft.Extensions.Options;
using Tellma.Services.Utilities;

namespace Tellma.Services.Sms
{
    public class SmsSenderFactory : ISmsSenderFactory
    {
        private readonly GlobalOptions _globalOptions;
        private readonly TwilioSmsSender _twilioSender;

        public SmsSenderFactory(IOptions<GlobalOptions> globalOptions, TwilioSmsSender twilioSender)
        {
            _globalOptions = globalOptions.Value;
            _twilioSender = twilioSender;
        }

        public ISmsSender Create()
        {
            if (_globalOptions.SmsEnabled)
            {
                return _twilioSender;
            } 
            else
            {
                return new DisabledSmsSender();
            }            
        }
    }
}
