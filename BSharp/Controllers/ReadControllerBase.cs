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
using System.Collections.Concurrent;
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
            var permissions = await UserPermissions(PermissionLevel.Read);

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
            var (entities, relatedEntities) = await query.ToListAsync();

            string collectionName = GetCollectionName(typeof(TDto));

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(entities, relatedEntities, collectionName, qClone, permissions, defaultMask);

            // Prepare the result in a response object
            var result = new GetResponse<TDto>
            {
                Skip = skip,
                Top = entities.Count(),
                OrderBy = args.OrderBy,
                TotalCount = totalCount,

                Result = entities,
                RelatedEntities = relatedEntities,
                CollectionName = collectionName
            };

            // Finally return the result
            return result;
        }

        /// <summary>
        /// Returns the entities as per the specifications in the get request
        /// </summary>
        protected virtual async Task<GetAggregateResponse> GetAggregateImplAsync(GetAggregateArguments args)
        {
            // Prepare the odata query
            var query = CreateODataAggregateQuery();

            // Retrieve the user permissions for the current view
            var permissions = await UserPermissions(PermissionLevel.Read);
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
            var (result, relatedEntities) = await query.ToListAsync();

            // Finally return the result
            return new GetAggregateResponse
            {
                Top = args.Top,
                IsPartial = isPartial,

                Result = result,
                RelatedEntities = relatedEntities,
            };
        }

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
        protected abstract Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level);

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
        protected virtual async Task ApplyReadPermissionsMask(
            List<TDto> result,
            EntitiesMap relatedEntities,
            string collectionName,
            ODataQuery<TDto> query,
            IEnumerable<AbstractPermission> permissions,
            MaskTree defaultMask)
        {
            //bool defaultMaskIsUnrestricted = defaultMask == null || defaultMask.IsUnrestricted;
            //bool allPermissionMasksAreEmpty = permissions.All(e => string.IsNullOrWhiteSpace(e.Mask));
            //bool anEmptyCriteriaIsPairedWithEmptyMask =
            //    permissions.Any(e => string.IsNullOrWhiteSpace(e.Mask) && string.IsNullOrWhiteSpace(e.Criteria));

            //if ((allPermissionMasksAreEmpty || anEmptyCriteriaIsPairedWithEmptyMask) && defaultMaskIsUnrestricted)
            //{
            //    // Optimization: if all masks are unrestricted, or an empty criteria is paired with an empty mask then we can skip this whole ordeal
            //    return;
            //}
            //else
            //{
            //    // Maps every DTO to its list of masks
            //    var maskedDtos = new Dictionary<DtoBase, HashSet<string>>();
            //    var unrestrictedDtos = new HashSet<DtoBase>();

            //    // Marks the DTO and all DTOs reachable from it as unrestricted
            //    void MarkUnrestricted(DtoBase dto, Type dtoType)
            //    {
            //        if (dto == null)
            //        {
            //            return;
            //        }

            //        if (maskedDtos.ContainsKey(dto))
            //        {
            //            maskedDtos.Remove(dto);
            //        }

            //        if (!unrestrictedDtos.Contains(dto))
            //        {
            //            unrestrictedDtos.Add(dto);
            //            foreach (var key in dto.EntityMetadata.Keys)
            //            {
            //                var prop = dtoType.GetProperty(key);
            //                if (prop.PropertyType.IsList())
            //                {
            //                    // This is a navigation collection, iterate over the rows
            //                    var collection = prop.GetValue(dto);
            //                    if (collection != null)
            //                    {
            //                        var collectionType = prop.PropertyType.CollectionType();
            //                        foreach (var row in collection.Enumerate<DtoBase>())
            //                        {
            //                            MarkUnrestricted(row, collectionType);
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    // This is a normal navigation property
            //                    var propValue = prop.GetValue(dto) as DtoBase;
            //                    var propType = prop.PropertyType;
            //                    MarkUnrestricted(propValue, propType);
            //                }
            //            }
            //        }
            //    }

            //    // Goes over this DTO and every dto reachable from it and marks each one with the accessible fields
            //    void MarkMask(DtoBase dto, Type dtoType, MaskTree mask)
            //    {
            //        if (dto == null)
            //        {
            //            return;
            //        }

            //        if (mask.IsUnrestricted)
            //        {
            //            MarkUnrestricted(dto, dtoType);
            //        }
            //        else
            //        {
            //            if (unrestrictedDtos.Contains(dto))
            //            {
            //                // Nothing to mask in an unrestricted DTO
            //                return;
            //            }
            //            else
            //            {
            //                if (!maskedDtos.ContainsKey(dto))
            //                {
            //                    // All DTOs will have their basic fields accessible
            //                    var accessibleFields = new HashSet<string>();
            //                    foreach (var basicField in dtoType.BasicFields())
            //                    {
            //                        accessibleFields.Add(basicField.Name);
            //                    }

            //                    maskedDtos[dto] = accessibleFields;
            //                }

            //                {
            //                    var accessibleFields = maskedDtos[dto];
            //                    foreach (var requestedField in dto.EntityMetadata.Keys)
            //                    {
            //                        var prop = dtoType.GetProperty(requestedField);
            //                        if (mask.ContainsKey(requestedField))
            //                        {
            //                            // If the field is included in the mask, make it accessible
            //                            if (!accessibleFields.Contains(requestedField))
            //                            {
            //                                accessibleFields.Add(requestedField);
            //                            }

            //                            if (prop.PropertyType.IsList())
            //                            {
            //                                // This is a navigation collection, iterate over the rows and apply the mask subtree
            //                                var collection = prop.GetValue(dto);
            //                                if (collection != null)
            //                                {
            //                                    var collectionType = prop.PropertyType.CollectionType();
            //                                    foreach (var row in collection.Enumerate<DtoBase>())
            //                                    {
            //                                        MarkMask(row, collectionType, mask[requestedField]);
            //                                    }
            //                                }
            //                            }
            //                            else
            //                            {
            //                                if (prop.IsNavigationField())
            //                                {
            //                                    // Make sure if the navigation property is included that its foreign key is included as well
            //                                    var foreignKeyNameAtt = prop.GetCustomAttribute<NavigationPropertyAttribute>();
            //                                    var foreignKeyName = foreignKeyNameAtt.ForeignKey;
            //                                    if (!string.IsNullOrWhiteSpace(foreignKeyName) && !accessibleFields.Contains(foreignKeyName))
            //                                    {
            //                                        accessibleFields.Add(foreignKeyName);
            //                                    }

            //                                    // Use recursion to update the rest of the tree
            //                                    var propValue = prop.GetValue(dto) as DtoBase;
            //                                    var propType = prop.PropertyType;
            //                                    MarkMask(propValue, propType, mask[requestedField]);
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    if (permissions.All(e => string.IsNullOrWhiteSpace(e.Criteria)))
            //    {
            //        // Having no criteria is a very common case that can be optimized by skipping the database call
            //        var addDefault = permissions.Any(p => string.IsNullOrWhiteSpace(p.Mask));
            //        var masks = permissions.Select(e => e.Mask).Where(mask => !string.IsNullOrWhiteSpace(mask));
            //        var maskTrees = masks.Select(mask => MaskTree.GetMaskTree(MaskTree.Split(mask))).ToList();
            //        if (addDefault)
            //        {
            //            maskTrees.Add(defaultMask);
            //        }

            //        // Calculate the union of all the mask fields
            //        var maskUnion = maskTrees.Aggregate(MaskTree.BasicFieldsMaskTree(), (t1, t2) => t1.UnionWith(t2));

            //        // Mark all the DTOs
            //        var dtoForQueryType = typeof(TDto);
            //        foreach (var item in result)
            //        {
            //            MarkMask(item, dtoForQueryType, maskUnion);
            //        }
            //    }
            //    else
            //    {
            //        // an array of every criteria and every mask
            //        var maskAndCriteriaArray = permissions
            //            .Where(e => !string.IsNullOrWhiteSpace(e.Criteria)) // Optimization: a null criteria is satisfied by the entire list of DTOs
            //            .GroupBy(e => e.Criteria)
            //            .Select(g => new
            //            {
            //                Criteria = g.Key,
            //                Mask = g.Select(e => string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : MaskTree.GetMaskTree(MaskTree.Split(e.Mask)))
            //                .Aggregate((t1, t2) => t1.UnionWith(t2)) // takes the union of all the mask trees
            //            }).ToArray();

            //        // This mask applies to every single DTO since the criteria is null
            //        var universalMask = permissions
            //            .Where(e => string.IsNullOrWhiteSpace(e.Criteria))
            //            .Distinct()
            //            .Select(e => string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : MaskTree.GetMaskTree(MaskTree.Split(e.Mask)))
            //            .Aggregate(MaskTree.BasicFieldsMaskTree(), (t1, t2) => t1.UnionWith(t2)); // we use a seed here since if the collection is empty this will throw an error

            //        var criteriaWithIndexes = maskAndCriteriaArray
            //            .Select((e, index) => new IndexAndCriteria { Criteria = e.Criteria, Index = index });

            //        var criteriaMapList = await query.GetIndexToIdMap(criteriaWithIndexes);

            //        // Go over the Ids in the result and apply all relevant masks to said DTO
            //        var dtoForQueryType = typeof(TDto);
            //        var criteriaMapDictionary = criteriaMapList
            //            .GroupBy(e => e.Id)
            //            .ToDictionary(e => e.Key, e => e.ToList());

            //        foreach (var (dto, i) in result.Select((e, i) => (e, i)))
            //        {
            //            var id = dto.Id;
            //            MaskTree mask;

            //            if (criteriaMapDictionary.ContainsKey(id))
            //            {
            //                // Those are DTOs that satisfy one or more non-null Criteria
            //                mask = criteriaMapDictionary[id]
            //                    .Select(e => maskAndCriteriaArray[e.Index].Mask)
            //                    .Aggregate((t1, t2) => t1.UnionWith(t2))
            //                    .UnionWith(universalMask);
            //            }
            //            else
            //            {
            //                // Those are DTOs that belong to the universal mask of null criteria
            //                mask = universalMask;
            //            }

            //            MarkMask(dto, dtoForQueryType, mask);
            //        }
            //    }

            //    // This where field-level security is applied, we read all masked DTOs and apply the
            //    // masks on them by setting the field to null and adjusting the metadata accordingly
            //    foreach (var (dto, accessibleFields) in maskedDtos)
            //    {
            //        List<Action> updates = new List<Action>(dto.EntityMetadata.Keys.Count);
            //        foreach (var requestedField in dto.EntityMetadata.Keys)
            //        {
            //            if (!accessibleFields.Contains(requestedField))
            //            {
            //                // Mark the field as restricted (we delay the call to avoid the dreadful "collection-was-modified" Exception)
            //                updates.Add(() => dto.EntityMetadata[requestedField] = FieldMetadata.Restricted);

            //                // Set the field to null
            //                var prop = dto.GetType().GetProperty(requestedField);
            //                try
            //                {
            //                    prop.SetValue(dto, null);
            //                }
            //                catch (Exception ex)
            //                {
            //                    if (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
            //                    {
            //                        // Programmer mistake
            //                        throw new InvalidOperationException($"DTO field {prop.Name} has a non nullable type, all non-basic DTO fields must have a nullable type");
            //                    }
            //                    else
            //                    {
            //                        throw ex;
            //                    }
            //                }
            //            }
            //        }

            //        updates.ForEach(a => a());
            //    }
            //}
        }

        /// <summary>
        /// Retrieves the collection name from the DTO type
        /// </summary>
        protected static string GetCollectionName(Type dtoType)
        {
            return GetRootType(dtoType).Name;
        }

        protected static Type GetRootType(Type dtoType)
        {
            return dtoType; // TODO
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
