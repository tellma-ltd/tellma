using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Identity
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly string DB_USER_KEY = "DbUser";

        public UserService(IHttpContextAccessor httpContextAccessor)
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

        public void SetDbUser(DbUser user)
        {
            var items = _httpContextAccessor?.HttpContext?.Items;
            if(items != null)
            {
                items[DB_USER_KEY] = user;
            }
        }

        public DbUser GetDbUser()
        {
            var items = _httpContextAccessor?.HttpContext?.Items;
            return items == null ? null : items[DB_USER_KEY] as DbUser;
        }
    }
}
