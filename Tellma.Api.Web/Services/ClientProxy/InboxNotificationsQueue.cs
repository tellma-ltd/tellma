using System.Collections.Generic;
using Tellma.Api.Notifications;
using Tellma.Controllers.Dto;

namespace Tellma.Services.ClientProxy
{
    public class InboxNotificationsQueue : BackgroundQueue<IEnumerable<InboxStatusToSend>>
    {
    }
}
