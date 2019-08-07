using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.MultiTenancy
{
    /// <summary>
    /// This service retrieves the tenant id from the request header
    /// or null otherwise
    /// </summary>
    public class TenantIdProvider : ITenantIdProvider
    {
        public const string REQUEST_HEADER_TENANT_ID = "X-Tenant-Id";
        private readonly IHttpContextAccessor _accessor;

        public TenantIdProvider(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public int? GetTenantId()
        {
            string tenantIdString;

            var headers = _accessor?.HttpContext?.Request?.Headers;
            if (headers != null && headers.ContainsKey(REQUEST_HEADER_TENANT_ID))
            {
                tenantIdString = headers[REQUEST_HEADER_TENANT_ID];
                if (int.TryParse(tenantIdString, out int tenantId))
                {
                    return tenantId;
                }
            }

            return null;
        }
    }
}
