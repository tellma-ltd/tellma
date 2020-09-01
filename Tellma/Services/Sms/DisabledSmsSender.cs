using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Sms
{
    /// <summary>
    /// This one just throws an exception if sending an SMS is attempted
    /// </summary>
    public class DisabledSmsSender : ISmsSender
    {
        public Task SendAsync(string toPhoneNumber, string sms, int? tenantId = null, int? notificationId = null, CancellationToken cancellation = default)
        {
            // This indicates a bug, all SMS sending code should check if SMS is enabled first before sending any
            throw new InvalidOperationException("SMS is disabled in this installation, to enable it set SmsEnabled to true in a configuration provider.");
        }
    }
}
