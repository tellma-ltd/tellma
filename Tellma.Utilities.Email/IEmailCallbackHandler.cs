using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Email
{
    public interface IEmailCallbackHandler
    {
        /// <summary>
        /// Handles a batch of email events from an external email service (e.g. SendGrid)
        /// </summary>
        Task HandleCallback(IEnumerable<EmailEventNotification> emailEvents, CancellationToken cancellation);
    }
}
