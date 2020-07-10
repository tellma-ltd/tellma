using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Data;

namespace Tellma.Controllers
{
    /// <summary>
    /// Caching for user permissions
    /// </summary>
    public static class PermissionsCache
    {
        /// <summary>
        /// Retrives the permissions of the current user from the database accessible through the given repository, 
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<AbstractPermission>> AllPermissionsFromCache(this ApplicationRepository repo, CancellationToken cancellation)
        {
            // Grab contextual information
            var userInfo = repo.GetUserInfo();
            int userId = userInfo.UserId ?? throw new InvalidOperationException("No user was returned from repository");
            string dbPermissionsVersion = userInfo.PermissionsVersion ?? throw new InvalidOperationException("No permissions version was returned from repository");
            int tenantId = repo.GetTenantId();

            // Ensure the cache entry is created
            var cacheEntry = _cache.GetOrAdd((tenantId, userId), _ => new CacheEntry());

            // first check
            if (cacheEntry.Permissions?.Version != dbPermissionsVersion)
            {
                // Cache miss: enter the semaphore
                var semaphore = cacheEntry.Semaphore;
                await semaphore.WaitAsync(cancellation);
                try
                {
                    // A second OCD-check inside the semaphore
                    if (cacheEntry.Permissions?.Version != dbPermissionsVersion)
                    {
                        // Load from DB
                        var (guid, permissions) = await repo.Permissions__Load(cancellation);

                        // Set the cache
                        cacheEntry.Permissions = new Versioned<IEnumerable<AbstractPermission>>(
                            version: guid.ToString(),
                            data: permissions
                        );
                    }
                }
                finally
                {
                    // Very important
                    semaphore.Release();
                }
            }

            // Always return from the cache
            return cacheEntry.Permissions.Data;
        }

        public static async Task<IEnumerable<AbstractPermission>> PermissionsFromCache(
            this ApplicationRepository repo, string view, string action, CancellationToken cancellation)
        {
            var permissions = await repo.AllPermissionsFromCache(cancellation);

            // Apply view filter
            permissions = permissions.Where(e => e.View == view || e.View == "all");

            // Apply action filter (read action is implicity in all 
            if (action != Services.Utilities.Constants.Read)
            {
                permissions = permissions.Where(e => e.Action == action || e.Action == "All");
            }

            // Return
            return permissions;
        }

        public static async Task<IEnumerable<AbstractPermission>> GenericPermissionsFromCache(
            this ApplicationRepository repo, string viewPrefix, string action, CancellationToken cancellation)
        {
            var permissions = await repo.AllPermissionsFromCache(cancellation);

            // Apply view filter
            permissions = permissions.Where(e => e.View.StartsWith(viewPrefix) || e.View == "all");

            // Apply action filter (read action is implicity in all 
            if (action != Services.Utilities.Constants.Read)
            {
                permissions = permissions.Where(e => e.Action == action || e.Action == "All");
            }

            // Return
            return permissions;
        }

        /// <summary>
        /// Maps tenantId and user Id to a <see cref="CacheEntry"/>
        /// </summary>
        private static readonly ConcurrentDictionary<(int, int), CacheEntry> _cache = new ConcurrentDictionary<(int, int), CacheEntry>();

        /// <summary>
        /// Simple DTO to store the cached permission of users and tenants along with a <see cref="Semaphore"/> for handling a cache miss
        /// </summary>
        private class CacheEntry
        {
            /// <summary>
            /// To synchronize the threads that are trying to retrieve permissions for the same user in the same company form the DB
            /// </summary>
            public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

            /// <summary>
            /// The cached permissions and their version
            /// </summary>
            public Versioned<IEnumerable<AbstractPermission>> Permissions { get; set; }
        }
    }
}
