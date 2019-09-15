using BSharp.Data;

namespace BSharp.Services.MultiTenancy
{
    /// <summary>
    /// Provides access to the session-scoped tenant information that was automatically
    /// initialized in <see cref="ApplicationApiAttribute"/>. It is useful for consumption
    /// by singleton services since this itself is singleton-scoped.
    /// </summary>
    public interface ITenantInfoAccessor
    {
        void SetInfo(int tenantId, TenantInfo info);

        TenantInfo GetInfo(int tenantId);

        TenantInfo GetCurrentInfo();
    }
}
