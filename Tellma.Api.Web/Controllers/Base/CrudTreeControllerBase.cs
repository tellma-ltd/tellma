using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Common;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id, as well as updating, deleting, deleting with descendants
    /// and importing lists of that entity.
    /// </summary>
    public abstract class CrudTreeControllerBase<TEntityForSave, TEntity, TKey> : CrudControllerBase<TEntityForSave, TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        public CrudTreeControllerBase(IServiceProvider sp) : base(sp)
        {
        }

        // IMPORTANT: Children-of is replicated in FactTreeControllerBase, please keep them in sync
        [HttpGet("children-of")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetChildrenOf([FromQuery] GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Load the data
            var service = GetCrudTreeService();
            var (data, extras) = await service.GetChildrenOf(args, cancellation);

            var result = TransformToEntitiesResponse(data, extras, serverTime, cancellation);
            return Ok(result);
        }

        [HttpDelete("with-descendants")]
        public virtual async Task<ActionResult> DeleteWithDescendants([FromQuery] List<TKey> i)
        {
            // "i" parameter is given a short name to allow a large number of
            // ids to be passed in the query string before the url size limit
            var service = GetCrudTreeService();
            await service.DeleteWithDescendants(i);

            return Ok();
        }

        protected override CrudServiceBase<TEntityForSave, TEntity, TKey> GetCrudService()
        {
            return GetCrudTreeService();
        }

        protected abstract CrudTreeServiceBase<TEntityForSave, TEntity, TKey> GetCrudTreeService();
    }
}
