using System.Collections.Generic;
using Tellma.Controllers.Dto;

namespace Tellma.Controllers.Jobs
{
    public class InboxNotificationsQueue : BackgroundQueue<IEnumerable<InboxNotification>>
    {
    }
}
