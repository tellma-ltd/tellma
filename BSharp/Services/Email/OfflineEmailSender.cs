using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Email
{
    /// <summary>
    /// All this implementation does is shout and scream whenever we attempt to send an email
    /// in offline mode, to make such bugs go down with a loud bang
    /// </summary>
    public class OfflineEmailSender : IEmailSender
    {
        public Task SendEmailBulkAsync(List<string> tos, List<string> subjects, string htmlMessage, List<Dictionary<string, string>> substitutions, string fromEmail = null)
        {
            Debug.WriteLine($"Tos: {string.Join(",", tos)}");
            Debug.WriteLine($"Subject: {string.Join(",", subjects)}");
            Debug.WriteLine($"HTML Message: {htmlMessage}");
            Debug.WriteLine($"Substitutions: {string.Join(",", substitutions)}");

            return Task.CompletedTask;

            // Programmer mistake
            // throw new Exception("The system installation is configured as offline, to configure it as online set IsOnline value to 'true' in a configuration provider");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage, string fromEmail = null)
        {
            Debug.WriteLine($"Email: {email}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"HTML Message: {htmlMessage}");

            return Task.CompletedTask;

            // Programmer mistake
            // throw new Exception("The system installation is configured as offline, to configure it as online set IsOnline value to 'true' in a configuration provider");
        }
    }
}
