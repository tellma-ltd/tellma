namespace Tellma.Services.MultiTenancy
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

        /// <summary>
        /// Retrieves the tenant Id specified by the client in the request if one is available, returns null otherwise
        /// </summary>
        /// <returns>An int32 representing the tenantId or null if one isn't available</returns>
        int? GetTenantIdIfAny();
    }
}
