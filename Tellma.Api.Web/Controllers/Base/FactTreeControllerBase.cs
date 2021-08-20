using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Common;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments
    /// and allow selecting a certain record by Id.
    /// </summary>
    public abstract class FactTreeControllerBase<TEntity, TKey> : FactGetByIdControllerBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        // IMPORTANT: Children-of is replicated in CrudTreeControllerBase, please keep them in sync
        [HttpGet("children-of")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetChildrenOf([FromQuery] GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Load the data
            var service = GetFactTreeService();
            var result = await service.GetChildrenOf(args, cancellation);

            var response = TransformToEntitiesResponse(result, serverTime, cancellation);
            return Ok(response);
        }

        protected override FactGetByIdServiceBase<TEntity, TKey> GetFactGetByIdService()
        {
            return GetFactTreeService();
        }

        protected abstract FactTreeServiceBase<TEntity, TKey> GetFactTreeService();
    }
}
