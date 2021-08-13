using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tellma.Api.Instances
{
    public class InstanceInfoProvider
    {
        /// <summary>
        /// The sole purpose of this is to identify a running C# instance in a multi-instance environment 
        /// (e.g. Azure or a Web Farm), for the purpose of adopting and running tenant background jobs.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Returns the tenant Ids currently adopted by the current C# instance. 
        /// The current instance is responsible for carrying out their background jobs.
        /// </summary>
        public IEnumerable<int> AdoptedTenantIds => _adoptedTenantIds.ToArray();

        /// <summary>
        /// All the tenant Ids currently adopted by the current instance.
        /// </summary>
        private readonly ConcurrentBag<int> _adoptedTenantIds = new();

        /// <summary>
        /// Adds the new collection of orphans to the current instance's list of adoptions.
        /// </summary>
        internal void AddNewlyAdoptedOrphans(IEnumerable<int> orphans)
        {
            foreach(var tenantId in orphans)
            {
                _adoptedTenantIds.Add(tenantId);
            }
        }
    }
}
