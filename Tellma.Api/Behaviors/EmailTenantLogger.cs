using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Api.Notifications;
using Tellma.Utilities.Email;

namespace Tellma.Api.Behaviors
{
    public class EmailTenantLogger : ITenantLogger
    {
        private readonly IClientProxy _clientProxy;
        private readonly NotificationsQueue _notificationsQueue;

        public EmailTenantLogger(IClientProxy clientProxy, NotificationsQueue notificationsQueue)
        {
            _clientProxy = clientProxy;
            _notificationsQueue = notificationsQueue;
        }

        public void Log(TenantLogEntry entry)
        {
            if (entry.TenantSupportEmails != null && entry.TenantSupportEmails.Any())
            {
                EmailToSend email = _clientProxy.MakeTenantNotificationEmail(entry);
                Task _ = _notificationsQueue.Enqueue(entry.TenantId, new List<EmailToSend> { email });
            }
        }
    }
}
