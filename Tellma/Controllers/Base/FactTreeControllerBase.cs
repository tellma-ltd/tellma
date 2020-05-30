using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id
    /// </summary>
    public abstract class FactTreeControllerBase<TEntity, TKey> : FactGetByIdControllerBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        // Private Fields
        private readonly ILogger _logger;

        // Constructor
        public FactTreeControllerBase(ILogger logger) : base(logger)
        {
            _logger = logger;
        }

        // IMPORTANT: Children-of is replicated in CrudTreeControllerBase, please keep them in sync
        [HttpGet("children-of")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetChildrenOf([FromQuery] GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetFactTreeService();
                var (data, extras) = await service.GetChildrenOf(args, cancellation);

                var result = TransformToEntitiesResponse(data, extras, serverTime, cancellation);
                return Ok(result);
            }, _logger);
        }

        protected override FactGetByIdServiceBase<TEntity, TKey> GetFactGetByIdService()
        {
            return GetFactTreeService();
        }

        protected abstract FactTreeServiceBase<TEntity, TKey> GetFactTreeService();
    }

    public abstract class FactTreeServiceBase<TEntity, TKey> : FactGetByIdServiceBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        public FactTreeServiceBase(IServiceProvider sp) : base(sp)
        {
        }

        /// <summary>
        /// Returns a list of entities as per the specifications in the <see cref="GetChildrenArguments{TKey}"/>
        /// </summary>
        public virtual async Task<(List<TEntity>, Extras)> GetChildrenOf(GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            // Parse the parameters
            var expand = ExpandExpression.Parse(args.Expand);
            var select = ParseSelect(args.Select);
            var filter = FilterExpression.Parse(args.Filter);
            var orderby = OrderByExpression.Parse("Node");
            var ids = args.I ?? new List<TKey>();

            // Load the data
            var data = await GetEntitiesByCustomQuery(q => q.FilterByParentIds(ids, args.Roots).Filter(filter), expand, select, orderby, cancellation);
            var extras = await GetExtras(data, cancellation);

            // Transform and Return
            return (data, extras);
        }
    }
}
