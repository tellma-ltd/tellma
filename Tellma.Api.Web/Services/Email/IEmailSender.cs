using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Email
{
    public interface IEmailSender
    {
        /// <summary>
        /// Sends a batch of emails in bulk
        /// </summary>
        Task SendBulkAsync(IEnumerable<Email> emails, string fromEmail = null, CancellationToken cancellation = default);

        /// <summary>
        /// Sends a single email
        /// </summary>
        Task SendAsync(Email email, string fromEmail = null, CancellationToken cancellation = default);
    }
}
