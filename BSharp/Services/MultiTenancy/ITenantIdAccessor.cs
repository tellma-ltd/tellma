namespace BSharp.Services.MultiTenancy
{
    /// <summary>
    /// Service for extracting the tenant Id from the request context
    /// </summary>
    public interface ITenantIdAccessor
    {
        /// <summary>
        /// Retrieves the tenant Id specified by the client in the request, or throws a <see cref="MultitenancyException"/> if none is available.
        /// Note: The authenticated user may or may not be a member of the tenant Id returned by this method
        /// </summary>
        /// <returns>An int32 representing the tenantId</returns>
        /// <exception cref="MultitenancyException"></exception>
        int GetTenantId();

        // TODO: Check if Used, remove otherwise

        /// <summary>
        /// Tries to retrieves the tenant Id from the request context
        /// Note: The authenticated user may or may not be a member of the tenant Id returned by this method
        /// </summary>
        /// <param name="tenantId">The tenantId if any, null otherwise</param>
        void TryGetTenantId(out int? tenantId);
    }
}
