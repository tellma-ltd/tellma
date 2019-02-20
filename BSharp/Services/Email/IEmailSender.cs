using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Email
{
    public interface IEmailSender
    {
        Task SendEmailBulkAsync(List<string> tos, List<string> subjects, string htmlMessage, List<Dictionary<string, string>> substitutions, string fromEmail = null);
        Task SendEmailAsync(string email, string subject, string htmlMessage, string sourceEmail = null);
    }
}
