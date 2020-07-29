using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Controllers.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// </summary>
    public abstract class FactWithIdControllerBase<TEntity, TKey> : FactControllerBase<TEntity>
        where TEntity : EntityWithKey<TKey>
    {
        // Constructor
        public FactWithIdControllerBase(IServiceProvider sp) : base(sp)
        {
        }

        [HttpGet("by-ids")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetByIds([FromQuery] GetByIdsArguments<TKey> args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetFactWithIdService();
                var (entities, extras) = await service.GetByIds(args.I, args, Constants.Read, cancellation);

                // Flatten and Trim
                var relatedEntities = FlattenAndTrim(entities, cancellation);

                // Prepare the result in a response object
                var result = new EntitiesResponse<TEntity>
                {
                    Result = entities,
                    RelatedEntities = relatedEntities,
                    CollectionName = GetCollectionName(typeof(TEntity)),
                    Extras = extras,
                    ServerTime = serverTime,
                };
                return Ok(result);
            }, _logger);
        }

        protected override FactServiceBase<TEntity> GetFactService()
        {
            return GetFactWithIdService();
        }

        protected abstract FactWithIdServiceBase<TEntity, TKey> GetFactWithIdService();

        /// <summary>
        /// Transforms the data and the other data into an <see cref="EntitiesResponse{TEntity}"/> ready to be served by a web handler, after verifying the user's permissions
        /// </summary>
        protected EntitiesResponse<TEntity> TransformToEntitiesResponse(List<TEntity> data, Extras extras, DateTimeOffset serverTime, CancellationToken cancellation)
        {
            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(data, cancellation);

            // Prepare the result in a response object
            return new EntitiesResponse<TEntity>
            {
                Result = data,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TEntity)),
                Extras = TransformExtras(extras, cancellation),
                ServerTime = serverTime,
            };
        }
    }

    public abstract class FactWithIdServiceBase<TEntity, TKey> : FactServiceBase<TEntity>, IFactWithIdService
        where TEntity : EntityWithKey<TKey>
    {
        public FactWithIdServiceBase(IServiceProvider sp) : base(sp)
        {
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Ids and the specifications in the <see cref="SelectExpandArguments"/>, after verifying the user's permissions
        /// </summary>
        public virtual async Task<(List<TEntity>, Extras)> GetByIds(List<TKey> ids, SelectExpandArguments args, string action, CancellationToken cancellation)
        {
            // Parse the parameters
            var expand = ExpandExpression.Parse(args?.Expand);
            var select = ParseSelect(args?.Select);

            // Prepare the permissions filter
            var permissionsFilter = await UserPermissionsFilter(action, cancellation);

            // Load the data
            var data = await GetEntitiesByIds(ids, expand, select, permissionsFilter, cancellation);
            var extras = await GetExtras(data, cancellation);

            // Return result
            return (data, extras);
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Ids and the specifications in the <see cref="ExpandExpression"/> and <see cref="SelectExpression"/>,
        /// after verifying the user's permissions, returns the entities in the same order as the supplied Ids.
        /// If null was supplied for permission filter, the function by default uses the filter for the read permissions filter
        /// </summary>
        protected virtual async Task<List<TEntity>> GetEntitiesByIds(
            List<TKey> ids,
            ExpandExpression expand,
            SelectExpression select,
            FilterExpression permissionsFilter,
            CancellationToken cancellation)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<TEntity>();
            }
            else
            {
                var data = await GetEntitiesByCustomQuery(q => q.FilterByIds(ids), expand, select, null, permissionsFilter, cancellation);

                // If the data is only 
                if (ids.Count == 1 && data.Count == 1)
                {
                    // No need to sort
                    return data;
                }
                else
                {
                    // Sort the entities according to the original Ids, as a good practice
                    TEntity[] dataSorted = new TEntity[ids.Count];
                    Dictionary<TKey, TEntity> dataDic = data.ToDictionary(e => e.Id);
                    for (int i = 0; i < ids.Count; i++)
                    {
                        var id = ids[i];
                        if (dataDic.TryGetValue(id, out TEntity entity))
                        {
                            dataSorted[i] = entity;
                        }
                    }

                    return dataSorted.Where(e => e != null).ToList();
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the propName, values and the specifications in the <see cref="ExpandExpression"/> and <see cref="SelectExpression"/>,
        /// after verifying the user's READ permissions, returns the entities in the same order as the supplied Ids
        /// </summary>
        public virtual async Task<(List<TEntity>, Extras)> GetByPropertyValues(string propName, IEnumerable<object> values, SelectExpandArguments args, CancellationToken cancellation)
        {
            if (propName is null)
            {
                throw new ArgumentNullException(nameof(propName));
            }

            // Load the data
            List<TEntity> data;
            if (values == null || !values.Any())
            {
                data = new List<TEntity>();
            }
            else
            {
                // Parse the parameters
                var expand = ExpandExpression.Parse(args?.Expand);
                var select = ParseSelect(args?.Select);

                data = await GetEntitiesByCustomQuery(q => q.FilterByPropertyValues(propName, values), expand, select, null, null, cancellation);
            }

            // Load the extras
            var extras = await GetExtras(data, cancellation);

            // Return 
            return (data, extras);
        }

        /// <summary>
        /// Returns an <see cref="List{TEntity}"/> based on a custom filtering function applied to the query, as well as
        /// optional select and expand arguments, checking the user READ permissions along the way
        /// </summary>
        /// <param name="filterFunc">Allows any kind of filtering on the query</param>
        /// <param name="expand">Optional expand argument</param>
        /// <param name="select">Optional select argument</param>
        /// <param name="orderby">Optional select argument</param>
        /// <param name="permissionsFilter">Optional filter argument, if null is passed the q</param>
        /// <param name="cancellation">Optional select argument</param>
        protected async Task<List<TEntity>> GetEntitiesByCustomQuery(
            Func<Query<TEntity>, Query<TEntity>> filterFunc,
            ExpandExpression expand,
            SelectExpression select,
            OrderByExpression orderby,
            FilterExpression permissionsFilter,
            CancellationToken cancellation)
        {
            // Prepare a query of the result, and clone it
            var repo = GetRepository();
            var query = repo.Query<TEntity>();

            // Apply custom filter function
            query = filterFunc(query);

            // Apply read permissions
            permissionsFilter ??= await UserPermissionsFilter(Constants.Read, cancellation);
            query = query.Filter(permissionsFilter);

            // Expand, Select and Order the result as specified in the OData agruments
            var expandedQuery = query.Expand(expand).Select(select).OrderBy(orderby ?? OrderByExpression.Parse("Id")); // Required

            // Load the result into memory
            var data = await expandedQuery.ToListAsync(cancellation); // this is potentially unordered, should that be a concern?

            // TODO: This is slow and unused
            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            // await ApplyReadPermissionsMask(data, query, permissions, GetDefaultMask(), cancellation);

            // Return
            return data;
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse("Id desc");
        }

        ///// <summary>
        ///// If the user is subject to field-level access control, this method hides all the fields
        ///// that the user has no access to and modifies the metadata of the Entities accordingly
        ///// </summary>
        //protected override async Task ApplyReadPermissionsMask(
        //    List<TEntity> resultEntities,
        //    Query<TEntity> query,
        //    IEnumerable<AbstractPermission> permissions,
        //    MaskTree defaultMask,
        //    CancellationToken cancellation)
        //{
        //    bool defaultMaskIsUnrestricted = defaultMask == null || defaultMask.IsUnrestricted;
        //    bool allPermissionMasksAreEmpty = permissions.All(e => string.IsNullOrWhiteSpace(e.Mask));
        //    bool anEmptyCriteriaIsPairedWithEmptyMask =
        //        permissions.Any(e => string.IsNullOrWhiteSpace(e.Mask) && string.IsNullOrWhiteSpace(e.Criteria));

        //    if ((allPermissionMasksAreEmpty || anEmptyCriteriaIsPairedWithEmptyMask) && defaultMaskIsUnrestricted)
        //    {
        //        // Optimization: if all masks are unrestricted, or an empty criteria is paired with an empty mask then we can skip this whole ordeal
        //        return;
        //    }
        //    else
        //    {
        //        // Maps every Entity to its list of masks
        //        var maskedEntities = new Dictionary<Entity, HashSet<string>>();
        //        var unrestrictedEntities = new HashSet<Entity>();

        //        // Marks the Entity and all Entities reachable from it as unrestricted
        //        void MarkUnrestricted(Entity entity, Type entityType)
        //        {
        //            if (entity == null)
        //            {
        //                return;
        //            }

        //            if (maskedEntities.ContainsKey(entity))
        //            {
        //                maskedEntities.Remove(entity);
        //            }

        //            if (!unrestrictedEntities.Contains(entity))
        //            {
        //                unrestrictedEntities.Add(entity);
        //                foreach (var key in entity.EntityMetadata.Keys)
        //                {
        //                    var prop = entityType.GetProperty(key);
        //                    if (prop.PropertyType.IsList())
        //                    {
        //                        // This is a navigation collection, iterate over the rows
        //                        var collection = prop.GetValue(entity);
        //                        if (collection != null)
        //                        {
        //                            var collectionType = prop.PropertyType.CollectionType();
        //                            foreach (var row in collection.Enumerate<Entity>())
        //                            {
        //                                MarkUnrestricted(row, collectionType);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // This is a normal navigation property
        //                        var propValue = prop.GetValue(entity) as Entity;
        //                        var propType = prop.PropertyType;
        //                        MarkUnrestricted(propValue, propType);
        //                    }
        //                }
        //            }
        //        }

        //        // Goes over this entity and every entity reachable from it and marks each one with the accessible fields
        //        void MarkMask(Entity entity, Type entityType, MaskTree mask)
        //        {
        //            if (entity == null)
        //            {
        //                return;
        //            }

        //            if (mask.IsUnrestricted)
        //            {
        //                MarkUnrestricted(entity, entityType);
        //            }
        //            else
        //            {
        //                if (unrestrictedEntities.Contains(entity))
        //                {
        //                    // Nothing to mask in an unrestricted Entity
        //                    return;
        //                }
        //                else
        //                {
        //                    if (!maskedEntities.ContainsKey(entity))
        //                    {
        //                        // All entities will have their basic fields accessible
        //                        var accessibleFields = new HashSet<string>();
        //                        foreach (var basicField in entityType.AlwaysAccessibleFields())
        //                        {
        //                            accessibleFields.Add(basicField.Name);
        //                        }

        //                        maskedEntities[entity] = accessibleFields;
        //                    }

        //                    {
        //                        var accessibleFields = maskedEntities[entity];
        //                        foreach (var requestedField in entity.EntityMetadata.Keys)
        //                        {
        //                            var prop = entityType.GetProperty(requestedField);
        //                            if (mask.ContainsKey(requestedField))
        //                            {
        //                                // If the field is included in the mask, make it accessible
        //                                if (!accessibleFields.Contains(requestedField))
        //                                {
        //                                    accessibleFields.Add(requestedField);
        //                                }

        //                                if (prop.PropertyType.IsList())
        //                                {
        //                                    // This is a navigation collection, iterate over the rows and apply the mask subtree
        //                                    var collection = prop.GetValue(entity);
        //                                    if (collection != null)
        //                                    {
        //                                        var collectionType = prop.PropertyType.CollectionType();
        //                                        foreach (var row in collection.Enumerate<Entity>())
        //                                        {
        //                                            MarkMask(row, collectionType, mask[requestedField]);
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    var foreignKeyNameAtt = prop.GetCustomAttribute<ForeignKeyAttribute>();
        //                                    if (foreignKeyNameAtt != null)
        //                                    {
        //                                        // Make sure if the navigation property is included that its foreign key is included as well
        //                                        var foreignKeyName = foreignKeyNameAtt.Name;
        //                                        if (!string.IsNullOrWhiteSpace(foreignKeyName) && !accessibleFields.Contains(foreignKeyName))
        //                                        {
        //                                            accessibleFields.Add(foreignKeyName);
        //                                        }

        //                                        // Use recursion to update the rest of the tree
        //                                        var propValue = prop.GetValue(entity) as Entity;
        //                                        var propType = prop.PropertyType;
        //                                        MarkMask(propValue, propType, mask[requestedField]);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (permissions.All(e => string.IsNullOrWhiteSpace(e.Criteria)))
        //        {
        //            // Having no criteria is a very common case that can be optimized by skipping the database call
        //            var addDefault = permissions.Any(p => string.IsNullOrWhiteSpace(p.Mask));
        //            var masks = permissions.Select(e => e.Mask).Where(e => !string.IsNullOrWhiteSpace(e));
        //            var maskTrees = masks.Select(mask => MaskTree.Parse(mask)).ToList();
        //            if (addDefault)
        //            {
        //                maskTrees.Add(defaultMask);
        //            }

        //            // Calculate the union of all the mask fields
        //            var maskUnion = maskTrees.Aggregate(MaskTree.BasicFieldsMaskTree(), (t1, t2) => t1.UnionWith(t2));

        //            // Mark all the entities
        //            var entityType = typeof(TEntity);
        //            foreach (var item in resultEntities)
        //            {
        //                MarkMask(item, entityType, maskUnion);
        //            }
        //        }
        //        else
        //        {
        //            // an array of every criteria and every mask
        //            var maskAndCriteriaArray = permissions
        //                .Where(e => !string.IsNullOrWhiteSpace(e.Criteria)) // Optimization: a null criteria is satisfied by the entire list of entities
        //                .GroupBy(e => e.Criteria)
        //                .Select(g => new
        //                {
        //                    Criteria = g.Key,
        //                    Mask = g.Select(e => string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : MaskTree.Parse(e.Mask))
        //                    .Aggregate((t1, t2) => t1.UnionWith(t2)) // takes the union of all the mask trees
        //                }).ToArray();

        //            // This mask applies to every single entity since the criteria is null
        //            var universalMask = permissions
        //                .Where(e => string.IsNullOrWhiteSpace(e.Criteria))
        //                .Distinct()
        //                .Select(e => string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : MaskTree.Parse(e.Mask))
        //                .Aggregate(MaskTree.BasicFieldsMaskTree(), (t1, t2) => t1.UnionWith(t2)); // we use a seed here since if the collection is empty this will throw an error

        //            var criteriaWithIndexes = maskAndCriteriaArray
        //                .Select((e, index) => new IndexAndCriteria { Criteria = e.Criteria, Index = index });

        //            var criteriaMapList = await query.GetIndexToIdMap<TKey>(criteriaWithIndexes, cancellation);

        //            // Go over the Ids in the result and apply all relevant masks to said entity
        //            var entityType = typeof(TEntity);
        //            var criteriaMapDictionary = criteriaMapList
        //                .GroupBy(e => e.Id)
        //                .ToDictionary(e => e.Key, e => e.ToList());

        //            foreach (var entity in resultEntities)
        //            {
        //                var id = entity.Id;
        //                MaskTree mask;

        //                if (criteriaMapDictionary.ContainsKey(id))
        //                {
        //                    // Those are entities that satisfy one or more non-null Criteria
        //                    mask = criteriaMapDictionary[id]
        //                        .Select(e => maskAndCriteriaArray[e.Index].Mask)
        //                        .Aggregate((t1, t2) => t1.UnionWith(t2))
        //                        .UnionWith(universalMask);
        //                }
        //                else
        //                {
        //                    // Those are entities that belong to the universal mask of null criteria
        //                    mask = universalMask;
        //                }

        //                MarkMask(entity, entityType, mask);
        //            }
        //        }

        //        // This where field-level security is applied, we read all masked entities and apply the
        //        // masks on them by setting the field to null and adjusting the metadata accordingly
        //        foreach (var pair in maskedEntities)
        //        {
        //            var entity = pair.Key;
        //            var accessibleFields = pair.Value;

        //            List<Action> updates = new List<Action>(entity.EntityMetadata.Keys.Count);
        //            foreach (var requestedField in entity.EntityMetadata.Keys)
        //            {
        //                if (!accessibleFields.Contains(requestedField))
        //                {
        //                    // Mark the field as restricted (we delay the call to avoid the dreadful "collection-was-modified" Exception)
        //                    updates.Add(() => entity.EntityMetadata[requestedField] = FieldMetadata.Restricted);

        //                    // Set the field to null
        //                    var prop = entity.GetType().GetProperty(requestedField);
        //                    try
        //                    {
        //                        prop.SetValue(entity, null);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        if (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
        //                        {
        //                            // Programmer mistake
        //                            throw new InvalidOperationException($"Entity field {prop.Name} has a non nullable type, all Entity fields must have a nullable type");
        //                        }
        //                        else
        //                        {
        //                            throw ex;
        //                        }
        //                    }
        //                }
        //            }

        //            updates.ForEach(a => a());
        //        }
        //    }
        //}

        /// <summary>
        /// Many actions are simply a list of Ids
        /// This is a utility method for permission actions that do not have a mask, it checks that the
        /// user has enough permissions to cover the entire list of Ids affected by the action, if not it
        /// throws a <see cref="ForbiddenException"/>, unless the uncovered Ids are invisible to the user
        /// or do not exist, in that case it returns a watered down list of Ids the user can perform the
        /// action on. Handling missing and invisible Ids are up to the API implementation
        /// </summary>
        protected virtual async Task<List<TKey>> CheckActionPermissionsBefore(FilterExpression actionFilter, List<TKey> ids)
        {
            if (actionFilter == null)
            {
                return ids; // No row level security
            }
            else
            {
                var actionedIds = ids.Distinct();

                var baseQuery = GetRepository()
                       .Query<TEntity>()
                       .Select("Id")
                       .FilterByIds(actionedIds);

                // First query to count how many Ids the user can action
                var actionableEntities = await baseQuery
                    .Filter(actionFilter)
                    .ToListAsync(cancellation: default);

                if (actionableEntities.Count == actionedIds.Count())
                {
                    return ids; // The user has permission to view and perform the action on all the Ids
                }
                else // Else Potential problem, either the user (1) can't view one or more of the Ids (2) or can't perform the action on said Ids
                {
                    // Do a second query to verify that the missing Ids are solely due to read permission (not action permissions)
                    var readFilter = await UserPermissionsFilter(Constants.Read, cancellation: default);
                    var readableIdsCount = await baseQuery
                        .Filter(readFilter)
                        .CountAsync(cancellation: default);

                    if (actionableEntities.Count < readableIdsCount) // Definitely a problem
                    {
                        // Trying to perform an action on Ids you can see but cannot perform that action onto
                        throw new ForbiddenException();
                    } 
                    else
                    {
                        // Trying to perform an action on Ids that are invisible to you, treat them like you would treat entirely missing Ids
                        // Return the actionable Ids while preserving their order
                        var actionableIdsHash = actionableEntities.Select(e => e.Id).ToHashSet();
                        return ids.Where(id => actionableIdsHash.Contains(id)).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Compliments <see cref="CheckActionPermissionsBefore(FilterExpression, List{TKey})"/> when the user has partial (Row level) access on a table
        /// This utility method checks (after the action has been perfromed) that the permission criteria predicate is still true for all actioned
        /// Ids, otherwise throws a <see cref="ForbiddenException"/> to roll back the transaction (the method must be called before committing the transaction
        /// </summary>
        protected async Task CheckActionPermissionsAfter(FilterExpression actionFilter, List<TKey> actionedIds, List<TEntity> data)
        {
            if (actionFilter != null)
            {
                // How many of those Ids is the user allowed to apply the action to
                int actionableIdsCount;
                if (data != null)
                {
                    // Optimization, if the data is already loaded by the action handler
                    // (with the action permissions filter applied), count that in memory
                    actionableIdsCount = data.Count;
                }
                else
                {
                    // If data is not loaded, a DB request is necessary to count
                    actionableIdsCount = await GetRepository()
                           .Query<TEntity>()
                           .Select("Id")
                           .Filter(actionFilter)
                           .FilterByIds(actionedIds)
                           .CountAsync(cancellation: default);
                }

                // If permitted less than actual => Forbidden
                if (actionableIdsCount < actionedIds.Count)
                {
                    throw new ForbiddenException();
                }
            }
        }

        async Task<(List<EntityWithKey>, Extras)> IFactWithIdService.GetByIds(List<object> ids, SelectExpandArguments args, CancellationToken cancellation)
        {
            var (data, extras) = await GetByIds(ids.Cast<TKey>().ToList(), args, Constants.Read, cancellation);
            var genericData = data.Cast<EntityWithKey>().ToList();

            return (genericData, extras);
        }

        async Task<(List<EntityWithKey>, Extras)> IFactWithIdService.GetByPropertyValues(string propName, IEnumerable<object> values, SelectExpandArguments args, CancellationToken cancellation)
        {
            var (data, extras) = await GetByPropertyValues(propName, values, args, cancellation);
            var genericData = data.Cast<EntityWithKey>().ToList();

            return (genericData, extras);
        }
    }

    public interface IFactWithIdService : IFactServiceBase
    {
        Task<(List<EntityWithKey>, Extras)> GetByIds(List<object> ids, SelectExpandArguments args, CancellationToken cancellation);

        Task<(List<EntityWithKey>, Extras)> GetByPropertyValues(string propName, IEnumerable<object> values, SelectExpandArguments args, CancellationToken cancellation);
    }
}
