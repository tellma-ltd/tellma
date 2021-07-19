using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Sms;

namespace Tellma.Notifications
{
    /// <summary>
    /// An implementation of <see cref="ISmsSender"/> that throws exceptions.<br/>
    /// This is the default implementation when SMS is not enabled.
    /// </summary>
    public class NullSmsSender : ISmsSender
    {
        public bool IsEnabled => false;

        public Task SendAsync(SmsToSend sms, CancellationToken cancellation = default)
        {
            // This indicates a bug, all SMS sending code should check if SMS is enabled first before sending any
            throw new InvalidOperationException("SMS is disabled in this installation, to enable it set SmsEnabled to true in a configuration provider.");
        }
    }
}
