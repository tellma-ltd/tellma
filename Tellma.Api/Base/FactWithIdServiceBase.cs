using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Api.Templating;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments.
    /// </summary>
    public abstract class FactWithIdServiceBase<TEntity, TKey, TEntitiesResult> : FactServiceBase<TEntity, TEntitiesResult>, IFactWithIdService
        where TEntitiesResult : EntitiesResult<TEntity>
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        private readonly TemplateService _templateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactWithIdServiceBase{TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="deps">The service dependencies.</param>
        public FactWithIdServiceBase(FactServiceDependencies deps) : base(deps)
        {
            _templateService = deps.TemplateService;
        }

        /// <summary>
        /// Sets the definition Id that scopes the service to only a subset of the definitioned entities.
        /// </summary>
        public new FactWithIdServiceBase<TEntity, TKey, TEntitiesResult> SetDefinitionId(int definitionId)
        {
            base.SetDefinitionId(definitionId);
            return this;
        }

        #endregion

        #region API

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Ids and the specifications 
        /// in <paramref name="args"/>, after verifying the user's READ permissions.
        /// </summary>
        public virtual async Task<TEntitiesResult> GetByIds(IList<TKey> ids, SelectExpandArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await GetByIds(ids, args, PermissionActions.Read, cancellation);
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per <paramref name="propName"/>, <paramref name="values"/> 
        /// and the specifications in <paramref name="args"/>, after verifying the user's READ permissions.
        /// </summary>
        public virtual async Task<TEntitiesResult> GetByPropertyValues(string propName, IEnumerable<object> values, SelectExpandArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

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

            // Return 
            return await ToEntitiesResult(data, data.Count, cancellation);
        }

        /// <summary>
        /// Returns a template-generated text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The text generation will implicitly contain a variable $ that evaluates to the results of the query specified in <paramref name="args"/>.
        /// </summary>
        public async Task<FileResult> PrintEntities(int templateId, PrintEntitiesArguments<TKey> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var collection = typeof(TEntity).Name;
            var defId = DefinitionId;

            // (1) The Template Plan
            var template = await FactBehavior.GetPrintingTemplate(templateId, cancellation);

            var nameP = new TemplatePlanLeaf(template.DownloadName, TemplateLanguage.Text);
            var bodyP = new TemplatePlanLeaf(template.Body, TemplateLanguage.Html);
            var printoutP = new TemplatePlanTuple(nameP, bodyP);

            TemplatePlan plan;
            if (string.IsNullOrWhiteSpace(template.Context))
            {
                plan = printoutP;
            }
            else
            {
                plan = new TemplatePlanDefine("$", template.Context, printoutP);
            }

            QueryInfo contextQuery = BaseUtil.EntitiesPreloadedQuery(args, collection, defId);
            plan = new TemplatePlanDefineQuery("$", contextQuery, plan);

            // (2) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = BaseUtil.EntitiesLocalVariables(args, collection, defId);

            await FactBehavior.SetPrintingFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetPrintingVariables(localVariables, globalVariables, cancellation);

            // (3)  Generate the output
            CultureInfo culture = GetCulture(args.Culture);
            var genArgs = new TemplateArguments(globalFunctions, globalVariables, localFunctions, localVariables, culture);
            await _templateService.GenerateFromPlan(plan: plan, args: genArgs, cancellation: cancellation);

            var downloadName = nameP.Outputs[0];
            var body = bodyP.Outputs[0];

            // Change the body to bytes
            var bodyBytes = Encoding.UTF8.GetBytes(body);

            // Use a default download name if none is provided
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                var meta = await GetMetadata(cancellation);
                var titlePlural = meta.PluralDisplay();
                if (args.I != null && args.I.Count > 0)
                {
                    downloadName = $"{titlePlural} ({args.I.Count})";
                }
                else
                {
                    int from = args.Skip + 1;
                    int to = Math.Max(from, args.Skip + args.Top);
                    downloadName = $"{titlePlural} {from}-{to}";
                }
            }

            if (!downloadName.ToLower().EndsWith(".html"))
            {
                downloadName += ".html";
            }

            // Return as a file
            return new FileResult(bodyBytes, downloadName);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Ids and the specifications 
        /// in <paramref name="args"/>, after verifying the user's permissions.
        /// </summary>
        /// <remarks>
        /// This function does not call <see cref="ServiceBase.Initialize"/>. That is the responsibility of the caller.
        /// </remarks>
        protected virtual async Task<TEntitiesResult> GetByIds(IList<TKey> ids, SelectExpandArguments args, string action, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var expand = ExpressionExpand.Parse(args?.Expand);
            var select = ParseSelect(args?.Select);

            // Prepare the permissions filter
            var permissionsFilter = await UserPermissionsFilter(action, cancellation);

            // Load the data
            var data = await GetEntitiesByIds(ids, expand, select, permissionsFilter, cancellation);

            // Return result
            return await ToEntitiesResult(data, data.Count, cancellation);
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the <paramref name="ids"/> and the specifications in <paramref name="expand"/> and <paramref name="select"/>,
        /// after verifying the user's permissions, returns the entities in the same order as the supplied Ids.<br/>
        /// If null was supplied for <paramref name="permissionsFilter"/>, the function by default uses the read permissions filter of the current user.
        /// </summary>
        protected virtual async Task<List<TEntity>> GetEntitiesByIds(
            IList<TKey> ids,
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

            // Expand, Select and Order the result as specified in the Queryex agruments
            var expandedQuery = query.Expand(expand).Select(select).OrderBy(orderby ?? ExpressionOrderBy.Parse("Id")); // Required

            // Load the result into memory
            var data = await expandedQuery.ToListAsync(QueryContext(), cancellation); // this is potentially unordered, should that be a concern?

            // Return
            return data;
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            return Task.FromResult(ExpressionOrderBy.Parse("Id desc"));
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
                    .ToListAsync(QueryContext(), cancellation: default);

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
                        .CountAsync(QueryContext());

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
        /// <remarks>
        /// This function can handle null <paramref name="data"/>.
        /// </remarks>
        /// <exception cref="ForbiddenException"></exception>
        protected async Task CheckActionPermissionsAfter(ExpressionFilter actionFilter, List<TKey> actionedIds, IReadOnlyList<TEntity> data)
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
                           .CountAsync(QueryContext());
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

        async Task<EntitiesResult<EntityWithKey>> IFactWithIdService.GetByIds(IList ids, SelectExpandArguments args, CancellationToken cancellation)
        {
            var result = await GetByIds(ids.Cast<TKey>().ToList(), args, cancellation);
            var genericData = result.Data.Cast<EntityWithKey>().ToList();
            var count = result.Count;

            return new EntitiesResult<EntityWithKey>(genericData, count);
        }

        async Task<EntitiesResult<EntityWithKey>> IFactWithIdService.GetByPropertyValues(string propName, IEnumerable<object> values, SelectExpandArguments args, CancellationToken cancellation)
        {
            var result = await GetByPropertyValues(propName, values, args, cancellation);
            var genericData = result.Data.Cast<EntityWithKey>().ToList();
            var count = result.Count;

            return new EntitiesResult<EntityWithKey>(genericData, count);
        }

        #endregion
    }

    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments.
    /// </summary>
    public abstract class FactWithIdServiceBase<TEntity, TKey> : FactWithIdServiceBase<TEntity, TKey, EntitiesResult<TEntity>>
        where TEntity : EntityWithKey<TKey>
    {
        public FactWithIdServiceBase(FactServiceDependencies deps) : base(deps)
        {
        }

        protected override Task<EntitiesResult<TEntity>> ToEntitiesResult(List<TEntity> data, int? count = null, CancellationToken cancellation = default)
        {
            var result = new EntitiesResult<TEntity>(data, count);
            return Task.FromResult(result);
        }
    }

    public interface IFactWithIdService : IFactService
    {
        Task<EntitiesResult<EntityWithKey>> GetByIds(IList ids, SelectExpandArguments args, CancellationToken cancellation);

        Task<EntitiesResult<EntityWithKey>> GetByPropertyValues(string propName, IEnumerable<object> values, SelectExpandArguments args, CancellationToken cancellation);
    }
}
