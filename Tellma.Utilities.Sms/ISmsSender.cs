using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Sms
{
    public interface ISmsSender
    {
        public bool IsEnabled => true;

        Task SendAsync(SmsToSend sms, CancellationToken cancellation = default);
    }
}
