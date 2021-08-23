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
        
        public string Email => User?.FindFirstValue(JwtClaimTypes.Email);

        public string UserId => User?.FindFirstValue(JwtClaimTypes.Subject);

        public string ClientId => User?.FindFirstValue(JwtClaimTypes.ClientId);

        public bool IsServiceAccount => UserId == null;

        private ClaimsPrincipal User => _accessor?.HttpContext?.User;
    }
}