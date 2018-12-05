using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.MultiTenancy
{
    public static class TenantIdProviderExtensions
    {
        public static bool HasTenantId(this ITenantIdProvider tenantIdProvider)
        {
            return tenantIdProvider.GetTenantId() != null;
        }
    }
}
