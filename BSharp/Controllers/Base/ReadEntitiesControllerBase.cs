using BSharp.Controllers.Dto;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    public abstract class ReadEntitiesControllerBase<TEntity, TKey> : ReadControllerBase<TEntity>
        where TEntity : EntityWithKey<TKey>
    {
        // Private Fields
        private readonly ILogger _logger;

        // Constructor
        public ReadEntitiesControllerBase(ILogger logger, IStringLocalizer localizer) : base(logger, localizer)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TEntity>>> GetById(TKey id, [FromQuery] GetByIdArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await GetByIdImplAsync(id, args);
                return Ok(result);
            }, _logger);
        }

        /// <summary>
        /// Returns a single entity as per the ID and specifications in the get request
        /// </summary>
        protected virtual async Task<GetByIdResponse<TEntity>> GetByIdImplAsync(TKey id, [FromQuery] GetByIdArguments args)
        {
            // Parse the parameters
            var expand = ExpandExpression.Parse(args.Expand);
            var select = SelectExpression.Parse(args.Select);

            // Prepare the odata query
            var repo = GetRepository();
            var query = repo.Query<TEntity>();

            // Add the filter by Id
            query = query.FilterByIds(id);

            // Check that the entity exists
            int count = await query.CountAsync();
            if (count == 0)
            {
                throw new NotFoundException<TKey>(id);
            }

            // Apply read permissions
            var permissions = await UserPermissions(Constants.Read);
            var permissionsFilter = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsFilter);

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            var expandedQuery = query.Expand(expand);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            expandedQuery = expandedQuery.Select(select);

            // Load
            var result = await expandedQuery.FirstOrDefaultAsync();
            if (result == null)
            {
                // We already checked for not found earlier,
                // This can only mean lack of permissions
                throw new ForbiddenException();
            }
            
            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var singleton = new List<TEntity> { result };
            await ApplyReadPermissionsMask(singleton, query, permissions, GetDefaultMask());

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(singleton, expand);

            // Return
            return new GetByIdResponse<TEntity>
            {
                Result = result,
                CollectionName = GetCollectionName(typeof(TEntity)),
                RelatedEntities = relatedEntities
            };
        }

        protected async Task<EntitiesResponse<TEntity>> GetByIdListAsync(TKey[] ids, ExpandExpression expand = null)
        {
            // Prepare a query of the result, and clone it
            var repo = GetRepository();
            var query = repo.Query<TEntity>();

            // Filter by Ids
            query = query.FilterByIds(ids);

            // Expand the result as specified in the OData agruments and load into memory
            var expandedQuery = query.Expand(expand);
            expandedQuery = expandedQuery.OrderBy(OrderByExpression.Parse("Id")); // Required
            var result = await expandedQuery.ToListAsync(); // this is potentially unordered, should that be a concern?

            // Apply the permissions on the result
            var permissions = await UserPermissions(Constants.Read);
            var defaultMask = GetDefaultMask();
            await ApplyReadPermissionsMask(result, query, permissions, defaultMask);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(result, expand);

            // Sort the entities according to the original Ids, as a good practice
            TEntity[] sortedResult = new TEntity[ids.Length];
            Dictionary<TKey, TEntity> affectedEntitiesDic = result.ToDictionary(e => e.Id);
            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                TEntity entity = null;
                if (affectedEntitiesDic.ContainsKey(id))
                {
                    entity = affectedEntitiesDic[id];
                }

                sortedResult[i] = entity;
            }

            // Prepare the result in a response object
            return new EntitiesResponse<TEntity>
            {
                Result = sortedResult,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TEntity))
            };
        }

        /// <summary>
        /// Many actions are simply a list of Ids
        /// This is a utility method for permission actions that do not have a mask, it checks that the
        /// user has enough permissions to cover the entire list of Ids affected by the action
        /// </summary>
        protected virtual async Task CheckActionPermissions(string action, params TKey[] ids)
        {
            var permissions = await UserPermissions(action);
            if (!permissions.Any())
            {
                // User has no permissions on this view whatsoever, forbid
                throw new ForbiddenException();
            }
            else if (permissions.Any(e => string.IsNullOrWhiteSpace(e.Criteria)))
            {
                // User has at least one unrestricted permission to do this action => proceed
                return;
            }
            else
            {
                var permissionsFilter = permissions
                    .Select(e => FilterExpression.Parse(e.Criteria))
                    .Aggregate((e1, e2) => FilterDisjunction.Make(e1, e2));

                var query = GetRepository()
                    .Query<TEntity>()
                    .Select(SelectExpression.Parse("Id"))
                    .FilterByIds(ids);

                var filteredQuery = query.Filter(permissionsFilter);

                var countBeforeFilter = await query.CountAsync();
                var countAfterFilter = await filteredQuery.CountAsync();

                if (countBeforeFilter > countAfterFilter)
                {
                    throw new ForbiddenException();
                }
            }
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse("Id desc");
        }


        /// <summary>
        /// If the user is subject to field-level access control, this method hides all the fields
        /// that the user has no access to and modifies the metadata of the Entities accordingly
        /// </summary>
        protected override async Task ApplyReadPermissionsMask(
            List<TEntity> resultEntities,
            Query<TEntity> query,
            IEnumerable<AbstractPermission> permissions,
            MaskTree defaultMask)
        {
            bool defaultMaskIsUnrestricted = defaultMask == null || defaultMask.IsUnrestricted;
            bool allPermissionMasksAreEmpty = permissions.All(e => string.IsNullOrWhiteSpace(e.Mask));
            bool anEmptyCriteriaIsPairedWithEmptyMask =
                permissions.Any(e => string.IsNullOrWhiteSpace(e.Mask) && string.IsNullOrWhiteSpace(e.Criteria));

            if ((allPermissionMasksAreEmpty || anEmptyCriteriaIsPairedWithEmptyMask) && defaultMaskIsUnrestricted)
            {
                // Optimization: if all masks are unrestricted, or an empty criteria is paired with an empty mask then we can skip this whole ordeal
                return;
            }
            else
            {
                // Maps every Entity to its list of masks
                var maskedEntities = new Dictionary<Entity, HashSet<string>>();
                var unrestrictedEntities = new HashSet<Entity>();

                // Marks the Entity and all Entities reachable from it as unrestricted
                void MarkUnrestricted(Entity entity, Type entityType)
                {
                    if (entity == null)
                    {
                        return;
                    }

                    if (maskedEntities.ContainsKey(entity))
                    {
                        maskedEntities.Remove(entity);
                    }

                    if (!unrestrictedEntities.Contains(entity))
                    {
                        unrestrictedEntities.Add(entity);
                        foreach (var key in entity.EntityMetadata.Keys)
                        {
                            var prop = entityType.GetProperty(key);
                            if (prop.PropertyType.IsList())
                            {
                                // This is a navigation collection, iterate over the rows
                                var collection = prop.GetValue(entity);
                                if (collection != null)
                                {
                                    var collectionType = prop.PropertyType.CollectionType();
                                    foreach (var row in collection.Enumerate<Entity>())
                                    {
                                        MarkUnrestricted(row, collectionType);
                                    }
                                }
                            }
                            else
                            {
                                // This is a normal navigation property
                                var propValue = prop.GetValue(entity) as Entity;
                                var propType = prop.PropertyType;
                                MarkUnrestricted(propValue, propType);
                            }
                        }
                    }
                }

                // Goes over this entity and every entity reachable from it and marks each one with the accessible fields
                void MarkMask(Entity entity, Type entityType, MaskTree mask)
                {
                    if (entity == null)
                    {
                        return;
                    }

                    if (mask.IsUnrestricted)
                    {
                        MarkUnrestricted(entity, entityType);
                    }
                    else
                    {
                        if (unrestrictedEntities.Contains(entity))
                        {
                            // Nothing to mask in an unrestricted Entity
                            return;
                        }
                        else
                        {
                            if (!maskedEntities.ContainsKey(entity))
                            {
                                // All entities will have their basic fields accessible
                                var accessibleFields = new HashSet<string>();
                                foreach (var basicField in entityType.AlwaysAccessibleFields())
                                {
                                    accessibleFields.Add(basicField.Name);
                                }

                                maskedEntities[entity] = accessibleFields;
                            }

                            {
                                var accessibleFields = maskedEntities[entity];
                                foreach (var requestedField in entity.EntityMetadata.Keys)
                                {
                                    var prop = entityType.GetProperty(requestedField);
                                    if (mask.ContainsKey(requestedField))
                                    {
                                        // If the field is included in the mask, make it accessible
                                        if (!accessibleFields.Contains(requestedField))
                                        {
                                            accessibleFields.Add(requestedField);
                                        }

                                        if (prop.PropertyType.IsList())
                                        {
                                            // This is a navigation collection, iterate over the rows and apply the mask subtree
                                            var collection = prop.GetValue(entity);
                                            if (collection != null)
                                            {
                                                var collectionType = prop.PropertyType.CollectionType();
                                                foreach (var row in collection.Enumerate<Entity>())
                                                {
                                                    MarkMask(row, collectionType, mask[requestedField]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var foreignKeyNameAtt = prop.GetCustomAttribute<ForeignKeyAttribute>();
                                            if (foreignKeyNameAtt != null)
                                            {
                                                // Make sure if the navigation property is included that its foreign key is included as well
                                                var foreignKeyName = foreignKeyNameAtt.Name;
                                                if (!string.IsNullOrWhiteSpace(foreignKeyName) && !accessibleFields.Contains(foreignKeyName))
                                                {
                                                    accessibleFields.Add(foreignKeyName);
                                                }

                                                // Use recursion to update the rest of the tree
                                                var propValue = prop.GetValue(entity) as Entity;
                                                var propType = prop.PropertyType;
                                                MarkMask(propValue, propType, mask[requestedField]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (permissions.All(e => string.IsNullOrWhiteSpace(e.Criteria)))
                {
                    // Having no criteria is a very common case that can be optimized by skipping the database call
                    var addDefault = permissions.Any(p => string.IsNullOrWhiteSpace(p.Mask));
                    var masks = permissions.Select(e => e.Mask).Where(e => !string.IsNullOrWhiteSpace(e));
                    var maskTrees = masks.Select(mask => MaskTree.Parse(mask)).ToList();
                    if (addDefault)
                    {
                        maskTrees.Add(defaultMask);
                    }

                    // Calculate the union of all the mask fields
                    var maskUnion = maskTrees.Aggregate(MaskTree.BasicFieldsMaskTree(), (t1, t2) => t1.UnionWith(t2));

                    // Mark all the entities
                    var entityType = typeof(TEntity);
                    foreach (var item in resultEntities)
                    {
                        MarkMask(item, entityType, maskUnion);
                    }
                }
                else
                {
                    // an array of every criteria and every mask
                    var maskAndCriteriaArray = permissions
                        .Where(e => !string.IsNullOrWhiteSpace(e.Criteria)) // Optimization: a null criteria is satisfied by the entire list of entities
                        .GroupBy(e => e.Criteria)
                        .Select(g => new
                        {
                            Criteria = g.Key,
                            Mask = g.Select(e => string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : MaskTree.Parse(e.Mask))
                            .Aggregate((t1, t2) => t1.UnionWith(t2)) // takes the union of all the mask trees
                        }).ToArray();

                    // This mask applies to every single entity since the criteria is null
                    var universalMask = permissions
                        .Where(e => string.IsNullOrWhiteSpace(e.Criteria))
                        .Distinct()
                        .Select(e => string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : MaskTree.Parse(e.Mask))
                        .Aggregate(MaskTree.BasicFieldsMaskTree(), (t1, t2) => t1.UnionWith(t2)); // we use a seed here since if the collection is empty this will throw an error

                    var criteriaWithIndexes = maskAndCriteriaArray
                        .Select((e, index) => new IndexAndCriteria { Criteria = e.Criteria, Index = index });

                    var criteriaMapList = await query.GetIndexToIdMap<TKey>(criteriaWithIndexes);

                    // Go over the Ids in the result and apply all relevant masks to said entity
                    var entityType = typeof(TEntity);
                    var criteriaMapDictionary = criteriaMapList
                        .GroupBy(e => e.Id)
                        .ToDictionary(e => e.Key, e => e.ToList());

                    foreach (var entity in resultEntities)
                    {
                        var id = entity.Id;
                        MaskTree mask;

                        if (criteriaMapDictionary.ContainsKey(id))
                        {
                            // Those are entities that satisfy one or more non-null Criteria
                            mask = criteriaMapDictionary[id]
                                .Select(e => maskAndCriteriaArray[e.Index].Mask)
                                .Aggregate((t1, t2) => t1.UnionWith(t2))
                                .UnionWith(universalMask);
                        }
                        else
                        {
                            // Those are entities that belong to the universal mask of null criteria
                            mask = universalMask;
                        }

                        MarkMask(entity, entityType, mask);
                    }
                }

                // This where field-level security is applied, we read all masked entities and apply the
                // masks on them by setting the field to null and adjusting the metadata accordingly
                foreach (var pair in maskedEntities)
                {
                    var entity = pair.Key;
                    var accessibleFields = pair.Value;

                    List<Action> updates = new List<Action>(entity.EntityMetadata.Keys.Count);
                    foreach (var requestedField in entity.EntityMetadata.Keys)
                    {
                        if (!accessibleFields.Contains(requestedField))
                        {
                            // Mark the field as restricted (we delay the call to avoid the dreadful "collection-was-modified" Exception)
                            updates.Add(() => entity.EntityMetadata[requestedField] = FieldMetadata.Restricted);

                            // Set the field to null
                            var prop = entity.GetType().GetProperty(requestedField);
                            try
                            {
                                prop.SetValue(entity, null);
                            }
                            catch (Exception ex)
                            {
                                if (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
                                {
                                    // Programmer mistake
                                    throw new InvalidOperationException($"Entity field {prop.Name} has a non nullable type, all Entity fields must have a nullable type");
                                }
                                else
                                {
                                    throw ex;
                                }
                            }
                        }
                    }

                    updates.ForEach(a => a());
                }
            }
        }
    }
}
