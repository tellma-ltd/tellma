using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tellma.Controllers.Jobs
{
    public static class Instance
    {
        /// <summary>
        /// The sole purpose of this is to identify a running C# instance in a multi-instance environment 
        /// (e.g. Azure or in a Web Farm), for the purpose of adopting and running tenant background jobs
        /// </summary>
        public static Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Returns the tenant Ids currently adopted by the current instance. 
        /// The current instance is responsible for carrying out their background jobs
        /// </summary>
        public static IEnumerable<int> AdoptedTenantIds => _adoptedTenantIds;

        /// <summary>
        /// All the tenant Ids currently adopted by the current instance
        /// </summary>
        private static readonly ConcurrentBag<int> _adoptedTenantIds = new ConcurrentBag<int>();

        /// <summary>
        /// Adds the new collection of orphans to the current instance's list of adoptions
        /// </summary>
        public static void AddNewlyAdoptedOrphans(IEnumerable<int> orphans)
        {
            foreach(var tenantId in orphans)
            {
                _adoptedTenantIds.Add(tenantId);
            }
        }
    }
}
