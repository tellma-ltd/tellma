using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;

namespace Tellma.Api
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using OData-like parameters.
    /// </summary>
    public abstract class FactServiceBase<TEntity> : ServiceBase, IFactServiceBase
        where TEntity : Entity
    {
        protected readonly IStringLocalizer _localizer;
        protected readonly IInstrumentationService _instrumentation;
        protected readonly TemplateService _templateService;
        protected readonly ITenantInfoAccessor _tenantInfo;
        protected readonly ITenantIdAccessor _tenantIdAccessor;
        protected readonly MetadataProvider _metadata;

        protected virtual int? DefinitionId => null;

        public FactServiceBase(IServiceProvider sp)
        {
            _localizer = sp.GetRequiredService<IStringLocalizer<Strings>>();
            _instrumentation = sp.GetRequiredService<IInstrumentationService>();
            _tenantInfo = sp.GetRequiredService<ITenantInfoAccessor>();
            _templateService = sp.GetRequiredService<TemplateService>();
            _tenantIdAccessor = sp.GetRequiredService<ITenantIdAccessor>();
            _metadata = sp.GetRequiredService<MetadataProvider>();
        }

        /// <summary>
        /// The default maximum page size returned by the <see cref="GetFact(GetArguments)"/>,
        /// it can be overridden by overriding <see cref="MaximumPageSize()"/>
        /// </summary>
        private static int DEFAULT_MAX_PAGE_SIZE => 10000;

        /// <summary>
        /// The maximum number of rows (data points) that can be returned by <see cref="GetAggregate(GetAggregateArguments)"/>, 
        /// if the result is lager the implementation returns a bad request 400
        /// </summary>
        private static int MAXIMUM_AGGREGATE_RESULT_SIZE => 65536;

        /// <summary>
        /// Queries that have a total count of more than this will not be counted since it
        /// impacts performance. <see cref="int.MaxValue"/> is returned instead
        /// </summary>
        private static int MAXIMUM_COUNT => 10000; // IMPORTANT: Keep in sync with client side

        /// <summary>
        /// Returns the <see cref="GetResponse{TEntity}"/> as per the specifications in the <see cref="GetArguments"/>
        /// </summary>
        public virtual async Task<(List<TEntity> data, Extras extras, bool isPartial, int? count)> GetEntities(GetArguments args, CancellationToken cancellation)
        {
            try
            {
                // Parse the parameters
                var filter = ExpressionFilter.Parse(args.Filter);
                var orderby = ExpressionOrderBy.Parse(args.OrderBy);
                var expand = ExpressionExpand.Parse(args.Expand);
                var select = ParseSelect(args.Select);

                // Prepare the query
                var query = GetRepository().Query<TEntity>();

                // Apply read permissions
                var permissionsFilter = await UserPermissionsFilter(Constants.Read, cancellation);
                query = query.Filter(permissionsFilter);
                bool isPartial = false;

                // Apply search
                query = Search(query, args);

                // Apply filter
                query = query.Filter(filter);

                // Apply orderby
                query = OrderBy(query, orderby);

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
                    (data, count) = await query.ToListAndCountAsync(MAXIMUM_COUNT, cancellation);
                }
                else
                {
                    data = await query.ToListAsync(cancellation);
                }

                var extras = await GetExtras(data, cancellation);

                // Return
                return (data, extras, isPartial, count);
            }
            catch (QueryException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        /// <summary>
        /// Returns the <see cref="GetResponse{TEntity}"/> as per the specifications in the <see cref="GetArguments"/>
        /// </summary>
        public virtual async Task<(IEnumerable<DynamicRow> data, bool isPartial, int? count)> GetFact(GetArguments args, CancellationToken cancellation)
        {
            try
            {
                // Parse the parameters
                var filter = ExpressionFilter.Parse(args.Filter);
                var orderby = ExpressionOrderBy.Parse(args.OrderBy);
                var select = ExpressionFactSelect.Parse(args.Select);

                // Prepare the query
                var query = GetRepository().FactQuery<TEntity>();

                // Apply read permissions
                var permissionsFilter = await UserPermissionsFilter(Constants.Read, cancellation);
                query = query.Filter(permissionsFilter);
                bool isPartial = false;

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
                    (data, count) = await query.ToListAndCountAsync(MAXIMUM_COUNT, cancellation);
                }
                else
                {
                    data = await query.ToListAsync(cancellation);
                }

                // Return
                return (data, isPartial, count);
            }
            catch (QueryException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        /// <summary>
        /// Returns a <see cref="List{DynamicEntity}"/> as per the specifications in the <see cref="GetAggregateArguments"/>,
        /// </summary>
        public virtual async Task<(List<DynamicRow> Data, IEnumerable<TreeDimensionResult> Ancestors, bool IsPartial)> GetAggregate(GetAggregateArguments args, CancellationToken cancellation)
        {
            try
            {
                // Parse the parameters
                var filter = ExpressionFilter.Parse(args.Filter);
                var having = ExpressionHaving.Parse(args.Having);
                var select = ExpressionAggregateSelect.Parse(args.Select);
                var orderby = ExpressionAggregateOrderBy.Parse(args.OrderBy);

                // Prepare the query
                var query = GetRepository().AggregateQuery<TEntity>();

                // Retrieve and Apply read permissions
                var permissionsFilterExp = await UserPermissionsFilter(Constants.Read, cancellation);
                query = query.Filter(permissionsFilterExp); // Important
                var isPartial = false;

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
                var (data, ancestors) = await query.ToListAsync(cancellation);

                // Put a limit on the number of data points returned, to prevent DoS attacks
                if (data.Count > MAXIMUM_AGGREGATE_RESULT_SIZE)
                {
                    var msg = _localizer["Error_NumberOfDataPointsExceedsMaximum0", MAXIMUM_AGGREGATE_RESULT_SIZE];
                    throw new BadRequestException(msg);
                }

                // Return
                return (data, ancestors, isPartial);
            }
            catch (QueryException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        public async Task<(byte[] FileBytes, string FileName)> PrintByFilter([FromRoute] int templateId, [FromQuery] GenerateMarkupByFilterArguments<int> args, CancellationToken cancellation)
        {
            var collection = ControllerUtilities.GetCollectionName(typeof(TEntity));
            var defId = DefinitionId;
            var repo = GetRepository();

            var template = await repo.Query<MarkupTemplate>().FilterByIds(new int[] { templateId }).FirstOrDefaultAsync(cancellation);
            if (template == null)
            {
                // Shouldn't happen in theory cause of previous check, but just to be extra safe
                throw new BadRequestException($"The template with Id {templateId} does not exist");
            }

            if (!(template.IsDeployed ?? false))
            {
                // A proper UI will only allow the user to use supported template
                throw new BadRequestException($"The template with Id {templateId} is not deployed");
            }

            // The errors below should be prevented through SQL validation, but just to be safe
            if (template.Usage != MarkupTemplateConst.QueryByFilter)
            {
                throw new BadRequestException($"The template with Id {templateId} does not have the proper usage");
            }

            if (template.MarkupLanguage != MimeTypes.Html)
            {
                throw new BadRequestException($"The template with Id {templateId} is not an HTML template");
            }

            if (template.Collection != collection)
            {
                throw new BadRequestException($"The template with Id {templateId} does not have Collection = '{collection}'");
            }

            if (template.DefinitionId != null && template.DefinitionId != defId)
            {
                throw new BadRequestException($"The template with Id {templateId} has an incompatible DefinitionId = '{defId}'");
            }

            // Onto the printing itself
            var templates = new (string, string)[] {
                (template.DownloadName, MimeTypes.Text),
                (template.Body, template.MarkupLanguage)
            };

            var tenantInfo = _tenantInfo.GetCurrentInfo();
            var culture = TemplateUtil.GetCulture(args, tenantInfo);

            var preloadedQuery = new QueryByFilterInfo(collection, defId, args.Filter, args.OrderBy, args.Top, args.Skip, args.I);
            var inputVariables = new Dictionary<string, object>
            {
                ["$Source"] = $"{collection}/{defId}",
                ["$Filter"] = args.Filter,
                ["$OrderBy"] = args.OrderBy,
                ["$Top"] = args.Top,
                ["$Skip"] = args.Skip,
                ["$Ids"] = args.I
            };

            // Generate the output
            string[] outputs;
            try
            {
                outputs = await _templateService.GenerateMarkup(templates, inputVariables, preloadedQuery, culture, cancellation);
            }
            catch (TemplateException ex)
            {
                throw new BadRequestException(ex.Message);
            }

            var downloadName = outputs[0];
            var body = outputs[1];

            // Change the body to bytes
            var bodyBytes = Encoding.UTF8.GetBytes(body);

            // Do some sanitization of the downloadName
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


        /// <summary>
        /// Select argument may get huge and unweildly in certain cases, this method offers a chance
        /// for controllers to optimize queries by understanding special concise "shorthands" in
        /// the select string that get expanded into a proper select expression on the server.
        /// This way clients don't have to send large select string in the request for common scenarios
        /// </summary>
        protected virtual ExpressionSelect ParseSelect(string select)
        {
            return ExpressionSelect.Parse(select);
        }

        /// <summary>
        /// Get the DbContext source on which the controller is based
        /// </summary>
        /// <returns></returns>
        protected abstract IRepository GetRepository();

        /// <summary>
        /// Retrieves the user permissions for the current view and the specified action
        /// </summary>
        protected abstract Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation);

        /// <summary>
        /// Retrieves the user permissions for the given action and parses them in the form of an <see cref="ExpressionFilter"/>, throws a <see cref="ForbiddenException"/> if none are found.
        /// </summary>        
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
                // of filters that will  be ORed together in a dynamic query
                return permissions.Select(e => ExpressionFilter.Parse(e.Criteria))
                        .Aggregate((e1, e2) => ExpressionFilter.Disjunction(e1, e2));
            }
        }

        ///// <summary>
        ///// If the user has no permission masks defined (can see all), this mask is used.
        ///// This makes it easier to setup permissions such that a user cannot see employee
        ///// salaries for example, since without this, the user can "expand" the Entity tree 
        ///// all the way to the salaries if she has access to any Entity from which the employee entity is reachable
        ///// </summary>
        ///// <returns></returns>
        //protected virtual MaskTree GetDefaultMask()
        //{
        //    // TODO implement
        //    return new MaskTree();
        //}

        ///// <summary>
        ///// Removes from the permissions all permissions that would be violated by the filter or order by, the behavior
        ///// of the system here is that when a user orders by a field that she has no full access too, she only sees the
        ///// rows where she can see that field, sometimes resulting in a shorter list, this is to prevent the user gaining
        ///// any insight over fields she has no access to by filter or order the data
        ///// </summary>
        //protected IEnumerable<AbstractPermission> FilterViolatedPermissionsForFlatQuery(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, FilterExpression filter, OrderByExpression orderby)
        //{
        //    // Step 1 - Build the "User Mask", i.e the mask containing the fields mentioned in the relevant components of the user query
        //    var userMask = MaskTree.BasicFieldsMaskTree();
        //    userMask = UpdateUserMaskAsPerFilter(filter, userMask);
        //    userMask = UpdateUserMaskAsPerOrderBy(orderby, userMask);

        //    // Filter out those permissions whose mask does not cover the entire user mask
        //    return FilterViolatedPermissionsInner(permissions, defaultMask, userMask);
        //}

        ///// <summary>
        ///// Removes from the permissions all permissions that would be violated by the filter or aggregate select, the behavior
        ///// of the system here is that when a user orders by a field that she has no full access too, she only sees the
        ///// rows where she can see that field, sometimes resulting in a shorter list, this is to prevent the user gaining
        ///// any insight over fields she has no access to by filter or order the data
        ///// </summary>
        //protected IEnumerable<AbstractPermission> FilterViolatedPermissionsForAggregateQuery(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, FilterExpression filter, AggregateSelectExpression select)
        //{
        //    // Step 1 - Build the "User Mask", i.e the mask containing the fields mentioned in the relevant components of the user query
        //    var userMask = MaskTree.BasicFieldsMaskTree();
        //    userMask = UpdateUserMaskAsPerFilter(filter, userMask);
        //    userMask = UpdateUserMaskAsPerAggregateSelect(select, userMask);

        //    // Filter out those permissions whose mask does not cover the entire user mask
        //    return FilterViolatedPermissionsInner(permissions, defaultMask, userMask);
        //}

        //private MaskTree UpdateUserMaskAsPerFilter(FilterExpression filter, MaskTree userMask)
        //{
        //    if (filter != null)
        //    {
        //        var filterPaths = filter.Select(e => (e.Path, e.Property));
        //        var filterMask = MaskTree.GetMaskTree(filterPaths);
        //        var filterAccess = Normalize(filterMask);

        //        userMask = userMask.UnionWith(filterAccess);
        //    }

        //    return userMask;
        //}

        //private MaskTree UpdateUserMaskAsPerOrderBy(OrderByExpression orderby, MaskTree userMask)
        //{
        //    if (orderby != null)
        //    {
        //        var orderbyPaths = orderby.Select(e => string.Join(".", e.Path.Union(new string[] { e.Property })));
        //        var orderbyMask = MaskTree.GetMaskTree(orderbyPaths);
        //        var orderbyAccess = Normalize(orderbyMask);

        //        userMask = userMask.UnionWith(orderbyAccess);
        //    }

        //    return userMask;
        //}

        //private MaskTree UpdateUserMaskAsPerAggregateSelect(AggregateSelectExpression select, MaskTree userMask)
        //{
        //    if (select != null)
        //    {
        //        var aggSelectPaths = select.Select(e => (e.Path, e.Property));
        //        var aggSelectMask = MaskTree.GetMaskTree(aggSelectPaths);
        //        var aggSelectAccess = Normalize(aggSelectMask);

        //        userMask = userMask.UnionWith(aggSelectAccess);
        //    }

        //    return userMask;
        //}

        //private MaskTree Normalize(MaskTree tree)
        //{
        //    tree.Validate(typeof(TEntity), _localizer);
        //    tree.Normalize(typeof(TEntity));

        //    return tree;
        //}

        //private IEnumerable<AbstractPermission> FilterViolatedPermissionsInner(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, MaskTree userMask)
        //{
        //    defaultMask = Normalize(defaultMask);
        //    return permissions.Where(e =>
        //    {
        //        var permissionMask = string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : Normalize(MaskTree.Parse(e.Mask));
        //        return permissionMask.Covers(userMask);
        //    });
        //}

        ///// <summary>
        ///// If the user has no permissions, throw a <see cref="ForbiddenException"/>.
        ///// Else if the user is subject to row-level access, return the query filter to apply
        ///// Else if the user has full => return a null filter
        ///// </summary>
        //protected virtual FilterExpression GetPermissionsFilter(IEnumerable<AbstractPermission> permissions)
        //{

        //}

        /// <summary>
        /// Applies the search argument, which is handled differently in every controller
        /// </summary>
        protected abstract Query<TEntity> Search(Query<TEntity> query, GetArguments args);

        /// <summary>
        /// Orders the query as per the orderby and desc arguments
        /// </summary>
        /// <param name="query">The base query to order</param>
        /// <param name="orderby">The orderby parameter which has the format 'A.B.C desc,D.E'</param>
        /// <param name="desc">True for a descending order</param>
        /// <returns>Ordered query</returns>
        protected virtual Query<TEntity> OrderBy(Query<TEntity> query, ExpressionOrderBy orderby)
        {
            orderby ??= DefaultOrderBy();
            return query.OrderBy(orderby);
        }

        /// <summary>
        /// Applies the default order which is over "Id" property descending
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected abstract ExpressionOrderBy DefaultOrderBy();

        /// <summary>
        /// Specifies the maximum page size to be returned by GET, defaults to <see cref="DEFAULT_MAX_PAGE_SIZE"/>
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DEFAULT_MAX_PAGE_SIZE;
        }

        ///// <summary>
        ///// If the user is subject to field-level access control, this method hides all the fields
        ///// that the user has no access to and modifies the metadata of the Entities accordingly
        ///// </summary>
        //protected virtual Task ApplyReadPermissionsMask(
        //    List<TEntity> resultEntities,
        //    Query<TEntity> query,
        //    IEnumerable<AbstractPermission> permissions,
        //    MaskTree defaultMask,
        //    CancellationToken cancellation)
        //{
        //    // TODO: is there a solution to this?
        //    return Task.CompletedTask;
        //}

        /// <summary>
        /// Gives controllers the chance to include custom data with all GET responses
        /// </summary>
        /// <param name="result">The unflattenned, untrimmed response to the GET request</param>
        /// <returns>An optional dictionary containing any extra information and an optional set of related entities</returns>
        protected virtual Task<Extras> GetExtras(IEnumerable<TEntity> result, CancellationToken cancellation)
        {
            return Task.FromResult<Extras>(null);
        }

        /// <summary>
        /// Retrieves the metadata of the entity
        /// </summary>
        /// <returns></returns>
        protected TypeMetadata GetMetadata()
        {
            int? tenantId = _tenantIdAccessor.GetTenantIdIfAny();
            int? definitionId = DefinitionId;
            Type type = typeof(TEntity);

            return _metadata.GetMetadata(tenantId, type, definitionId);
        }

        #region IFactSericeBase Implementation

        async Task<(List<Entity> Data, Extras Extras, bool IsPartial, int? Count)> IFactServiceBase.GetFact(GetArguments args, CancellationToken cancellation)
        {
            var (data, extras, isPartial, count) = await GetEntities(args, cancellation);
            var genericData = data.Cast<Entity>().ToList();

            return (genericData, extras, isPartial, count);
        }

        Task<(List<DynamicRow> Data, IEnumerable<TreeDimensionResult> Ancestors, bool IsPartial)> IFactServiceBase.GetAggregate(GetAggregateArguments args, CancellationToken cancellation)
        {
            return GetAggregate(args, cancellation);
        }

        #endregion
    }

    public interface IFactServiceBase
    {
        Task<(List<Entity> Data, Extras Extras, bool IsPartial, int? Count)> GetFact(GetArguments args, CancellationToken cancellation);

        Task<(List<DynamicRow> Data, IEnumerable<TreeDimensionResult> Ancestors, bool IsPartial)> GetAggregate(GetAggregateArguments args, CancellationToken cancellation);
    }
}
