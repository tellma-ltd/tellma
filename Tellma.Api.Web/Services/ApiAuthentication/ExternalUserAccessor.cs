using IdentityModel;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Tellma.Services.ApiAuthentication
{
    public class ExternalUserAccessor : IExternalUserAccessor
    {
        private readonly IHttpContextAccessor _accessor;

        public ExternalUserAccessor(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public bool IsService => UserId == null;

        public string GetUserEmail() => Email;

        public string GetUserId() => UserId ?? ClientId;

        private ClaimsPrincipal User => _accessor?.HttpContext?.User;

        private string Email => User?.FindFirstValue(JwtClaimTypes.Email);

        private string UserId => User?.FindFirstValue(JwtClaimTypes.Subject);

        private string ClientId => User?.FindFirstValue(JwtClaimTypes.ClientId);
    }
}