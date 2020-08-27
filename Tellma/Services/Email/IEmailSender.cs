using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Services.Email
{
    public interface IEmailSender
    {
        Task SendBulkAsync(List<string> tos, List<string> subjects, string htmlMessage, List<Dictionary<string, string>> substitutions, string fromEmail = null);
        
        Task SendAsync(string to, string subject, string htmlMessage, string sourceEmail = null);
    }
}
