using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.ImportExport;
using BSharp.Services.OData;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace BSharp.Controllers
{
    [ApiController]
    [AuthorizeAccess]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class ReadControllerBase<TDto> : ControllerBase
        where TDto : DtoBase
    {
        // Constants
        private const int DEFAULT_MAX_PAGE_SIZE = 10000;
        public const string ALL = ControllerUtilities.ALL;


        // Private Fields
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        protected readonly IODataQueryFactory _odataFactory;

        // Constructor
        public ReadControllerBase(ILogger logger, IStringLocalizer localizer, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _localizer = localizer;
            _odataFactory = serviceProvider.GetRequiredService<IODataQueryFactory>();
        }

        // HTTP Methods
        [HttpGet]
        public virtual async Task<ActionResult<GetResponse<TDto>>> Get([FromQuery] GetArguments args)
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
                var abstractFile = DtosToAbstractGrid(response, args);
                return AbstractGridToFileResult(abstractFile, args.Format);
            }, _logger);
        }

        // Endpoint implementations

        /// <summary>
        /// Returns the entities as per the specifications in the get request
        /// </summary>
        protected virtual async Task<GetResponse<TDto>> GetImplAsync(GetArguments args)
        {
            // Prepare the odata query
            var query = CreateODataQuery();

            // Retrieve the user permissions for the current view
            var permissions = await UserPermissions(Constants.Read);

            // Filter out permissions with masks that would be violated by the filter or order by arguments
            var defaultMask = GetDefaultMask() ?? new MaskTree();
            permissions = FilterViolatedPermissionsForFlatQuery(permissions, defaultMask, args.Filter, args.OrderBy);

            // Apply read permissions
            var permissionsFilter = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsFilter);

            // Search
            query = Search(query, args, permissions);

            // Filter
            query = query.Filter(args.Filter);

            // Before ordering or paging, retrieve the total count
            int totalCount = await query.CountAsync();

            // OrderBy
            query = OrderBy(query, args.OrderBy);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Top(top);
            var qClone = query.Clone();

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            query.Expand(args.Expand);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            query.Select(args.Select);

            // Load the data in memory
            var result = await query.ToListAsync();

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(result, qClone, permissions, defaultMask);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(result, args.Expand);

            // Prepare the result in a response object
            return new GetResponse<TDto>
            {
                Skip = skip,
                Top = result.Count(),
                OrderBy = args.OrderBy,
                TotalCount = totalCount,

                Result = result,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TDto))
            };
        }

        /// <summary>
        /// Returns the entities as per the specifications in the get request
        /// </summary>
        protected virtual async Task<GetAggregateResponse> GetAggregateImplAsync(GetAggregateArguments args)
        {
            // Prepare the odata query
            var query = CreateODataAggregateQuery();

            // Retrieve the user permissions for the current view
            var permissions = await UserPermissions(Constants.Read);
            var permissionsCount = permissions.Count();

            // Filter out permissions with masks that would be violated by the filter argument
            // orderby on the other hand is always mandated to be a subset of the selected parameters
            // and those in turn must be universally visible to the user, so no need to check orderby
            var defaultMask = GetDefaultMask() ?? new MaskTree();
            permissions = FilterViolatedPermissionsForAggregateQuery(permissions, defaultMask, args.Filter, args.Select);
            var filteredPermissionCount = permissions.Count();
            var isPartial = permissionsCount != filteredPermissionCount;

            // Apply read permissions
            string permissionsCriteria = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsCriteria);

            // Filter
            query = query.Filter(args.Filter);

            // Apply the top parameter
            query = query.Top(args.Top);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            query.Select(args.Select);

            // Load the data in memory
            var result = await query.ToListAsync();

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

        //protected EntitiesMap RelatedEntities(IEnumerable<DtoBase> result, IndexedEntities strongIdEntities)
        //{
        //    var map = new EntitiesMap();
        //    if (result.Any())
        //    {
        //        var resultType = result.First().GetType();
        //        var resultRootType = GetRootType(resultType);

        //        foreach (var (rootType, entities) in strongIdEntities)
        //        {
        //            if (rootType == resultRootType)
        //            {
        //                // related entities do not contain the elements in the main result
        //                var resultHash = result.ToHashSet();
        //                map[rootType.Name] = entities.Values.Where(e => !resultHash.Contains(e));
        //            }
        //            else
        //            {
        //                map[rootType.Name] = entities.Values;
        //            }
        //        }
        //    }

        //    return map;
        //}

        /// <summary>
        /// Get the DbContext source on which the controller is based
        /// </summary>
        /// <returns></returns>
        protected abstract DbContext GetDbContext();

        /// <summary>
        /// Get the function that maps every type to a "source", ie. a composable SQL statement that returns that source
        /// </summary>
        protected abstract Func<Type, string> GetSources();

        /// <summary>
        /// Retrieves the user permissions for the current view and the specified level
        /// </summary>
        protected abstract Task<IEnumerable<AbstractPermission>> UserPermissions(string action);

        /// <summary>
        /// If the user has no permission masks defined (can see all), this mask is used.
        /// This makes it easier to setup permissions such that a user cannot see employee
        /// salaries for example, since without this, the user can "expand" the DTO tree 
        /// all the way to the salaries if s/he has access to any DTO from which the employee entity is reachable
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
        protected IEnumerable<AbstractPermission> FilterViolatedPermissionsForFlatQuery(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, string filter, string orderby)
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
        protected IEnumerable<AbstractPermission> FilterViolatedPermissionsForAggregateQuery(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, string filter, string select)
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
            tree.Validate(typeof(TDto), _localizer);
            tree.Normalize(typeof(TDto));

            return tree;
        }

        private MaskTree UpdateUserMaskAsPerFilter(string filter, MaskTree userMask)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterExp = FilterExpression.Parse(filter);
                var filterPaths = filterExp.Select(e => (e.Path, e.Property));
                var filterMask = MaskTree.GetMaskTree(filterPaths);
                var filterAccess = Normalize(filterMask);

                userMask = userMask.UnionWith(filterAccess);
            }

            return userMask;
        }

        private MaskTree UpdateUserMaskAsPerOrderBy(string orderby, MaskTree userMask)
        {
            if (!string.IsNullOrEmpty(orderby))
            {
                var orderbyExp = OrderByExpression.Parse(orderby);
                var orderbyPaths = orderbyExp.Select(e => string.Join("/", e.Path.Union(new string[] { e.Property })));
                var orderbyMask = MaskTree.GetMaskTree(orderbyPaths);
                var orderbyAccess = Normalize(orderbyMask);

                userMask = userMask.UnionWith(orderbyAccess);
            }

            return userMask;
        }

        private MaskTree UpdateUserMaskAsPerAggregateSelect(string select, MaskTree userMask)
        {
            if (!string.IsNullOrEmpty(select))
            {
                var aggSelectExp = SelectAggregateExpression.Parse(select);
                var aggSelectPaths = aggSelectExp.Select(e => (e.Path, e.Property));
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
        protected virtual string GetReadPermissionsCriteria(IEnumerable<AbstractPermission> permissions)
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
                return oredCrtieria;
            }
        }

        /// <summary>
        /// Applies the search argument, which is handled differently in every controller
        /// </summary>
        protected abstract ODataQuery<TDto> Search(ODataQuery<TDto> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions);

        /// <summary>
        /// Orders the query as per the orderby and desc arguments
        /// </summary>
        /// <param name="query">The base query to order</param>
        /// <param name="orderby">The orderby parameter which has the format 'A/B/C desc,D/E'</param>
        /// <param name="desc">True for a descending order</param>
        /// <returns>Ordered query</returns>
        protected virtual ODataQuery<TDto> OrderBy(ODataQuery<TDto> query, string orderby)
        {
            orderby = string.IsNullOrWhiteSpace(orderby) ? DefaultOrderBy() : orderby;
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
        protected abstract string DefaultOrderBy();

        /// <summary>
        /// If the user is subject to field-level access control, this method hides all the fields
        /// that the user has no access to and modifies the metadata of the DTOs accordingly
        /// </summary>
        protected virtual Task ApplyReadPermissionsMask(
            List<TDto> resultEntities,
            ODataQuery<TDto> query,
            IEnumerable<AbstractPermission> permissions,
            MaskTree defaultMask)
        {
            // TODO: is there is a solution to this?
            return Task.CompletedTask;
        }

        protected virtual Dictionary<string, IEnumerable<DtoBase>> FlattenAndTrim(IEnumerable<DtoBase> resultEntities, string expand)
        {
            // If the result is empty, nothing to do
            if (resultEntities == null || !resultEntities.Any())
            {
                return new Dictionary<string, IEnumerable<DtoBase>>();
            }

            var relatedEntities = new HashSet<DtoBase>();
            var resultHash = resultEntities.ToHashSet();

            // Method for efficiently retrieving the nav and nav collection properties of any entity
            var cacheNavigationProperties = new Dictionary<Type, IEnumerable<IPropInfo>>();
            IEnumerable<IPropInfo> NavProps(DtoBase entity)
            {
                if (!cacheNavigationProperties.TryGetValue(entity.GetType(), out IEnumerable<IPropInfo> properties))
                {
                    if (entity is DynamicEntity dynamicEntity)
                    {
                        properties = cacheNavigationProperties[entity.GetType()] =
                            dynamicEntity.Properties.Where(e =>
                                e.PropertyType.IsDto());
                    }
                    else
                    {
                        // Return all navigation properties that DTO or list types
                        properties = cacheNavigationProperties[entity.GetType()] =
                            entity.GetType().GetProperties().Where(e =>
                                e.PropertyType.IsDto() ||  /* nav property */
                                e.PropertyType.IsList()) /* nav collection property */
                            .Select(e => new PropInfo(e));
                    }
                }

                return properties;
            }

            // Recursively trims and flattens the entity and all entities reachable from it
            var alreadyFlattenedAndTrimmed = new HashSet<DtoBase>();
            void FlattenAndTrimInner(DtoBase entity)
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
                            foreach (var item in collection.Enumerate<DtoBase>())
                            {
                                FlattenAndTrimInner(item);
                            }
                        }
                    }
                    else if (navProp.GetValue(entity) is DtoBase relatedEntity) // Checks for null
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
        /// Retrieves the collection name from the DTO type
        /// </summary>
        protected static string GetCollectionName(Type entityType)
        {
            return entityType.GetRootType().Name;
        }

        /// <summary>
        /// Transforms a DTO response into an abstract grid that can be transformed into an file
        /// </summary>
        protected abstract AbstractDataGrid DtosToAbstractGrid(GetResponse<TDto> response, ExportArguments args);

        protected ODataQuery<TDto> CreateODataQuery()
        {
            var conn = GetDbContext().Database.GetDbConnection();
            var sources = GetSources();
            ODataQuery<TDto> query = _odataFactory.MakeODataQuery<TDto>(conn, sources);

            return query;
        }


        protected ODataAggregateQuery<TDto> CreateODataAggregateQuery()
        {
            var conn = GetDbContext().Database.GetDbConnection();
            var sources = GetSources();
            ODataAggregateQuery<TDto> query = _odataFactory.MakeODataAggregateQuery<TDto>(conn, sources);

            return query;
        }

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
