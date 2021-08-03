using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Email
{
    public interface IEmailSender
    {
        /// <summary>
        /// True if the current installation supports sending emails, false otherwise.
        /// </summary>
        public bool IsEnabled => true;

        /// <summary>
        /// Sends a batch of emails in bulk.
        /// </summary>
        Task SendBulkAsync(IEnumerable<EmailToSend> emails, string fromEmail = null, CancellationToken cancellation = default);

        /// <summary>
        /// Sends a single email.
        /// </summary>
        Task SendAsync(EmailToSend email, string fromEmail = null, CancellationToken cancellation = default);
    }
}
