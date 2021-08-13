using System.Collections.Generic;
using Tellma.Api.Notifications;
using Tellma.Api.Dto;

namespace Tellma.Services.ClientProxy
{
    public class InboxNotificationsQueue : BackgroundQueue<IEnumerable<InboxStatusToSend>>
    {
    }
}
