using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Sms
{
    public class SmsSender : ISmsSender
    {
        private readonly ISmsSender _smsSender;

        public SmsSender(ISmsSenderFactory factory)
        {
            _smsSender = factory.Create();
        }

        public async Task SendAsync(SmsForSender sms, CancellationToken cancellation = default)
        {
            await _smsSender.SendAsync(sms, cancellation);
        }
    }
}
