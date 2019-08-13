using Microsoft.AspNetCore.Http;
using System;

namespace BSharp.Services.ClientInfo
{
    public class ClientInfoAccessor : IClientInfoAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientInfoAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClientInfo GetInfo()
        {
            // TODO: Extract the real time zone from a header
            return new ClientInfo {
                TimeZone = TimeZoneInfo.Local
            };
        }
    }
}
