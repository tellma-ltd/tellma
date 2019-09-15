using Microsoft.AspNetCore.Http;

namespace BSharp.Services.MultiTenancy
{
    /// <summary>
    /// This service extracts the tenant id from the request context,
    /// </summary>
    public class TenantIdAccessor : ITenantIdAccessor
    {
        public const string REQUEST_HEADER_TENANT_ID = "X-Tenant-Id";
        private readonly IHttpContextAccessor _contextAccessor;

        public TenantIdAccessor(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public int GetTenantId()
        {
            string tenantIdString;

            var headers = _contextAccessor?.HttpContext?.Request?.Headers;
            if (headers != null && headers.ContainsKey(REQUEST_HEADER_TENANT_ID))
            {
                tenantIdString = headers[REQUEST_HEADER_TENANT_ID];
                if (int.TryParse(tenantIdString, out int tenantId))
                {
                    return tenantId;
                }
                else
                {
                    throw new MultitenancyException($"The header '{REQUEST_HEADER_TENANT_ID}' must have an integer value");
                }
            }
            else
            {
                throw new MultitenancyException($"The required header '{REQUEST_HEADER_TENANT_ID}' was not supplied");
            }
        }
    }
}
