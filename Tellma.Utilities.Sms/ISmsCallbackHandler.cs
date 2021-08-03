using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Sms
{
    public interface ISmsCallbackHandler
    {
        /// <summary>
        /// Handles the notification from the external SMS service
        /// </summary>
        Task HandleCallback(SmsEventNotification smsEvent, CancellationToken cancellation);
    }
}
