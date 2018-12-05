using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Identity
{
    public class UserIdProvider : IUserIdProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.UserId();
        }
    }
}
