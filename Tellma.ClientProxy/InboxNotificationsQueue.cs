using System.Collections.Generic;
using Tellma.Notifications;

namespace Tellma.ClientProxy
{
    public class InboxNotificationsQueue : BackgroundQueue<IEnumerable<InboxStatusToSend>>
    {
    }
}
