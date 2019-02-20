using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private IEmailSender _emailSender;

        public EmailSender(IEmailSenderFactory factory)
        {
            _emailSender = factory.Create();
        }

        public async Task SendEmailBulkAsync(List<string> tos, List<string> subjects, string htmlMessage, List<Dictionary<string, string>> substitutions, string fromEmail = null)
        {
            await _emailSender.SendEmailBulkAsync(tos, subjects, htmlMessage, substitutions, fromEmail);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage, string sourceEmail = null)
        {
            await _emailSender.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
