using System.Collections.Generic;
using System.Linq;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class TemplatesOutput
    {
        public TemplatesOutput(
            string schedulesVersion, 
            string settingsVersion,
            string supportEmails,
            IEnumerable<EmailTemplate> emailTemplates, 
            IEnumerable<MessageTemplate> messageTemplates)
        {
            SchedulesVersion = schedulesVersion;
            SettingsVersion = settingsVersion;
            SupportEmails = supportEmails;
            EmailTemplates = emailTemplates?.ToList();
            MessageTemplates = messageTemplates?.ToList();
        }

        public string SchedulesVersion { get; }
        public string SettingsVersion { get; }
        public string SupportEmails { get; }
        public IEnumerable<EmailTemplate> EmailTemplates { get; }
        public IEnumerable<MessageTemplate> MessageTemplates { get; }
    }
}
