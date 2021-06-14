using Tellma.Data;
using Microsoft.AspNetCore.Http;

namespace Tellma.Services.MultiTenancy
{
    public class TenantInfoAccessor : ITenantInfoAccessor
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITenantIdAccessor _tenantIdProvider;

        public TenantInfoAccessor(IHttpContextAccessor contextAccessor, ITenantIdAccessor tenantIdProvider)
        {
            _contextAccessor = contextAccessor;
            _tenantIdProvider = tenantIdProvider;
        }

        public TenantInfo GetInfo(int tenantId)
        {
            return _contextAccessor.HttpContext.Items[Key(tenantId)] as TenantInfo;
        }

        public void SetInfo(int tenantId, TenantInfo info)
        {
            _contextAccessor.HttpContext.Items[Key(tenantId)] = info;
        }

        public TenantInfo GetCurrentInfo()
        {
            return GetInfo(_tenantIdProvider.GetTenantId());
        }

        private string Key(int tenantId)
        {
            return $"Tenant-Info-{tenantId}";
        }
    }
}
