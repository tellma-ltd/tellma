using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Email
{
    public interface IEmailCallbackHandler
    {
        /// <summary>
        /// Handles a batch of notifications from an external Email service (e.g. SendGrid)
        /// </summary>
        Task HandleCallback(IEnumerable<EmailEventNotification> emailEvents, CancellationToken cancellation);
    }
}
