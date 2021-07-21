using System.Collections.Generic;
using Tellma.Controllers.Dto;
using Tellma.Notifications;

namespace Tellma.Services.ClientApp
{
    public class InboxNotificationsQueue : BackgroundQueue<IEnumerable<InboxStatusToSend>>
    {
    }
}
