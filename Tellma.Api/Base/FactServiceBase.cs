using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
    /// entity type using OData-like parameters.
    /// </summary>
    public abstract class FactServiceBase<TEntity> : ServiceBase, IFactService
        where TEntity : Entity
    {
        #region Constants 

        /// <summary>
        /// The default maximum page size returned by the <see cref="GetFact(GetArguments)"/>,
        /// it can be overridden by overriding <see cref="MaximumPageSize()"/>.
        /// </summary>
        private const int DEFAULT_MAX_PAGE_SIZE = 10000;

        /// <summary>
        /// The maximum number of rows (data points) that can be returned by <see cref="GetAggregate(GetAggregateArguments)"/>, 
        /// if the result is lager the implementation returns a bad request 400.
        /// </summary>
        private const int MAXIMUM_AGGREGATE_RESULT_SIZE = 65536;

        /// <summary>
        /// Queries that have a total count of more than this will not be counted since it
        /// impacts performance. <see cref="int.MaxValue"/> is returned instead.
        /// </summary>
        private const int MAXIMUM_COUNT = 10000; // IMPORTANT: Keep in sync with client side

        #endregion

        #region Lifecycle

        protected readonly IStringLocalizer _localizer;
        protected readonly TemplateService _templateService;
        protected readonly MetadataProvider _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactServiceBase{TEntity}"/> class.
        /// </summary>
        public FactServiceBase(ServiceDependencies deps, IServiceContextAccessor contextAccessor) : base(contextAccessor)
        {
            _localizer = deps.Localizer;
            _templateService = deps.TemplateService;
            _metadata = deps.Metadata;
        }

        /// <summary>
        /// Sets the definition Id that scopes the service to only a subset of the definitioned entities.
        /// </summary>
        public void SetDefinitionId(int definitionId)
        {
            DefinitionId = definitionId;
            FactBehavior.SetDefinitionId(definitionId);
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
        public virtual async Task<(List<TEntity> data, Extras extras, int? count)> GetEntities(GetArguments args)
        {
            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var orderby = ExpressionOrderBy.Parse(args.OrderBy);
            var expand = ExpressionExpand.Parse(args.Expand);
            var select = ParseSelect(args.Select);

            // Prepare the query
            var query = QueryFactory().EntityQuery<TEntity>();

            // Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read);
            query = query.Filter(permissionsFilter);

            // Apply search
            query = Search(query, args);

            // Apply filter
            query = query.Filter(filter);

            // Apply orderby
            orderby ??= DefaultOrderBy();
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
                (data, count) = await query.ToListAndCountAsync(MAXIMUM_COUNT, QueryContext, Cancellation);
            }
            else
            {
                data = await query.ToListAsync(QueryContext, Cancellation);
            }

            // Load any extra data that are service-specific
            var extras = await GetExtras(data);

            // Return
            return (data, extras, count);
        }

        /// <summary>
        /// Returns a list of dynamic rows and optionally their count as per the specifications in <paramref name="args"/>.
        /// </summary>
        public virtual async Task<(IEnumerable<DynamicRow> data, int? count)> GetFact(GetArguments args)
        {
            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var orderby = ExpressionOrderBy.Parse(args.OrderBy);
            var select = ExpressionFactSelect.Parse(args.Select);

            // Prepare the query
            var query = QueryFactory().FactQuery<TEntity>();

            // Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read);
            query = query.Filter(permissionsFilter);

            // Apply filter
            query = query.Filter(filter);

            // Apply orderby
            orderby ??= DefaultOrderBy();
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
                (data, count) = await query.ToListAndCountAsync(MAXIMUM_COUNT, QueryContext, Cancellation);
            }
            else
            {
                data = await query.ToListAsync(QueryContext, Cancellation);
            }

            // Return
            return (data, count);
        }

        /// <summary>
        /// Returns an aggregated list of dynamic rows and any tree dimension ancestors as per the specifications in <paramref name="args"/>.
        /// </summary>
        public virtual async Task<(List<DynamicRow> data, IEnumerable<DimensionAncestorsResult> ancestors)> GetAggregate(GetAggregateArguments args)
        {
            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var having = ExpressionHaving.Parse(args.Having);
            var select = ExpressionAggregateSelect.Parse(args.Select);
            var orderby = ExpressionAggregateOrderBy.Parse(args.OrderBy);

            // Prepare the query
            var query = QueryFactory().AggregateQuery<TEntity>();

            // Retrieve and Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read);
            query = query.Filter(permissionsFilter); // Important

            // Filter and Having
            query = query.Filter(filter);
            query = query.Having(having);

            // Apply the top parameter
            var top = args.Top == 0 ? int.MaxValue : args.Top; // 0 means get all
            top = Math.Min(top, MAXIMUM_AGGREGATE_RESULT_SIZE + 1);
            query = query.Top(top);

            // Apply the select, which has the general format 'Select=A+B.C,Sum(D)'
            query = query.Select(select);

            // Apply the orderby, which has the general format 'A+B.C desc,Sum(D) asc'
            query = query.OrderBy(orderby);

            // Load the data in memory
            var (data, ancestors) = await query.ToListAsync(QueryContext, Cancellation);

            // Put a limit on the number of data points returned, to prevent DoS attacks
            if (data.Count > MAXIMUM_AGGREGATE_RESULT_SIZE)
            {
                var msg = _localizer["Error_NumberOfDataPointsExceedsMaximum0", MAXIMUM_AGGREGATE_RESULT_SIZE];
                throw new ServiceException(msg);
            }

            // Return
            return (data, ancestors);
        }

        /// <summary>
        /// Returns a generated markup text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The markup generation will implicitly contain a variable $ that is build from the query arguments in <paramref name="args"/>.
        /// </summary>
        public async Task<(byte[] fileBytes, string fileName)> PrintByFilter(int templateId, PrintEntitiesArguments<int> args)
        {
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
            var template = await FactBehavior.GetMarkupTemplate<TEntity>(templateId);
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

            await FactBehavior.SetMarkupFunctions(localFunctions, globalFunctions);
            await FactBehavior.SetMarkupVariables(localVariables, globalVariables);            

            // (4) Culture
            CultureInfo culture = GetCulture(args.Culture);

            // Generate the output
            var genArgs = new GenerateMarkupArguments(templates, globalFunctions, globalVariables, localFunctions, localVariables, preloadedQuery, culture);
            string[] outputs = await _templateService.GenerateMarkup(genArgs, Cancellation);

            var downloadName = outputs[0];
            var body = outputs[1];

            // Change the body to bytes
            var bodyBytes = Encoding.UTF8.GetBytes(body);

            // Use a default download name if none is provided
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                var meta = GetMetadata();
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
            return (bodyBytes, downloadName);
        }

        #endregion

        #region Helper Functions

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
        protected CultureInfo GetCulture(string name)
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
                throw new ServiceException($"The culture code '{name}' was not found.");
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
        protected abstract Task<IEnumerable<AbstractPermission>> UserPermissions(string action);

        /// <summary>
        /// Retrieves the user permissions for the given action and parses them in the form of an 
        /// <see cref="ExpressionFilter"/>, throws a <see cref="ForbiddenException"/> if none are found.
        /// </summary>    
        /// <exception cref="ForbiddenException">When the user lacks the needed permissions.</exception>
        protected async Task<ExpressionFilter> UserPermissionsFilter(string action)
        {
            // Check if the user has any permissions on View at all, else throw forbidden exception
            // If the user has some permissions on View, OR all their criteria together and return as a FilterExpression
            var permissions = await UserPermissions(action);
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
        protected abstract EntityQuery<TEntity> Search(EntityQuery<TEntity> query, GetArguments args);

        /// <summary>
        /// Returns the default order by to apply on queries when the orderby parameter is null.
        /// </summary>
        protected abstract ExpressionOrderBy DefaultOrderBy();

        /// <summary>
        /// Specifies the maximum page size to be returned by <see cref="GetEntities(GetArguments)"/>. Defaults to <see cref="DEFAULT_MAX_PAGE_SIZE"/>.
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DEFAULT_MAX_PAGE_SIZE;
        }

        /// <summary>
        /// Gives services the chance to include custom data with <see cref="GetEntities(GetArguments)"/> responses.
        /// </summary>
        /// <param name="result">The entities to be returned by <see cref="GetEntities(GetArguments)"/>.</param>
        /// <returns>A dictionary containing any extra information to be returned together with the entities.</returns>
        protected virtual Task<Extras> GetExtras(IEnumerable<TEntity> result)
        {
            return Task.FromResult<Extras>(null);
        }

        /// <summary>
        /// Retrieves the metadata of the entity.
        /// </summary>
        protected TypeMetadata GetMetadata()
        {
            int? tenantId = TenantId;
            int? definitionId = DefinitionId;
            Type type = typeof(TEntity);

            return _metadata.GetMetadata(tenantId, type, definitionId);
        }

        #endregion

        #region IFactService Implementation

        async Task<(List<Entity> Data, Extras Extras, int? Count)> IFactService.GetEntities(GetArguments args)
        {
            var (data, extras, count) = await GetEntities(args);
            var genericData = data.Cast<Entity>().ToList();

            return (genericData, extras, count);
        }

        Task<(List<DynamicRow> Data, IEnumerable<DimensionAncestorsResult> Ancestors)> IFactService.GetAggregate(GetAggregateArguments args)
        {
            return GetAggregate(args);
        }

        #endregion
    }

    public interface IFactService
    {
        Task<(List<Entity> Data, Extras Extras, int? Count)> GetEntities(GetArguments args);

        Task<(List<DynamicRow> Data, IEnumerable<DimensionAncestorsResult> Ancestors)> GetAggregate(GetAggregateArguments args);
    }
}
