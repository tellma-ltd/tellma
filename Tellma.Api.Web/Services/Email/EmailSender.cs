using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IEmailSender _emailSender;

        public EmailSender(IEmailSenderFactory factory)
        {
            _emailSender = factory.Create();
        }

        public async Task SendBulkAsync(IEnumerable<Email> emails, string fromEmail = null, CancellationToken cancellation = default)
        {
            await _emailSender.SendBulkAsync(emails, fromEmail, cancellation);
        }

        public async Task SendAsync(Email email, string fromEmail = null, CancellationToken cancellation = default)
        {
            await _emailSender.SendAsync(email, fromEmail, cancellation);
        }
    }
}
