using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;

namespace BSharp.Services.Identity
{
    public class UserProvider : IUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserEmail()
        {
            return _httpContextAccessor?.HttpContext?.User?.Email();
        }

        public string GetUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.UserId();
        }
    }
}
