using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// The only purpose of this is to get rid of the IdentitySever warning.
    /// In reality the embedded identity server does not use any persisted grants.
    /// </summary>
    public class PersistedGrantStore : IPersistedGrantStore
    {
        public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            throw new InvalidOperationException($"Grants are not supported.");
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            throw new InvalidOperationException($"Grants are not supported.");
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            throw new InvalidOperationException($"Grants are not supported.");
        }

        public Task RemoveAsync(string key)
        {
            throw new InvalidOperationException($"Grants are not supported.");
        }

        public Task StoreAsync(PersistedGrant grant)
        {
            throw new InvalidOperationException($"Grants are not supported.");
        }
    }
}
