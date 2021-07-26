using System.Collections.Generic;
using Tellma.Controllers.Dto;
using Tellma.Notifications;

namespace Tellma.Services.ClientProxy
{
    public class InboxNotificationsQueue : BackgroundQueue<IEnumerable<InboxStatusToSend>>
    {
    }
}
