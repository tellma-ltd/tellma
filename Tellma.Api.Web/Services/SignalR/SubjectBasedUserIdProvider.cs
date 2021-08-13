using IdentityModel;
using Microsoft.AspNetCore.SignalR;

namespace Tellma.Services.SignalR
{
    public class SubjectBasedUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(JwtClaimTypes.Subject)?.Value;
        }
    }
}
