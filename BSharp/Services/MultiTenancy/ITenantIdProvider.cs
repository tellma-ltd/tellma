using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.MultiTenancy
{
    /// <summary>
    /// Service for accessing the tenant Id in the request
    /// </summary>
    public interface ITenantIdProvider
    {
        /// <summary>
        /// Retrieves the tenant Id from the request headers, or null if none is available
        /// </summary>
        /// <returns>An int32 representing the tenantId</returns>
        int? GetTenantId();
    }
}
