using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Utilities.Email
{
    public interface IEmailQueuer
    {
        /// <summary>
        /// Queues generic application emails to be send asynchronously.
        /// </summary>
        /// <param name="tenantId">The tenant Id sending the email.</param>
        /// <param name="emails">The emails to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task EnqueueEmails(int tenantId, List<EmailToSend> emails);
    }
}
