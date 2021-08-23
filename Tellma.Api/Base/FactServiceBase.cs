using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;
using Tellma.Model.Common;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using Queryex-style arguments.
    /// </summary>
    public abstract class FactServiceBase<TEntity, TEntitiesResult> : ServiceBase, IFactService 
        where TEntitiesResult : EntitiesResult<TEntity>
        where TEntity : Entity
    {
        #region Constants 

        /// <summary>
        /// The default maximum page size returned by the <see cref="GetFact(GetArguments)"/>,
        /// it can be overridden by overriding <see cref="MaximumPageSize()"/>.
        /// </summary>
        private const int DefaultMaxPageSize = 10000;

        /// <summary>
        /// The maximum number of rows (data points) that can be returned by <see cref="GetAggregate(GetAggregateArguments)"/>, 
        /// if the result is lager the implementation returns a bad request 400.
        /// </summary>
        private const int MaximumAggregateResultSize = 65536;

        /// <summary>
        /// Queries that have a total count of more than this will not be counted since it
        /// impacts performance. <see cref="int.MaxValue"/> is returned instead.
        /// </summary>
        private const int MaximumCount = 10000; // IMPORTANT: Keep in sync with client side

        #endregion

        #region Lifecycle

        private readonly IStringLocalizer _localizer;
        private readonly TemplateService _templateService;
        private readonly MetadataProvider _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactServiceBase{TEntity}"/> class.
        /// </summary>
        public FactServiceBase(FactServiceDependencies deps) : base(deps.ContextAccessor)
        {
            _localizer = deps.Localizer;
            _templateService = deps.TemplateService;
            _metadata = deps.Metadata;
        }

        /// <summary>
        /// Sets the definition Id that scopes the service to only a subset of the definitioned entities.
        /// </summary>
        public FactServiceBase<TEntity, TEntitiesResult> SetDefinitionId(int definitionId)
        {
            DefinitionId = definitionId;
            FactBehavior.SetDefinitionId(definitionId);

            return this;
        }

        #endregion

        #region Behavior

        protected override IServiceBehavior Behavior => FactBehavior;

        /// <summary>
        /// When implemented, returns <see cref="IServiceBehavior"/> that is invoked every 
        /// time <see cref="Initialize()"/> is invoked.
        /// </summary>
        protected abstract IFactServiceBehavior FactBehavior { get; }

        #endregion

        #region API

        /// <summary>
        /// Returns a list of entities and optionally their count as per the specifications in <paramref name="args"/>.
        /// </summary>
        public virtual async Task<TEntitiesResult> GetEntities(GetArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var orderby = ExpressionOrderBy.Parse(args.OrderBy);
            var expand = ExpressionExpand.Parse(args.Expand);
            var select = ParseSelect(args.Select);

            // Prepare the query
            var query = QueryFactory().EntityQuery<TEntity>();

            // Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read, cancellation);
            query = query.Filter(permissionsFilter);

            // Apply search
            query = await Search(query, args, cancellation);

            // Apply filter
            query = query.Filter(filter);

            // Apply orderby
            orderby ??= await DefaultOrderBy(cancellation);
            query = query.OrderBy(orderby);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Top(top);

            // Apply the expand, which has the general format 'Expand=A,B.C,D'
            query = query.Expand(expand);

            // Apply the select, which has the general format 'Select=A,B.C,D'
            query = query.Select(select);

            // Load the data and count in memory
            List<TEntity> data;
            int? count = null;
            if (args.CountEntities)
            {
                var output = await query.ToListAndCountAsync(MaximumCount, QueryContext, cancellation);
                data = output.Entities;
                count = output.Count;
            }
            else
            {
                data = await query.ToListAsync(QueryContext, cancellation);
            }

            // Return
            return await ToEntitiesResult(data, count, cancellation);
        }

        /// <summary>
        /// Returns a list of dynamic rows and optionally their count as per the specifications in <paramref name="args"/>.
        /// </summary>
        public virtual async Task<FactResult> GetFact(FactArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var orderby = ExpressionOrderBy.Parse(args.OrderBy);
            var select = ExpressionFactSelect.Parse(args.Select);

            // Prepare the query
            var query = QueryFactory().FactQuery<TEntity>();

            // Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read, cancellation);
            query = query.Filter(permissionsFilter);

            // Apply filter
            query = query.Filter(filter);

            // Apply orderby
            orderby ??= await DefaultOrderBy(cancellation);
            query = query.OrderBy(orderby);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Top(top);

            // Apply the select
            query = query.Select(select);

            // Load the data and count in memory
            List<DynamicRow> data;
            int? count = null;
            if (args.CountEntities)
            {
                (data, count) = await query.ToListAndCountAsync(MaximumCount, QueryContext, cancellation);
            }
            else
            {
                data = await query.ToListAsync(QueryContext, cancellation);
            }

            // Return
            return new FactResult(data, count);
        }

        /// <summary>
        /// Returns an aggregated list of dynamic rows and any tree dimension ancestors as per the specifications in <paramref name="args"/>.
        /// </summary>
        public virtual async Task<AggregateResult> GetAggregate(GetAggregateArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var having = ExpressionHaving.Parse(args.Having);
            var select = ExpressionAggregateSelect.Parse(args.Select);
            var orderby = ExpressionAggregateOrderBy.Parse(args.OrderBy);

            // Prepare the query
            var query = QueryFactory().AggregateQuery<TEntity>();

            // Retrieve and Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read, cancellation);
            query = query.Filter(permissionsFilter); // Important

            // Filter and Having
            query = query.Filter(filter);
            query = query.Having(having);

            // Apply the top parameter
            var top = args.Top == 0 ? int.MaxValue : args.Top; // 0 means get all
            top = Math.Min(top, MaximumAggregateResultSize + 1);
            query = query.Top(top);

            // Apply the select, which has the general format 'Select=A+B.C,Sum(D)'
            query = query.Select(select);

            // Apply the orderby, which has the general format 'A+B.C desc,Sum(D) asc'
            query = query.OrderBy(orderby);

            // Load the data in memory
            var output = await query.ToListAsync(QueryContext, cancellation);
            var data = output.Rows;
            var ancestors = output.Ancestors.Select(e => new DimensionAncestorsResult(e.Result, e.IdIndex, e.MinIndex));

            // Put a limit on the number of data points returned, to prevent DoS attacks
            if (data.Count > MaximumAggregateResultSize)
            {
                var msg = _localizer["Error_NumberOfDataPointsExceedsMaximum0", MaximumAggregateResultSize];
                throw new ServiceException(msg);
            }

            // Return
            return new AggregateResult(data, ancestors);
        }

        /// <summary>
        /// Returns a generated markup text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The markup generation will implicitly contain a variable $ that evaluates to the results of the query specified in <paramref name="args"/>.
        /// </summary>
        public async Task<FileResult> PrintEntities(int templateId, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) Preloaded Query
            var collection = typeof(TEntity).Name;
            var defId = DefinitionId;

            QueryInfo preloadedQuery;
            if (args.I != null && args.I.Any())
            {
                preloadedQuery = new QueryEntitiesByIdsInfo(
                    collection: collection,
                    definitionId: defId,
                    ids: args.I);
            }
            else
            {
                preloadedQuery = new QueryEntitiesInfo(
                    collection: collection,
                    definitionId: defId,
                    filter: args.Filter,
                    orderby: args.OrderBy,
                    top: args.Top,
                    skip: args.Skip);
            }

            // (2) The templates
            var template = await FactBehavior.GetMarkupTemplate<TEntity>(templateId, cancellation);
            var templates = new (string, string)[] {
                (template.DownloadName, MimeTypes.Text),
                (template.Body, template.MarkupLanguage)
            };

            // (3) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable($"{collection}/{defId}"),
                ["$Filter"] = new EvaluationVariable(args.Filter),
                ["$OrderBy"] = new EvaluationVariable(args.OrderBy),
                ["$Top"] = new EvaluationVariable(args.Top),
                ["$Skip"] = new EvaluationVariable(args.Skip),
                ["$Ids"] = new EvaluationVariable(args.I)
            };

            await FactBehavior.SetMarkupFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetMarkupVariables(localVariables, globalVariables, cancellation);

            // (4) Culture
            CultureInfo culture = GetCulture(args.Culture);

            // Generate the output
            var genArgs = new MarkupArguments(templates, globalFunctions, globalVariables, localFunctions, localVariables, preloadedQuery, culture);
            string[] outputs = await _templateService.GenerateMarkup(genArgs, cancellation);

            var downloadName = outputs[0];
            var body = outputs[1];

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
        /// An optional definition Id for services that are accessing definitioned resources.
        /// <summary/>
        protected int? DefinitionId { get; private set; }

        /// <summary>
        /// Helper property that returns a <see cref="QueryContext"/> based on <see cref="UserId"/> and <see cref="Today"/>.
        /// </summary>
        protected QueryContext QueryContext => new(UserId, Today);

        /// <summary>
        /// Helper function that returns the <see cref="CultureInfo"/> that corresponds
        /// to the given <paramref name="name"/>, or the current UI culture if name was null.
        /// </summary>
        /// <param name="name">The culture name, for example "en".</param>
        /// <exception cref="ServiceException">If <paramref name="name"/> is not null and invalid.</exception>
        protected static CultureInfo GetCulture(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return CultureInfo.CurrentUICulture;
            }

            try
            {
                return new CultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                throw new ServiceException($"Value '{name}' could not be interpreted as a valid culture.");
            }
        }

        /// <summary>
        /// Select argument may get huge and unweildly in certain cases, this method offers a chance
        /// for services to optimize queries by understanding special concise "shorthands" in
        /// the select string that get expanded into a proper select expression.
        /// This way clients don't have to send large select strings in the request for common scenarios.
        /// </summary>
        protected virtual ExpressionSelect ParseSelect(string select)
        {
            return ExpressionSelect.Parse(select);
        }

        /// <summary>
        /// Get the <see cref="IQueryFactory"/> that the <see cref="FactServiceBase{TEntity}"/> can use to query the entities.
        /// </summary>
        protected virtual IQueryFactory QueryFactory() => FactBehavior.QueryFactory<TEntity>();

        /// <summary>
        /// Retrieves the user permissions for the current view and the specified action.
        /// </summary>
        protected virtual async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
            => await FactBehavior.UserPermissions(View, action, cancellation);

        /// <summary>
        /// Returns the view to use when checking user permissions.
        /// </summary>
        protected abstract string View { get; }

        /// <summary>
        /// Implementations create the <see cref="TEntitiesResult"/> to return from all the service
        /// methods that return it.
        /// </summary>
        protected abstract Task<TEntitiesResult> ToEntitiesResult(List<TEntity> data, int? count = null, CancellationToken cancellation = default);

        /// <summary>
        /// Retrieves the user permissions for the given action and parses them in the form of an 
        /// <see cref="ExpressionFilter"/>, throws a <see cref="ForbiddenException"/> if none are found.
        /// </summary>    
        /// <exception cref="ForbiddenException">When the user lacks the needed permissions.</exception>
        protected async Task<ExpressionFilter> UserPermissionsFilter(string action, CancellationToken cancellation)
        {
            // Check if the user has any permissions on View at all, else throw forbidden exception
            // If the user has some permissions on View, OR all their criteria together and return as a FilterExpression
            var permissions = await UserPermissions(action, cancellation);
            if (!permissions.Any())
            {
                // Not even authorized to call this API
                throw new ForbiddenException();
            }
            else if (permissions.Any(e => string.IsNullOrWhiteSpace(e.Criteria)))
            {
                // The user can read the entire data set
                return null;
            }
            else
            {
                // The user has access to part of the data set based on a list
                // of filters that will be ORed together in a dynamic query
                return permissions.Select(e => ExpressionFilter.Parse(e.Criteria))
                        .Aggregate((e1, e2) => ExpressionFilter.Disjunction(e1, e2));
            }
        }

        /// <summary>
        /// Applies the search argument to the <see cref="EntityQuery{T}"/>. This is handled differently in every service.
        /// </summary>
        /// <param name="query">The <see cref="EntityQuery{T}"/> to apply the search argument to.</param>
        /// <param name="args">The <see cref="GetArguments"/> containing the relevant search argument.</param>
        /// <returns>The query with the search argument applied to it.</returns>
        protected abstract Task<EntityQuery<TEntity>> Search(EntityQuery<TEntity> query, GetArguments args, CancellationToken cancellation);

        /// <summary>
        /// Returns the default order by to apply on queries when the orderby parameter is null.
        /// </summary>
        protected abstract Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation);

        /// <summary>
        /// Specifies the maximum page size to be returned by <see cref="GetEntities(GetArguments)"/>. Defaults to <see cref="DefaultMaxPageSize"/>.
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DefaultMaxPageSize;
        }

        /// <summary>
        /// Retrieves the metadata of the entity.
        /// </summary>
        protected async Task<TypeMetadata> GetMetadata(CancellationToken cancellation)
        {
            int? tenantId = TenantId;
            int? definitionId = DefinitionId;
            Type type = typeof(TEntity);
            IMetadataOverridesProvider overrides = await FactBehavior.GetMetadataOverridesProvider(cancellation);

            return _metadata.GetMetadata(tenantId, type, definitionId, overrides);
        }

        #endregion

        #region IFactService

        async Task<EntitiesResult<Entity>> IFactService.GetEntities(GetArguments args, CancellationToken cancellation)
        {
            var result = await GetEntities(args, cancellation);
            var genericData = result.Data.Cast<Entity>().ToList();
            var count = result.Count;

            return new EntitiesResult<Entity>(genericData, count);
        }

        #endregion
    }

    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using Queryex-style arguments.
    /// </summary>
    public abstract class FactServiceBase<TEntity> : FactServiceBase<TEntity, EntitiesResult<TEntity>>
        where TEntity : Entity
    {
        public FactServiceBase(FactServiceDependencies deps) : base(deps)
        {
        }

        protected override Task<EntitiesResult<TEntity>> ToEntitiesResult(List<TEntity> data, int? count = null, CancellationToken cancellation = default)
        {
            var result = new EntitiesResult<TEntity>(data, count);
            return Task.FromResult(result);
        }
    }

    public interface IFactService
    {
        Task<EntitiesResult<Entity>> GetEntities(GetArguments args, CancellationToken cancellation);

        Task<FactResult> GetFact(FactArguments args, CancellationToken cancellation);

        Task<AggregateResult> GetAggregate(GetAggregateArguments args, CancellationToken cancellation);
    }
}
