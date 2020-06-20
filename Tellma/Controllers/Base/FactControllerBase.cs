using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.ImportExport;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Tellma.Entities.Descriptors;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using OData-like parameters
    /// </summary>
    [AuthorizeAccess]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class FactControllerBase<TEntity> : ControllerBase
        where TEntity : Entity
    {
        private readonly ILogger _logger;

        public FactControllerBase(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public virtual async Task<ActionResult<GetResponse<TEntity>>> GetFact([FromQuery] GetArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Retrieves the raw data from the database, unflattend, untrimmed 
                var service = GetFactService();
                var (data, extras, isPartial, totalCount) = await service.GetFact(args, cancellation);

                // Flatten and Trim
                var relatedEntities = FlattenAndTrim(data, cancellation);

                // Prepare the result in a response object
                var result = new GetResponse<TEntity>
                {
                    Skip = args.Skip,
                    Top = data.Count,
                    OrderBy = args.OrderBy,
                    TotalCount = totalCount,
                    IsPartial = isPartial,
                    Result = data,
                    RelatedEntities = relatedEntities,
                    CollectionName = GetCollectionName(typeof(TEntity)),
                    Extras = TransformExtras(extras, cancellation),
                    ServerTime = serverTime
                };

                return Ok(result);
            }, _logger);
        }

        [HttpGet("aggregate")]
        public virtual async Task<ActionResult<GetAggregateResponse>> GetAggregate([FromQuery] GetAggregateArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var (data, isPartial) = await GetFactService().GetAggregate(args, cancellation);

                // Finally return the result
                var result = new GetAggregateResponse
                {
                    Top = args.Top,
                    IsPartial = isPartial,
                    ServerTime = serverTime,

                    Result = data,
                    RelatedEntities = new Dictionary<string, IEnumerable<Entity>>() // TODO: Add ancestors of tree dimensions
                };

                return Ok(result);
            }, _logger);
        }

        #region Export Stuff

        //[HttpGet("export")]
        //public virtual async Task<ActionResult> Export([FromQuery] ExportArguments args, CancellationToken cancellation)
        //{
        //    return await ControllerUtilities.InvokeActionImpl(async () =>
        //    {
        //        // Get abstract grid
        //        var service = GetFactService();
        //        var (response, _, _) = await service.GetFact(args, cancellation);
        //        var abstractFile = EntitiesToAbstractGrid(response, args);
        //        return AbstractGridToFileResult(abstractFile, args.Format);
        //    }, _logger);
        //}

        /////////////////////////////
        // Endpoint Implementations
        /////////////////////////////

        #endregion

        protected abstract FactServiceBase<TEntity> GetFactService();

        /// <summary>
        /// Takes a list of <see cref="Entity"/>s, and for every entity it inspects the navigation properties, if a navigation property
        /// contains an <see cref="Entity"/> with a strong type, it sets that property to null, and moves the strong entity into a separate
        /// "relatedEntities" hash set, this has several advantages:
        /// 1 - JSON.NET will not have to deal with circular references
        /// 2 - Every strong entity is mentioned once in the JSON response (smaller response size)
        /// 3 - It makes it easier for clients to store and track entities in a central workspace
        /// </summary>
        /// <returns>A hash set of strong related entity in the original result entities (excluding the result entities)</returns>
        protected Dictionary<string, IEnumerable<Entity>> FlattenAndTrim<T>(IEnumerable<T> resultEntities, CancellationToken cancellation)
            where T : Entity
        {
            return ControllerUtilities.FlattenAndTrim(resultEntities, cancellation);
        }

        /// <summary>
        /// Retrieves the collection name from the Entity type
        /// </summary>
        protected static string GetCollectionName(Type entityType)
        {
            return entityType.GetRootType().Name;
        }

        protected virtual Extras TransformExtras(Extras extras, CancellationToken cancellation)
        {
            return extras;
        }
    }

    public abstract class ServiceBase
    {
        protected ValidationErrorsDictionary ModelState { get; } = new ValidationErrorsDictionary();


        #region Validation

        /// <summary>
        /// Recursively validates a list of entities, and all subsequent entities according to their <see cref="TypeMetadata"/>, adds the validation errors if any to the <see cref="ValidationErrorsDictionary"/>
        /// </summary>
        protected void ValidateList<T>(List<T> entities, TypeMetadata meta) where T : Entity
        {
            if (entities is null)
            {
                return;
            }

            if (meta is null)
            {
                throw new ArgumentNullException(nameof(meta));
            }

            // meta ??= _metadata.GetMetadata(_tenantIdAccessor.GetTenantId(), typeof(T));

            var validated = new HashSet<Entity>();
            foreach (var (key, errorMsg) in ValidateListInner(entities, meta, validated))
            {
                ModelState.AddModelError(key, errorMsg);
                if (ModelState.HasReachedMaxErrors)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Recursively validates an entity according to the provided <see cref="TypeMetadata"/>, adds the validation errors if any to the <see cref="ValidationErrorsDictionary"/>
        /// </summary>
        protected void ValidateEntity<T>(T entity, TypeMetadata meta) where T : Entity
        {
            if (entity is null)
            {
                return;
            }

            if (meta is null)
            {
                throw new ArgumentNullException(nameof(meta));
            }

            // meta ??= _metadata.GetMetadata(_tenantIdAccessor.GetTenantId(), typeof(T));

            var validated = new HashSet<Entity>();
            foreach (var (key, errorMsg) in ValidateEntityInner(entity, meta, validated))
            {
                ModelState.AddModelError(key, errorMsg);
                if (ModelState.HasReachedMaxErrors)
                {
                    return;
                }
            }
        }

        private static IEnumerable<(string key, string error)> ValidateListInner(IList entities, TypeMetadata meta, HashSet<Entity> validated)
        {
            for (int index = 0; index < entities.Count; index++)
            {
                var atIndex = entities[index];
                if (atIndex is null)
                {
                    continue;
                }
                else if (atIndex is Entity entity)
                {
                    if (!validated.Contains(entity))
                    {
                        validated.Add(entity);
                        foreach (var (key, error) in ValidateEntityInner(entity, meta, validated))
                        {
                            yield return ($"[{index}].{key}", error);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Bug: Only entities can be validated with {nameof(ValidateList)}. {atIndex.GetType().Name} does not derive from {nameof(Entity)}");
                }
            }
        }

        private static IEnumerable<(string key, string error)> ValidateEntityInner<T>(T entity, TypeMetadata meta, HashSet<Entity> validated) where T : Entity
        {
            foreach (var p in meta.SimpleProperties)
            {
                var value = p.Descriptor.GetValue(entity);
                var results = p.Validate(entity, value);

                foreach (var result in results)
                {
                    yield return (p.Descriptor.Name, result.ErrorMessage);
                }
            }

            foreach (var p in meta.NavigationProperties)
            {
                var valueMeta = p.TargetTypeMetadata;
                if (p.Descriptor.GetValue(entity) is Entity value && !validated.Contains(value))
                {
                    validated.Add(value);
                    foreach (var (key, msg) in ValidateEntityInner(entity, valueMeta, validated))
                    {
                        yield return ($"{p.Descriptor.Name}.{key}", msg);
                    }
                }
            }

            foreach (var p in meta.CollectionProperties)
            {
                var valueMeta = p.CollectionTargetTypeMetadata;
                var value = p.Descriptor.GetValue(entity);
                if (value is IList list)
                {
                    var listMeta = p.CollectionTargetTypeMetadata;
                    foreach (var (key, msg) in ValidateListInner(list, listMeta, validated))
                    {
                        yield return ($"{p.Descriptor.Name}{key}", msg);
                    }
                }
                else if (value is null)
                {
                    // Nothing to do
                }
                else
                {
                    throw new InvalidOperationException($"Bug: {nameof(CollectionPropertyDescriptor)}.{nameof(CollectionPropertyDescriptor.GetValue)} returned a non-list");
                }
            }
        }

        #endregion

    }

    public abstract class FactServiceBase<TEntity> : ServiceBase, IFactServiceBase
        where TEntity : Entity
    {
        private readonly IStringLocalizer _localizer;

        public FactServiceBase(IServiceProvider sp)
        {
            _localizer = sp.GetRequiredService<IStringLocalizer<Strings>>();
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
        public virtual async Task<(List<TEntity> Data, Extras Extras, bool IsPartial, int? Count)> GetFact(GetArguments args, CancellationToken cancellation)
        {
            // Parse the parameters
            var filter = FilterExpression.Parse(args.Filter);
            var orderby = OrderByExpression.Parse(args.OrderBy);
            var expand = ExpandExpression.Parse(args.Expand);
            var select = ParseSelect(args.Select);

            // Prepare the query
            var query = GetRepository().Query<TEntity>();

            // Retrieve the user permissions for the current view
            var permissions = await UserPermissions(Constants.Read, cancellation);

            // Filter out permissions with masks that would be violated by the filter or order by arguments
            var defaultMask = GetDefaultMask() ?? new MaskTree();
            var permissionsCount = permissions.Count();
            permissions = FilterViolatedPermissionsForFlatQuery(permissions, defaultMask, filter, orderby);
            var filteredPermissionsCount = permissions.Count();
            bool isPartial = permissionsCount != filteredPermissionsCount;

            // Apply read permissions
            var permissionsFilter = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsFilter);

            // Search
            query = Search(query, args, permissions);

            // Filter
            query = query.Filter(filter);

            // If requested, retrieve the total count before any ordering, paging, expanding or selecting
            int? totalCount = null;
            if (args.CountEntities)
            {
                totalCount = await query.CountAsync(cancellation, MAXIMUM_COUNT);
                if (totalCount >= MAXIMUM_COUNT)
                {
                    totalCount = int.MaxValue; // All we know is that the real count is >= MAXIMUM_COUNT
                }
            }

            // OrderBy
            query = OrderBy(query, orderby);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Top(top);

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            var expandedQuery = query.Expand(expand);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            expandedQuery = expandedQuery.Select(select);

            // Load the data in memory
            var data = await expandedQuery.ToListAsync(cancellation);
            var extras = await GetExtras(data, cancellation);

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(data, query, permissions, defaultMask, cancellation);

            // Return
            return (data, extras, isPartial, totalCount);
        }

        /// <summary>
        /// Returns a <see cref="List{DynamicEntity}"/> as per the specifications in the <see cref="GetAggregateArguments"/>,
        /// </summary>
        public virtual async Task<(List<DynamicEntity> Data, bool IsPartial)> GetAggregate(GetAggregateArguments args, CancellationToken cancellation)
        {
            // Parse the parameters
            var filter = FilterExpression.Parse(args.Filter);
            var select = AggregateSelectExpression.Parse(args.Select);

            // Prepare the query
            var query = GetRepository().AggregateQuery<TEntity>();

            // Retrieve the user permissions for the current view
            var permissions = await UserPermissions(Constants.Read, cancellation);
            var permissionsCount = permissions.Count();

            // Filter out permissions with masks that would be violated by the filter argument
            // orderby on the other hand is always mandated to be a subset of the selected parameters
            // and those in turn must be universally visible to the user, so no need to check orderby
            var defaultMask = GetDefaultMask() ?? new MaskTree();
            permissions = FilterViolatedPermissionsForAggregateQuery(permissions, defaultMask, filter, select);
            var filteredPermissionCount = permissions.Count();
            var isPartial = permissionsCount != filteredPermissionCount;

            // Apply read permissions
            FilterExpression permissionsCriteria = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsCriteria);

            // Filter
            query = query.Filter(filter);

            // Apply the top parameter
            var top = args.Top == 0 ? int.MaxValue : args.Top; // 0 means get all
            top = Math.Min(top, MAXIMUM_AGGREGATE_RESULT_SIZE + 1);
            query = query.Top(top);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            query = query.Select(select);

            // Load the data in memory
            var data = await query.ToListAsync(cancellation);

            // Put a limit on the number of data points returned, to prevent DoS attacks
            if (data.Count > MAXIMUM_AGGREGATE_RESULT_SIZE)
            {
                var msg = _localizer["Error_NumberOfDataPointsExceedsMaximum0", MAXIMUM_AGGREGATE_RESULT_SIZE];
                throw new BadRequestException(msg);
            }

            // Return
            return (data, isPartial);
        }

        /// <summary>
        /// Select argument may get huge and unweildly in certain cases, this method offers a chance
        /// for controllers to optimize queries by understanding special concise "shorthands" in
        /// the select string that get expanded into a proper select expression on the server.
        /// This way clients don't have to send large select string in the request for common scenarios
        /// </summary>
        protected virtual SelectExpression ParseSelect(string select)
        {
            return SelectExpression.Parse(select);
        }

        /// <summary>
        /// Get the DbContext source on which the controller is based
        /// </summary>
        /// <returns></returns>
        protected abstract IRepository GetRepository();
        
        /// <summary>
        /// Retrieves the user permissions for the current view and the specified level
        /// </summary>
        protected abstract Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation);

        /// <summary>
        /// If the user has no permission masks defined (can see all), this mask is used.
        /// This makes it easier to setup permissions such that a user cannot see employee
        /// salaries for example, since without this, the user can "expand" the Entity tree 
        /// all the way to the salaries if s/he has access to any Entity from which the employee entity is reachable
        /// </summary>
        /// <returns></returns>
        protected virtual MaskTree GetDefaultMask()
        {
            // TODO implement
            return new MaskTree();
        }

        /// <summary>
        /// Removes from the permissions all permissions that would be violated by the filter or order by, the behavior
        /// of the system here is that when a user orders by a field that she has no full access too, she only sees the
        /// rows where she can see that field, sometimes resulting in a shorter list, this is to prevent the user gaining
        /// any insight over fields she has no access to by filter or order the data
        /// </summary>
        protected IEnumerable<AbstractPermission> FilterViolatedPermissionsForFlatQuery(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, FilterExpression filter, OrderByExpression orderby)
        {
            // Step 1 - Build the "User Mask", i.e the mask containing the fields mentioned in the relevant components of the user query
            var userMask = MaskTree.BasicFieldsMaskTree();
            userMask = UpdateUserMaskAsPerFilter(filter, userMask);
            userMask = UpdateUserMaskAsPerOrderBy(orderby, userMask);

            // Filter out those permissions whose mask does not cover the entire user mask
            return FilterViolatedPermissionsInner(permissions, defaultMask, userMask);
        }

        /// <summary>
        /// Removes from the permissions all permissions that would be violated by the filter or aggregate select, the behavior
        /// of the system here is that when a user orders by a field that she has no full access too, she only sees the
        /// rows where she can see that field, sometimes resulting in a shorter list, this is to prevent the user gaining
        /// any insight over fields she has no access to by filter or order the data
        /// </summary>
        protected IEnumerable<AbstractPermission> FilterViolatedPermissionsForAggregateQuery(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, FilterExpression filter, AggregateSelectExpression select)
        {
            // Step 1 - Build the "User Mask", i.e the mask containing the fields mentioned in the relevant components of the user query
            var userMask = MaskTree.BasicFieldsMaskTree();
            userMask = UpdateUserMaskAsPerFilter(filter, userMask);
            userMask = UpdateUserMaskAsPerAggregateSelect(select, userMask);

            // Filter out those permissions whose mask does not cover the entire user mask
            return FilterViolatedPermissionsInner(permissions, defaultMask, userMask);
        }

        private MaskTree UpdateUserMaskAsPerFilter(FilterExpression filter, MaskTree userMask)
        {
            if (filter != null)
            {
                var filterPaths = filter.Select(e => (e.Path, e.Property));
                var filterMask = MaskTree.GetMaskTree(filterPaths);
                var filterAccess = Normalize(filterMask);

                userMask = userMask.UnionWith(filterAccess);
            }

            return userMask;
        }

        private MaskTree UpdateUserMaskAsPerOrderBy(OrderByExpression orderby, MaskTree userMask)
        {
            if (orderby != null)
            {
                var orderbyPaths = orderby.Select(e => string.Join("/", e.Path.Union(new string[] { e.Property })));
                var orderbyMask = MaskTree.GetMaskTree(orderbyPaths);
                var orderbyAccess = Normalize(orderbyMask);

                userMask = userMask.UnionWith(orderbyAccess);
            }

            return userMask;
        }

        private MaskTree UpdateUserMaskAsPerAggregateSelect(AggregateSelectExpression select, MaskTree userMask)
        {
            if (select != null)
            {
                var aggSelectPaths = select.Select(e => (e.Path, e.Property));
                var aggSelectMask = MaskTree.GetMaskTree(aggSelectPaths);
                var aggSelectAccess = Normalize(aggSelectMask);

                userMask = userMask.UnionWith(aggSelectAccess);
            }

            return userMask;
        }

        private MaskTree Normalize(MaskTree tree)
        {
            tree.Validate(typeof(TEntity), _localizer);
            tree.Normalize(typeof(TEntity));

            return tree;
        }

        private IEnumerable<AbstractPermission> FilterViolatedPermissionsInner(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, MaskTree userMask)
        {
            defaultMask = Normalize(defaultMask);
            return permissions.Where(e =>
            {
                var permissionMask = string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : Normalize(MaskTree.Parse(e.Mask));
                return permissionMask.Covers(userMask);
            });
        }

        /// <summary>
        /// If the user has no permissions, throw a forbidden Exception.
        /// Else if the user is subject to row-level access, apply it as a filter to the query
        /// Else if the user has full access let execution pass unhindred
        /// </summary>
        protected virtual FilterExpression GetReadPermissionsCriteria(IEnumerable<AbstractPermission> permissions)
        {
            // Check if the user has any permissions on View at all, else throw forbidden exception
            // If the user has some permissions on View, OR all their criteria together and apply the where clause

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
                // The user has access to part of the data set based on a list of filters that will 
                // be ORed together in a dynamic query
                IEnumerable<string> criteriaList = permissions.Select(e => e.Criteria);
                var oredCrtieria = criteriaList.Aggregate((l1, l2) => $"({l1}) or ({l2})");
                return FilterExpression.Parse(oredCrtieria);
            }
        }

        /// <summary>
        /// Applies the search argument, which is handled differently in every controller
        /// </summary>
        protected abstract Query<TEntity> Search(Query<TEntity> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions);

        /// <summary>
        /// Orders the query as per the orderby and desc arguments
        /// </summary>
        /// <param name="query">The base query to order</param>
        /// <param name="orderby">The orderby parameter which has the format 'A/B/C desc,D/E'</param>
        /// <param name="desc">True for a descending order</param>
        /// <returns>Ordered query</returns>
        protected virtual Query<TEntity> OrderBy(Query<TEntity> query, OrderByExpression orderby)
        {
            orderby ??= DefaultOrderBy();
            return query.OrderBy(orderby);
        }

        /// <summary>
        /// Applies the default order which is over "Id" property descending
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected abstract OrderByExpression DefaultOrderBy();

        /// <summary>
        /// Specifies the maximum page size to be returned by GET, defaults to <see cref="DEFAULT_MAX_PAGE_SIZE"/>
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DEFAULT_MAX_PAGE_SIZE;
        }

        /// <summary>
        /// If the user is subject to field-level access control, this method hides all the fields
        /// that the user has no access to and modifies the metadata of the Entities accordingly
        /// </summary>
        protected virtual Task ApplyReadPermissionsMask(
            List<TEntity> resultEntities,
            Query<TEntity> query,
            IEnumerable<AbstractPermission> permissions,
            MaskTree defaultMask,
            CancellationToken cancellation)
        {
            // TODO: is there is a solution to this?
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gives controllers the chance to include custom data with all GET responses
        /// </summary>
        /// <param name="result">The unflattenned, untrimmed response to the GET request</param>
        /// <returns>An optional dictionary containing any extra information and an optional set of related entities</returns>
        protected virtual Task<Extras> GetExtras(IEnumerable<TEntity> result, CancellationToken cancellation)
        {
            return Task.FromResult<Extras>(null);
        }

        async Task<(List<Entity> Data, Extras Extras, bool IsPartial, int? Count)> IFactServiceBase.GetFact(GetArguments args, CancellationToken cancellation)
        {
            var (data, extras, isPartial, count) = await GetFact(args, cancellation);
            var genericData = data.Cast<Entity>().ToList();

            return (genericData, extras, isPartial, count);
        }

        Task<(List<DynamicEntity> Data, bool IsPartial)> IFactServiceBase.GetAggregate(GetAggregateArguments args, CancellationToken cancellation)
        {
            return GetAggregate(args, cancellation);
        }

        #region Export

        

        ///// <summary>
        ///// Transforms an Entity response into an abstract grid that can be transformed into an file
        ///// </summary>
        //protected AbstractDataGrid EntitiesToAbstractGrid(List<TEntity> response, ExportArguments args)
        //{
        //    throw new NotImplementedException();
        //}


        //// Maybe we should move these to a Utiltites class

        //protected FileResult AbstractGridToFileResult(AbstractDataGrid abstractFile, string format)
        //{
        //    // Get abstract grid

        //    FileHandlerBase handler;
        //    string contentType;
        //    if (format == FileFormats.Xlsx)
        //    {
        //        handler = new ExcelHandler(_localizer);
        //        contentType = MimeTypes.Xlsx;
        //    }
        //    else if (format == FileFormats.Csv)
        //    {
        //        handler = new CsvHandler(_localizer);
        //        contentType = MimeTypes.Csv;
        //    }
        //    else
        //    {
        //        throw new FormatException(_localizer["Error_UnknownFileFormat"]);
        //    }

        //    var fileStream = handler.ToFileStream(abstractFile);
        //    fileStream.Seek(0, System.IO.SeekOrigin.Begin);
        //    return new FileStreamResult(fileStream, contentType);
        //}

        //// DateTime utilities

        ///// <summary>
        ///// Changes the DateTimeOffset into a DateTime in the local time of the user suitable for exporting
        ///// </summary>
        //protected DateTime? ToExportDateTime(DateTimeOffset? offset)
        //{
        //    if (offset == null)
        //    {
        //        return null;
        //    }

        //    var timeZone = TimeZoneInfo.Local;  // TODO: Use the user time zone 
        //    return TimeZoneInfo.ConvertTime(offset.Value, timeZone).DateTime;
        //}

        ///// <summary>
        ///// Returns the default format for dates and date times
        ///// </summary>
        //protected string ExportDateTimeFormat(bool dateOnly)
        //{
        //    return dateOnly ? "yyyy-MM-dd" : "yyyy-MM-dd hh:mm";
        //}

        ///// <summary>
        ///// Attempts to intelligently parse an object (that comes from an imported file) to a DateTime
        ///// </summary>
        //protected DateTime? ParseImportedDateTime(object value)
        //{
        //    if (value == null)
        //    {
        //        return null;
        //    }

        //    DateTime dateTime;

        //    if (value.GetType() == typeof(double))
        //    {
        //        // Double indicates the OLE Automation date typically represented in excel
        //        dateTime = DateTime.FromOADate((double)value);
        //    }
        //    else
        //    {
        //        // Parse the import value into a DateTime
        //        var valueString = value.ToString();
        //        dateTime = DateTime.ParseExact(valueString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        //    }


        //    return dateTime;
        //}

        ///// <summary>
        ///// Changes the DateTime into a DateTimeOffset by adding the user's local timezone, this effectively
        ///// acts as the reverse of <see cref="ToExportDateTime(DateTimeOffset?)"/>
        ///// </summary>
        //protected DateTimeOffset? AddUserTimeZone(DateTime? value)
        //{
        //    if (value == null)
        //    {
        //        return null;
        //    }

        //    // The date time supplied in the import does not the contain time zone offset
        //    // The code below adds the current user time zone to the date time supplied
        //    var timeZone = TimeZoneInfo.Local;  // TODO: Use the user time zone   
        //    var offset = timeZone.GetUtcOffset(DateTimeOffset.Now);
        //    var dtOffset = new DateTimeOffset(value.Value, offset);

        //    return dtOffset;
        //}

        #endregion
    }

    public interface IFactServiceBase
    {
        Task<(List<Entity> Data, Extras Extras, bool IsPartial, int? Count)> GetFact(GetArguments args, CancellationToken cancellation);

        Task<(List<DynamicEntity> Data, bool IsPartial)> GetAggregate(GetAggregateArguments args, CancellationToken cancellation);
    }
}
