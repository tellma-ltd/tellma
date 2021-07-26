using Microsoft.AspNetCore.SignalR;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Services.ClientProxy
{
    [AuthorizeJwtBearer]
    public class ServerNotificationsHub : Hub<INotifiedClient>
    {
    }
}
