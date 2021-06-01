using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters.
    /// </summary>
    public abstract class FactWithIdServiceBase<TEntity, TKey> : FactServiceBase<TEntity>, IFactWithIdService
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        /// <summary>
        /// Initializes a new instance of the <see cref="FactWithIdServiceBase{TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="deps">The service dependencies.</param>
        public FactWithIdServiceBase(ServiceDependencies deps) : base(deps)
        {
        }

        #endregion

        #region API

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Ids and the specifications 
        /// in <paramref name="args"/>, after verifying the user's permissions.
        /// </summary>
        public virtual async Task<(List<TEntity>, Extras)> GetByIds(List<TKey> ids, SelectExpandArguments args, string action, CancellationToken cancellation)
        {
            // Parse the parameters
            var expand = ExpressionExpand.Parse(args?.Expand);
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
        /// Returns a <see cref="List{TEntity}"/> as per <paramref name="propName"/>, <paramref name="values"/> 
        /// and the specifications in <paramref name="args"/>, after verifying the user's READ permissions.
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
                var expand = ExpressionExpand.Parse(args?.Expand);
                var select = ParseSelect(args?.Select);

                data = await GetEntitiesByCustomQuery(q => q.FilterByPropertyValues(propName, values), expand, select, null, null, cancellation);
            }

            // Load the extras
            var extras = await GetExtras(data, cancellation);

            // Return 
            return (data, extras);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the <paramref name="ids"/> and the specifications in <paramref name="expand"/> and <paramref name="select"/>,
        /// after verifying the user's permissions, returns the entities in the same order as the supplied Ids.<br/>
        /// If null was supplied for <paramref name="permissionsFilter"/>, the function by default uses the read permissions filter of the current user.
        /// </summary>
        protected virtual async Task<List<TEntity>> GetEntitiesByIds(
            List<TKey> ids,
            ExpressionExpand expand,
            ExpressionSelect select,
            ExpressionFilter permissionsFilter, 
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
        /// Returns an <see cref="List{TEntity}"/> based on a custom <paramref name="filterFunc"/> applied to the query, as well as
        /// optional <paramref name="expand"/> and <paramref name="select"/> arguments, checking the user's READ permissions along the way.
        /// </summary>
        /// <param name="filterFunc">Allows any kind of filtering on the query</param>
        /// <param name="expand">Optional expand argument.</param>
        /// <param name="select">Optional select argument.</param>
        /// <param name="orderby">Optional orderby argument.</param>
        /// <param name="permissionsFilter">Optional filter argument, if null is passed the query uses the read permissions filter of the current user.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        protected async Task<List<TEntity>> GetEntitiesByCustomQuery(
            Func<EntityQuery<TEntity>, EntityQuery<TEntity>> filterFunc,
            ExpressionExpand expand,
            ExpressionSelect select,
            ExpressionOrderBy orderby,
            ExpressionFilter permissionsFilter, 
            CancellationToken cancellation)
        {
            // Prepare a query of the result, and clone it
            var factory = QueryFactory();
            var query = factory.EntityQuery<TEntity>();

            // Apply custom filter function
            query = filterFunc(query);

            // Apply read permissions
            permissionsFilter ??= await UserPermissionsFilter(PermissionActions.Read, cancellation);
            query = query.Filter(permissionsFilter);

            // Expand, Select and Order the result as specified in the OData agruments
            var expandedQuery = query.Expand(expand).Select(select).OrderBy(orderby ?? ExpressionOrderBy.Parse("Id")); // Required

            // Load the result into memory
            var data = await expandedQuery.ToListAsync(QueryContext, cancellation); // this is potentially unordered, should that be a concern?

            // Return
            return data;
        }

        protected override ExpressionOrderBy DefaultOrderBy()
        {
            return ExpressionOrderBy.Parse("Id desc");
        }

        /// <summary>
        /// Many actions take as a parameter a simple list of Ids. This is a utility method for permission 
        /// actions, it checks that the user has enough permissions to cover the entire list of Ids affected 
        /// by the action, if not it throws a <see cref="ForbiddenException"/>, unless the uncovered Ids are 
        /// invisible to the user or do not exist, in that case it returns a watered down list of Ids the user 
        /// can perform the action on. Handling missing and invisible Ids are up to the API implementation.
        /// </summary>
        protected virtual async Task<List<TKey>> CheckActionPermissionsBefore(ExpressionFilter actionFilter, List<TKey> ids)
        {
            if (actionFilter == null)
            {
                return ids; // No row level security
            }
            else
            {
                var actionedIds = ids.Distinct();

                var baseQuery = QueryFactory()
                       .EntityQuery<TEntity>()
                       .Select("Id")
                       .FilterByIds(actionedIds);

                // First query to count how many Ids the user can action
                var actionableEntities = await baseQuery
                    .Filter(actionFilter)
                    .ToListAsync(QueryContext, cancellation: default);

                if (actionableEntities.Count == actionedIds.Count())
                {
                    return ids; // The user has permission to view and perform the action on all the Ids
                }
                else // Else Potential problem, either the user (1) can't view one or more of the Ids (2) or can't perform the action on said Ids
                {
                    // Do a second query to verify that the missing Ids are solely due to read permission (not action permissions)
                    var readFilter = await UserPermissionsFilter(PermissionActions.Read, CancellationToken.None);
                    var readableIdsCount = await baseQuery
                        .Filter(readFilter)
                        .CountAsync(QueryContext);

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
        /// Compliments <see cref="CheckActionPermissionsBefore(ExpressionFilter, List{TKey})"/> when the user
        /// has partial (Row level) access on a table. This utility method checks (after the action has been 
        /// perfromed) that the permission criteria predicate is still true for all actioned Ids, otherwise throws
        /// a <see cref="ForbiddenException"/> to roll back the transaction (the method must be called before 
        /// committing the transaction).
        /// </summary>
        protected async Task CheckActionPermissionsAfter(ExpressionFilter actionFilter, List<TKey> actionedIds, List<TEntity> data)
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
                    actionableIdsCount = await QueryFactory()
                           .EntityQuery<TEntity>()
                           .Select("Id")
                           .Filter(actionFilter)
                           .FilterByIds(actionedIds)
                           .CountAsync(QueryContext);
                }

                // If permitted less than actual => Forbidden
                if (actionableIdsCount < actionedIds.Count)
                {
                    throw new ForbiddenException();
                }
            }
        }

        #endregion

        #region IFactWithIdService

        async Task<(List<EntityWithKey>, Extras)> IFactWithIdService.GetByIds(List<object> ids, SelectExpandArguments args, CancellationToken cancellation)
        {
            var (data, extras) = await GetByIds(ids.Cast<TKey>().ToList(), args, PermissionActions.Read, cancellation);
            var genericData = data.Cast<EntityWithKey>().ToList();

            return (genericData, extras);
        }

        async Task<(List<EntityWithKey>, Extras)> IFactWithIdService.GetByPropertyValues(string propName, IEnumerable<object> values, SelectExpandArguments args, CancellationToken cancellation)
        {
            var (data, extras) = await GetByPropertyValues(propName, values, args, cancellation);
            var genericData = data.Cast<EntityWithKey>().ToList();

            return (genericData, extras);
        }

        #endregion
    }

    public interface IFactWithIdService : IFactService
    {
        Task<(List<EntityWithKey>, Extras)> GetByIds(List<object> ids, SelectExpandArguments args, CancellationToken cancellation);

        Task<(List<EntityWithKey>, Extras)> GetByPropertyValues(string propName, IEnumerable<object> values, SelectExpandArguments args, CancellationToken cancellation);
    }
}
