namespace BSharp.Services.MultiTenancy
{
    /// <summary>
    /// Provides access to the session-scoped tenant and user information that was automatically
    /// initialized as soon as a connection was made to <see cref="Data.ApplicationContext"/>
    /// controllers and other classes may wish to access the user Id and tenant languages using this service
    /// </summary>
    public interface ITenantUserInfoAccessor
    {
        void SetInfo(int tenantId, TenantUserInfo info);
        TenantUserInfo GetInfo(int tenantId);
        TenantUserInfo GetCurrentInfo();
    }
}
