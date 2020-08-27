using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Services.Email
{
    /// <summary>
    /// This one just throws an exception if sending an SMS is attempted
    /// </summary>
    public class DisabledEmailSender : IEmailSender
    {
        public Task SendBulkAsync(List<string> tos, List<string> subjects, string htmlMessage, List<Dictionary<string, string>> substitutions, string fromEmail = null)
        {
            return Throw();
        }

        public Task SendAsync(string email, string subject, string htmlMessage, string fromEmail = null)
        {
            return Throw();
        }

        private Task Throw()
        {
            // This indicates a bug, all email sending code should check if email is enabled first before sending any
            throw new InvalidOperationException($"Email is disabled in this installation, to enable it set {nameof(GlobalOptions.EmailEnabled)} to true in a configuration provider.");
        }
    }
}
