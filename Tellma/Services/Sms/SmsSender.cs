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

        public async Task SendAsync(string toPhoneNumber, string sms, int? tenantId, int? notificationId, CancellationToken cancellation)
        {
            await _smsSender.SendAsync(toPhoneNumber, sms, tenantId, notificationId, cancellation);
        }
    }
}
