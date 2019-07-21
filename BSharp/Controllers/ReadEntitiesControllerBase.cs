using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace BSharp.Controllers
{

    public abstract class ReadEntitiesControllerBase<TDto, TKey> : ReadControllerBase<TDto>
        where TDto : DtoKeyBase<TKey>
    {
        // Private Fields
        private readonly ILogger _logger;

        // Constructor
        public ReadEntitiesControllerBase(ILogger logger, IStringLocalizer localizer, IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TDto>>> GetById(TKey id, [FromQuery] GetByIdArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                var result = await GetByIdImplAsync(id, args);
                return Ok(result);
            }, _logger);
        }

        /// <summary>
        /// Returns a single entity as per the ID and specifications in the get request
        /// </summary>
        protected virtual async Task<GetByIdResponse<TDto>> GetByIdImplAsync(TKey id, [FromQuery] GetByIdArguments args)
        {
            // Prepare the odata query
            var query = CreateODataQuery();

            // Add the filter by Id
            query.FilterByIds(id);

            // Check that the entity exists
            int count = await query.CountAsync();
            if (count == 0)
            {
                throw new NotFoundException<TKey>(id);
            }

            // Apply read permissions
            var permissions = await UserPermissions(PermissionLevel.Read);
            string permissionsCriteria = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsCriteria);

            // Take a copy of the query without the expand or the select
            var qClone = query.Clone();

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            query.Expand(args.Expand);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            query.Select(args.Select);

            // Load
            var (result, entities) = await query.FirstOrDefaultAsync();
            if (result == null)
            {
                // We already checked for not found earlier,
                // This can only mean lack of permissions
                throw new ForbiddenException();
            }

            var collectionName = GetCollectionName(typeof(TDto));

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var singleton = new List<TDto> { result };
            await ApplyReadPermissionsMask(singleton, entities, collectionName, qClone, permissions, GetDefaultMask());

            // Return
            return new GetByIdResponse<TDto>
            {
                Result = result,
                CollectionName = collectionName,
                RelatedEntities = entities
            };
        }

        protected async Task<EntitiesResponse<TDto>> GetByIdListAsync(TKey[] ids, string expand, System.Data.Common.DbTransaction trx = null)
        {
            // Prepare a query of the result, and clone it
            var query = CreateODataQuery();
            if (trx != null)
            {
                query.UseTransaction(trx);
            }

            query.FilterByIds(ids.ToArray());
            var qClone = query.Clone();

            // Expand the result as specified in the OData agruments and load into memory
            query.Expand(expand);
            var (resultIds, entities) = await query.ToListAsync(); // this is potentially unordered, should that be a concern?

            // Apply the permissions on the result
            var permissions = await UserPermissions(PermissionLevel.Read);
            var defaultMask = GetDefaultMask();
            var collectionName = GetCollectionName(typeof(TDto));
            await ApplyReadPermissionsMask(resultIds, entities, collectionName, qClone, permissions, defaultMask);

            // Sort the entities according to the original Ids, as a good practice
            TDto[] sortedResult = new TDto[ids.Length];
            Dictionary<TKey, TDto> affectedEntitiesDic = resultIds.ToDictionary(e => e.Id);
            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                TDto entity = null;
                if (affectedEntitiesDic.ContainsKey(id))
                {
                    entity = affectedEntitiesDic[id];
                }

                sortedResult[i] = entity;
            }

            // Prepare the result in a response object
            var result = new EntitiesResponse<TDto>
            {
                Result = sortedResult,
                RelatedEntities = entities,
                CollectionName = collectionName
            };

            return result;
        }

        /// <summary>
        /// Verifies that the user has sufficient permissions to update the list of entities provided, this implementation 
        /// assumes that the view has permission levels Read and Update only, which most entities
        /// </summary>
        protected virtual async Task CheckActionPermissions(IEnumerable<TKey> entityIds)
        {
            // TODO

            //var updatePermissions = await UserPermissions(PermissionLevel.Update);
            //if (!updatePermissions.Any())
            //{
            //    // User has no permissions on this table whatsoever, forbid
            //    throw new ForbiddenException();
            //}
            //else if (updatePermissions.Any(e => string.IsNullOrWhiteSpace(e.Criteria)))
            //{
            //    // User has unfiltered update permission on the table => proceed
            //    return;
            //}
            //else
            //{
            //    // User can update items under certain conditions, so we check those conditions here
            //    IEnumerable<string> criteriaList = updatePermissions.Select(e => e.Criteria);

            //    // The parameter on which the expression is based
            //    var eParam = Expression.Parameter(typeof(TDto));

            //    // Prepare the lambda
            //    Expression whereClause = ToORedWhereClause<TDto>(criteriaList, eParam);
            //    var lambda = Expression.Lambda<Func<TDto, bool>>(whereClause, eParam);

            //    await CheckPermissionsForOld(entityIds, lambda);
            //}
        }

        protected override string DefaultOrderBy()
        {
            return "Id desc";
        }
    }
}
