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

        public string GetUserEmail() => _accessor?.HttpContext?.User?.FindFirstValue(JwtClaimTypes.Email);

        public string GetUserId() => _accessor?.HttpContext?.User?.FindFirstValue(JwtClaimTypes.Subject);
    }
}