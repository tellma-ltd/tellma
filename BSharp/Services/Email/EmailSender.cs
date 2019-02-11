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

        public async Task SendEmailAsync(string email, string subject, string htmlMessage, string sourceEmail = null)
        {
            await _emailSender.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
