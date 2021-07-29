using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Email;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// An implementation of <see cref="IEmailSender"/> that throws exceptions.<br/>
    /// This is the default implementation when email is not enabled.
    /// </summary>
    public class NullEmailSender : IEmailSender
    {
        public bool IsEnabled => false;

        public Task SendBulkAsync(IEnumerable<EmailToSend> emails, string fromEmail = null, CancellationToken cancellation = default)
        {
            throw new InvalidOperationException(Message);
        }

        public Task SendAsync(EmailToSend email, string fromEmail = null, CancellationToken cancellation = default)
        {
            throw new InvalidOperationException(Message);
        }

        private static string Message => $"Email is disabled in this installation, to enable it set EmailEnabled to true in a configuration provider.";
    }
}
