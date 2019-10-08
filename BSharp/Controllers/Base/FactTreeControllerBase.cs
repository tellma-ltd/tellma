using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
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
        public FactTreeControllerBase(ILogger logger, IStringLocalizer localizer) : base(logger, localizer)
        {
            _logger = logger;
        }

        // Children-of is replicated in CrudTreeControllerBase, please keep them in sync
        [HttpGet("children-of")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetChildrenOf([FromQuery] GetChildrenArguments<TKey> args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await GetChildrenOfAsync(args);
                return Ok(result);
            }, _logger);
        }

        /// <summary>
        /// Returns a single entity as per the ID and specifications in the get request
        /// </summary>
        protected virtual async Task<EntitiesResponse<TEntity>> GetChildrenOfAsync(GetChildrenArguments<TKey> args)
        {
            // Parse the parameters
            var expand = ExpandExpression.Parse(args.Expand);
            var select = SelectExpression.Parse(args.Select);
            var filter = FilterExpression.Parse(args.Filter);
            var orderby = OrderByExpression.Parse("Node");
            var ids = args.I ?? new List<TKey>();

            return await GetByCustomQuery(q => q.FilterByParentIds(ids, includeRoots: args.Roots).Filter(filter), expand, select, orderby);
        }
    }
}
