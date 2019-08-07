using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;

namespace BSharp.Services.Identity
{
    public class ExternalUserAccessor : IExternalUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExternalUserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserEmail()
        {
            return _httpContextAccessor?.HttpContext?.User?.Email();
        }

        public string GetUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.ExternalUserId();
        }
    }
}
