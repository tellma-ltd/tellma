using System.Collections.Generic;
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

        public async Task SendBulkAsync(List<string> tos, List<string> subjects, string htmlMessage, List<Dictionary<string, string>> substitutions, string fromEmail = null)
        {
            await _emailSender.SendBulkAsync(tos, subjects, htmlMessage, substitutions, fromEmail);
        }

        public async Task SendAsync(string to, string subject, string htmlMessage, string sourceEmail = null)
        {
            await _emailSender.SendAsync(to, subject, htmlMessage);
        }
    }
}
