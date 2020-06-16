using Tellma.Controllers.Dto;
using Tellma.Services.MultiTenancy;
using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace Tellma.Controllers
{
    public class DefinitionsCache : IDefinitionsCache
    {
        private static string HttpContextKey(int databaseId) => $"REQUEST_DEFINITIONS/{databaseId}";

        /// <summary>
        /// Mapping from database ID to its <see cref="DefinitionsForClient"/>
        /// </summary>
        private static readonly ConcurrentDictionary<int, Versioned<DefinitionsForClient>> _cache 
            = new ConcurrentDictionary<int, Versioned<DefinitionsForClient>>();

        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IHttpContextAccessor _contextAccessor;

        public DefinitionsCache(ITenantIdAccessor tenantIdAccessor, IHttpContextAccessor contextAccessor)
        {
            _tenantIdAccessor = tenantIdAccessor;
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Implementation of <see cref="IDefinitionsCache"/>
        /// </summary>
        public Versioned<DefinitionsForClient> GetDefinitionsIfCached(int tenantId, bool forceFresh = false)
        {
            // This first step ensures that the same definitions are always returned within
            // the scope of a single request, even if another thread updates the cache
            var ctx = _contextAccessor.HttpContext;
            if (!forceFresh && ctx.Items.TryGetValue(HttpContextKey(tenantId), out object defsObj) && defsObj is Versioned<DefinitionsForClient> definitions)
            {
                return definitions;
            }

            _cache.TryGetValue(tenantId, out definitions);

            // Set the definitions in the HttpContext, to ensure subsequent requests return the same result
            if (definitions != null)
            {
                var key = HttpContextKey(tenantId);
                if (!ctx.Items.ContainsKey(key))
                {
                    ctx.Items.Add(key, definitions);
                }
            }

            return definitions;
        }

        public Versioned<DefinitionsForClient> GetCurrentDefinitionsIfCached(bool forceFresh = false)
        {
            int tenantId = _tenantIdAccessor.GetTenantId();
            return GetDefinitionsIfCached(tenantId, forceFresh);
        }

        /// <summary>
        /// Implementation of <see cref="IDefinitionsCache"/>
        /// </summary>
        public void SetDefinitions(int tenantId, Versioned<DefinitionsForClient> definitions)
        {
            if (tenantId == 0)
            {
                throw new ArgumentException($"{nameof(tenantId)} must be provided");
            }

            if (definitions is null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            _cache.AddOrUpdate(tenantId, definitions, (i, d) => definitions);
        }
    }
}
