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

        public async Task<string> SendAsync(string toPhoneNumber, string sms, CancellationToken cancellation)
        {
            return await _smsSender.SendAsync(toPhoneNumber, sms, cancellation);
        }
    }
}
