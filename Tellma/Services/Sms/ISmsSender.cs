using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(SmsMessage sms, CancellationToken cancellation = default);
    }
}
