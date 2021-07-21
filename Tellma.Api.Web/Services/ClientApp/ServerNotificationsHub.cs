using Microsoft.AspNetCore.SignalR;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Services.ClientApp
{
    [AuthorizeJwtBearer]
    public class ServerNotificationsHub : Hub<INotifiedClient>
    {
    }
}
