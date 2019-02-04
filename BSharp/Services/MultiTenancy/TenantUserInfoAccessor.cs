using Microsoft.AspNetCore.Http;

namespace BSharp.Services.MultiTenancy
{
    public class TenantUserInfoAccessor : ITenantUserInfoAccessor
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITenantIdProvider _tenantIdProvider;

        public TenantUserInfoAccessor(IHttpContextAccessor contextAccessor, ITenantIdProvider tenantIdProvider)
        {
            _contextAccessor = contextAccessor;
            _tenantIdProvider = tenantIdProvider;
        }

        public TenantUserInfo GetInfo(int tenantId)
        {
            return _contextAccessor.HttpContext.Items[Key(tenantId)] as TenantUserInfo;
        }

        public void SetInfo(int tenantId, TenantUserInfo info)
        {
            _contextAccessor.HttpContext.Items[Key(tenantId)] = info;
        }

        public TenantUserInfo GetCurrentInfo()
        {
            return GetInfo(_tenantIdProvider.GetTenantId().Value);
        }

        private string Key(int tenantId)
        {
            return $"Tenant-Info-{tenantId}";
        }
    }
}
