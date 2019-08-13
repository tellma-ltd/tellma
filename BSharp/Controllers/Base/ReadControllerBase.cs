using BSharp.Controllers.Dto;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.EntityModel;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.ImportExport;
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
    [ApiController]
    [AuthorizeAccess]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class ReadControllerBase<TEntity> : ControllerBase
        where TEntity : Entity
    {
        // Private Fields
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;

        /// <summary>
        /// The default maximum page size returned by the <see cref="Get(GetArguments)"/>,
        /// it can be overridden by overriding <see cref="MaximumPageSize()"/>
        /// </summary>
        private static int DEFAULT_MAX_PAGE_SIZE => 10000;

        /// <summary>
        /// The maximum number of rows (data points) that can be returned by <see cref="GetAggregate(GetAggregateArguments)"/>, 
        /// if the result is lager the implementation returns a bad request 400
        /// </summary>
        private static int MAXIMUM_AGGREGATE_RESULT_SIZE => 65536;

        // Constructor
        public ReadControllerBase(ILogger logger, IStringLocalizer localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

        // HTTP Methods
        [HttpGet]
        public virtual async Task<ActionResult<GetResponse<TEntity>>> Get([FromQuery] GetArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                var result = await GetImplAsync(args);
                return Ok(result);
            }, _logger);
        }

        [HttpGet("aggregate")]
        public virtual async Task<ActionResult<GetAggregateResponse>> GetAggregate([FromQuery] GetAggregateArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                var result = await GetAggregateImplAsync(args);
                return Ok(result);
            }, _logger);
        }

        [HttpGet("export")]
        public virtual async Task<ActionResult> Export([FromQuery] ExportArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                // Get abstract grid
                var response = await GetImplAsync(args);
                var abstractFile = EntitiesToAbstractGrid(response, args);
                return AbstractGridToFileResult(abstractFile, args.Format);
            }, _logger);
        }

        // Endpoint implementations

        /// <summary>
        /// Returns the entities as per the specifications in the get request
        /// </summary>
        protected virtual async Task<GetResponse<TEntity>> GetImplAsync(GetArguments args)
        {
            // Parse the parameters
            var filter = FilterExpression.Parse(args.Filter);
            var orderby = OrderByExpression.Parse(args.OrderBy);
            var expand = ExpandExpression.Parse(args.Expand);
            var select = SelectExpression.Parse(args.Select);

            // Prepare the query
            var repo = GetRepository();
            var query = await repo.QueryAsync<TEntity>();

            // Retrieve the user permissions for the current view
            var permissions = await UserPermissions(Constants.Read);

            // Filter out permissions with masks that would be violated by the filter or order by arguments
            var defaultMask = GetDefaultMask() ?? new MaskTree();
            permissions = FilterViolatedPermissionsForFlatQuery(permissions, defaultMask, filter, orderby);

            // Apply read permissions
            var permissionsFilter = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsFilter);

            // Search
            query = Search(query, args, permissions);

            // Filter
            query = query.Filter(filter);

            // Before ordering or paging, retrieve the total count
            int totalCount = await query.CountAsync();

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
            var result = await expandedQuery.ToListAsync();

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(result, query, permissions, defaultMask);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(result, expand);

            // Prepare the result in a response object
            return new GetResponse<TEntity>
            {
                Skip = skip,
                Top = result.Count(),
                OrderBy = args.OrderBy,
                TotalCount = totalCount,

                Result = result,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TEntity))
            };
        }

        /// <summary>
        /// Returns the entities as per the specifications in the get request
        /// </summary>
        protected virtual async Task<GetAggregateResponse> GetAggregateImplAsync(GetAggregateArguments args)
        {
            // Parse the parameters
            var filter = FilterExpression.Parse(args.Filter);
            var select = AggregateSelectExpression.Parse(args.Select);

            // Prepare the query
            var repo = GetRepository();
            var query = await repo.AggregateQueryAsync<TEntity>();

            // Retrieve the user permissions for the current view
            var permissions = await UserPermissions(Constants.Read);
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
            var result = await query.ToListAsync();

            // Put a limit on the number of data points returned, to prevent DoS attacks
            if(result.Count > MAXIMUM_AGGREGATE_RESULT_SIZE)
            {
                var msg = _localizer["Error_NumberOfDataPointsExceedsMaximum0", MAXIMUM_AGGREGATE_RESULT_SIZE];
                throw new BadRequestException(msg);
            }

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(result, null);

            // Finally return the result
            return new GetAggregateResponse
            {
                Top = args.Top,
                IsPartial = isPartial,

                Result = result,
                RelatedEntities = relatedEntities
            };
        }
        
        /// <summary>
        /// Get the DbContext source on which the controller is based
        /// </summary>
        /// <returns></returns>
        protected abstract IRepository GetRepository();

        /// <summary>
        /// Retrieves the user permissions for the current view and the specified level
        /// </summary>
        protected abstract Task<IEnumerable<AbstractPermission>> UserPermissions(string action);

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

        private IEnumerable<AbstractPermission> FilterViolatedPermissionsInner(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, MaskTree userMask)
        {
            defaultMask = Normalize(defaultMask);
            return permissions.Where(e =>
            {
                var permissionMask = string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : Normalize(MaskTree.GetMaskTree(MaskTree.Split(e.Mask)));
                return permissionMask.Covers(userMask);
            });
        }

        private MaskTree Normalize(MaskTree tree)
        {
            tree.Validate(typeof(TEntity), _localizer);
            tree.Normalize(typeof(TEntity));

            return tree;
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

        /// <summary>
        /// If the user has no permissions, throw a forbidden Exception.
        /// Else if the user is subject to row-level access, apply it as a filter to the query
        /// Else if the user has full access let execution pass unhindred
        /// </summary>
        protected virtual FilterExpression GetReadPermissionsCriteria(IEnumerable<AbstractPermission> permissions)
        {
            // Check if the user has any permissions on ViewId at all, else throw forbidden exception
            // If the user has some permissions on ViewId, OR all their criteria together and apply the where clause

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
            orderby = orderby ?? DefaultOrderBy();
            return query.OrderBy(orderby);
        }

        /// <summary>
        /// Specifies the maximum page size to be returned by GET, defaults to <see cref="DEFAULT_MAX_PAGE_SIZE"/>
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DEFAULT_MAX_PAGE_SIZE;
        }

        /// <summary>
        /// Applies the default order which is over "Id" property descending
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected abstract OrderByExpression DefaultOrderBy();

        /// <summary>
        /// If the user is subject to field-level access control, this method hides all the fields
        /// that the user has no access to and modifies the metadata of the Entities accordingly
        /// </summary>
        protected virtual Task ApplyReadPermissionsMask(
            List<TEntity> resultEntities,
            Query<TEntity> query,
            IEnumerable<AbstractPermission> permissions,
            MaskTree defaultMask)
        {
            // TODO: is there is a solution to this?
            return Task.CompletedTask;
        }

        /// <summary>
        /// Takes a list of <see cref="Entity"/>s, and for every entity it inspects the navigation properties, if a navigation property
        /// contains an <see cref="Entity"/> with a strong type, it sets that property to null, and moves the strong entity into a separate
        /// "relatedEntities" collection, this has several advantages:
        /// 1 - JSON.NET will not have to deal with circular references
        /// 2 - Every strong entity is mentioned once in the JSON response (smaller response size)
        /// 3 - It makes it easier for clients to store and track entities in a central workspace
        /// </summary>
        /// <returns>A dictionary mapping every strong type name to a collection of entities of that type</returns>
        protected virtual Dictionary<string, IEnumerable<Entity>> FlattenAndTrim(IEnumerable<Entity> resultEntities, ExpandExpression expand)
        {
            // If the result is empty, nothing to do
            if (resultEntities == null || !resultEntities.Any())
            {
                return new Dictionary<string, IEnumerable<Entity>>();
            }

            var relatedEntities = new HashSet<Entity>();
            var resultHash = resultEntities.ToHashSet();

            // Method for efficiently retrieving the nav and nav collection properties of any entity
            var cacheNavigationProperties = new Dictionary<Type, IEnumerable<IPropInfo>>();
            IEnumerable<IPropInfo> NavProps(Entity entity)
            {
                if (!cacheNavigationProperties.TryGetValue(entity.GetType(), out IEnumerable<IPropInfo> properties))
                {
                    if (entity is DynamicEntity dynamicEntity)
                    {
                        properties = cacheNavigationProperties[entity.GetType()] =
                            dynamicEntity.Properties.Where(e =>
                                e.PropertyType.IsEntity());
                    }
                    else
                    {
                        // Return all navigation properties that Entity or list types
                        properties = cacheNavigationProperties[entity.GetType()] =
                            entity.GetType().GetProperties().Where(e =>
                                e.PropertyType.IsEntity() ||  /* nav property */
                                e.PropertyType.IsList()) /* nav collection property */
                            .Select(e => new PropInfo(e));
                    }
                }

                return properties;
            }

            // Recursively trims and flattens the entity and all entities reachable from it
            var alreadyFlattenedAndTrimmed = new HashSet<Entity>();
            void FlattenAndTrimInner(Entity entity)
            {
                if (entity == null || alreadyFlattenedAndTrimmed.Contains(entity))
                {
                    return;
                }

                // This ensures Flatten is executed on every entity only once
                alreadyFlattenedAndTrimmed.Add(entity);

                foreach (var navProp in NavProps(entity))
                {
                    if (navProp.PropertyType.IsList())
                    {
                        var collection = navProp.GetValue(entity);
                        if (collection != null)
                        {
                            foreach (var item in collection.Enumerate<Entity>())
                            {
                                FlattenAndTrimInner(item);
                            }
                        }
                    }
                    else if (navProp.GetValue(entity) is Entity relatedEntity) // Checks for null
                    {
                        // If the type is a strong one trim the property and add the entity to relatedEntities
                        if (navProp.PropertyType.IsStrongEntity())
                        {
                            // This property has a strong type, so we set it to null and put its value in the
                            // related entities collection (unless it is part of the main result)

                            // Set the property to null
                            navProp.SetValue(entity, null);
                            if (!resultHash.Contains(relatedEntity))
                            {
                                // Unless it is part of the main result, add it to relatedEntities
                                relatedEntities.Add(relatedEntity);
                            }
                        }

                        // Recursively call flatten on the related entity whether it's strong or weak
                        FlattenAndTrimInner(relatedEntity);
                    }
                }
            }

            // Flatten every entity
            foreach (var entity in resultEntities)
            {
                FlattenAndTrimInner(entity);
            }

            // Return the result
            return relatedEntities
                .GroupBy(e => e.GetType().GetRootType().Name)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        /// <summary>
        /// Retrieves the collection name from the Entity type
        /// </summary>
        protected static string GetCollectionName(Type entityType)
        {
            return entityType.GetRootType().Name;
        }

        /// <summary>
        /// Transforms an Entity response into an abstract grid that can be transformed into an file
        /// </summary>
        protected abstract AbstractDataGrid EntitiesToAbstractGrid(GetResponse<TEntity> response, ExportArguments args);


        // Maybe we should move these to ControllerUtilities

        protected FileResult AbstractGridToFileResult(AbstractDataGrid abstractFile, string format)
        {
            // Get abstract grid

            FileHandlerBase handler;
            string contentType;
            if (format == FileFormats.Xlsx)
            {
                handler = new ExcelHandler(_localizer);
                contentType = MimeTypes.Xlsx;
            }
            else if (format == FileFormats.Csv)
            {
                handler = new CsvHandler(_localizer);
                contentType = MimeTypes.Csv;
            }
            else
            {
                throw new FormatException(_localizer["Error_UnknownFileFormat"]);
            }

            var fileStream = handler.ToFileStream(abstractFile);
            fileStream.Seek(0, System.IO.SeekOrigin.Begin);
            return File(fileStream, contentType);
        }

        // DateTime utilities

        /// <summary>
        /// Changes the DateTimeOffset into a DateTime in the local time of the user suitable for exporting
        /// </summary>
        protected DateTime? ToExportDateTime(DateTimeOffset? offset)
        {
            if (offset == null)
            {
                return null;
            }

            var timeZone = TimeZoneInfo.Local;  // TODO: Use the user time zone 
            return TimeZoneInfo.ConvertTime(offset.Value, timeZone).DateTime;
        }

        /// <summary>
        /// Returns the default format for dates and date times
        /// </summary>
        protected string ExportDateTimeFormat(bool dateOnly)
        {
            return dateOnly ? "yyyy-MM-dd" : "yyyy-MM-dd hh:mm";
        }

        /// <summary>
        /// Attempts to intelligently parse an object (that comes from an imported file) to a DateTime
        /// </summary>
        protected DateTime? ParseImportedDateTime(object value)
        {
            if (value == null)
            {
                return null;
            }

            DateTime dateTime;

            if (value.GetType() == typeof(double))
            {
                // Double indicates the OLE Automation date typically represented in excel
                dateTime = DateTime.FromOADate((double)value);
            }
            else
            {
                // Parse the import value into a DateTime
                var valueString = value.ToString();
                dateTime = DateTime.Parse(valueString);
            }


            return dateTime;
        }

        /// <summary>
        /// Changes the DateTime into a DateTimeOffset by adding the user's local timezone, this effectively
        /// acts as the reverse of <see cref="ToExportDateTime(DateTimeOffset?)"/>
        /// </summary>
        protected DateTimeOffset? AddUserTimeZone(DateTime? value)
        {
            if (value == null)
            {
                return null;
            }

            // The date time supplied in the import does not the contain time zone offset
            // The code below adds the current user time zone to the date time supplied
            var timeZone = TimeZoneInfo.Local;  // TODO: Use the user time zone   
            var offset = timeZone.GetUtcOffset(DateTimeOffset.Now);
            var dtOffset = new DateTimeOffset(value.Value, offset);

            return dtOffset;
        }
    }
}
