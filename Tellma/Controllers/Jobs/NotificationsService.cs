using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Data;

namespace Tellma.Controllers.Jobs
{
    public class NotificationsService
    {
        public async Task Enqueue(List<EmailQueueItem> emails = null, List<SmsQueueItem> smses = null, List<PushNotificationQueueItem> pushes = null)
        {
            // TODO
        }
    }
}
