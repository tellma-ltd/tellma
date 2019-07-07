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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;


namespace BSharp.Controllers
{
    [ApiController]
    [AuthorizeAccess]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class ReadControllerBase<TDto, TDtoForQuery, TKey> : ControllerBase
        where TDto : DtoKeyBase<TKey>
        where TDtoForQuery : DtoKeyBase<TKey>
    {
        // Constants
        private const int DEFAULT_MAX_PAGE_SIZE = 10000;
        public const string ALL = ControllerUtilities.ALL;


        // Private Fields
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        protected readonly IODataQueryFactory _odataFactory;
        protected static ConcurrentDictionary<Type, string> _getCollectionNameCache = new ConcurrentDictionary<Type, string>(); // This cache never expires

        protected IMapper Mapper { get; }

        // Constructor
        public ReadControllerBase(ILogger logger, IStringLocalizer localizer, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _localizer = localizer;
            _odataFactory = serviceProvider.GetRequiredService<IODataQueryFactory>();
            Mapper = serviceProvider.GetRequiredService<IMapper>();
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

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TDto>>> GetById(TKey id, [FromQuery] GetByIdArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                var result = await GetByIdImplAsync(id, args);
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
            permissions = FilterViolatedPermissions(permissions, defaultMask, args.Filter, args.OrderBy);

            // Apply read permissions
            query = await ApplyReadPermissionsCriteria(query, permissions);

            // Search
            query = Search(query, args, permissions);

            // Filter
            query.Filter(args.Filter);

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
            var memoryList = await query.ToListAsync();

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(memoryList, qClone, permissions, defaultMask);

            // Flatten related DTOs
            var relatedEntities = FlattenRelatedEntitiesAndTrim(memoryList, args.Expand);

            // Set to null all Dto ForQuery properties
            var responseData = Mapper.Map<List<TDto>>(memoryList);

            // Prepare the result in a response object
            var result = new GetResponse<TDto>
            {
                Skip = skip,
                Top = memoryList.Count(),
                OrderBy = args.OrderBy,
                TotalCount = totalCount,

                Data = responseData,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TDto))
            };

            // Finally return the result
            return result;
        }

        /// <summary>
        /// Returns a single entity as per the ID and specifications in the get request
        /// </summary>
        protected virtual async Task<GetByIdResponse<TDto>> GetByIdImplAsync(TKey id, [FromQuery] GetByIdArguments args)
        {
            // Prepare the odata query
            var query = CreateODataQuery();

            // Add the filter by Id
            query.FilterByIds(id);

            // Check that the entity exists
            int count = await query.CountAsync();
            if (count == 0)
            {
                throw new NotFoundException<TKey>(id);
            }

            // Apply read permissions
            var permissions = await UserPermissions(PermissionLevel.Read);
            query = await ApplyReadPermissionsCriteria(query, permissions);

            // Take a copy of the query without the expand or the select
            var qClone = query.Clone();

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            query.Expand(args.Expand);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            query.Select(args.Select);

            // Load
            var dtoForQuery = await query.FirstOrDefaultAsync();
            if (dtoForQuery == null)
            {
                // We already checked for not found earlier,
                // This can only mean lack of permissions
                throw new ForbiddenException();
            }


            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var singleton = new List<TDtoForQuery> { dtoForQuery };
            await ApplyReadPermissionsMask(singleton, qClone, permissions, GetDefaultMask());

            // Flatten Related Entities
            var relatedEntities = FlattenRelatedEntitiesAndTrim(singleton, args.Expand);

            // Map the primary result to DTO too
            var dto = Mapper.Map<TDto>(dtoForQuery);

            // Return
            var result = new GetByIdResponse<TDto>
            {
                Entity = dto,
                CollectionName = GetCollectionName(typeof(TDto)),
                RelatedEntities = relatedEntities
            };

            return result;
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
        /// Verifies that the user has sufficient permissions to update the list of entities provided, this implementation 
        /// assumes that the view has permission levels Read and Update only, which most entities
        /// </summary>
        protected virtual async Task CheckActionPermissions(IEnumerable<TKey> entityIds)
        {
            // TODO

            //var updatePermissions = await UserPermissions(PermissionLevel.Update);
            //if (!updatePermissions.Any())
            //{
            //    // User has no permissions on this table whatsoever, forbid
            //    throw new ForbiddenException();
            //}
            //else if (updatePermissions.Any(e => string.IsNullOrWhiteSpace(e.Criteria)))
            //{
            //    // User has unfiltered update permission on the table => proceed
            //    return;
            //}
            //else
            //{
            //    // User can update items under certain conditions, so we check those conditions here
            //    IEnumerable<string> criteriaList = updatePermissions.Select(e => e.Criteria);

            //    // The parameter on which the expression is based
            //    var eParam = Expression.Parameter(typeof(TDtoForQuery));

            //    // Prepare the lambda
            //    Expression whereClause = ToORedWhereClause<TDtoForQuery>(criteriaList, eParam);
            //    var lambda = Expression.Lambda<Func<TDtoForQuery, bool>>(whereClause, eParam);

            //    await CheckPermissionsForOld(entityIds, lambda);
            //}
        }

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
        protected IEnumerable<AbstractPermission> FilterViolatedPermissions(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, string filter, string orderby)
        {
            MaskTree Normalize(MaskTree tree)
            {
                tree.Validate(typeof(TDtoForQuery), _localizer);
                tree.Normalize(typeof(TDtoForQuery));

                return tree;
            }

            defaultMask = Normalize(defaultMask);
            var userMask = MaskTree.BasicFieldsMaskTree();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterExp = FilterExpression.Parse(filter);
                var filterPaths = filterExp.Select(e => string.Join("/", e.Path.Union(new string[] { e.Property })));
                var filterMask = MaskTree.GetMaskTree(filterPaths);
                var filterAccess = Normalize(filterMask);

                userMask = userMask.UnionWith(filterAccess);
            }

            if (!string.IsNullOrEmpty(orderby))
            {
                var orderbyExp = OrderByExpression.Parse(orderby);
                var orderbyPaths = orderbyExp.Select(e => string.Join("/", e.Path.Union(new string[] { e.Property })));
                var orderbyMask = MaskTree.GetMaskTree(orderbyPaths);
                var orderbyAccess = Normalize(orderbyMask);

                userMask = userMask.UnionWith(orderbyAccess);
            }

            return permissions.Where(e =>
            {
                var permissionMask = string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : Normalize(MaskTree.GetMaskTree(MaskTree.Split(e.Mask)));
                return permissionMask.Covers(userMask);
            });
        }

        /// <summary>
        /// If the user has no permissions, throw a forbidden Exception.
        /// Else if the user is subject to row-level access, apply it as a filter to the query
        /// Else if the user has full access let execution pass unhindred
        /// </summary>
        protected virtual Task<ODataQuery<TDtoForQuery, TKey>> ApplyReadPermissionsCriteria(ODataQuery<TDtoForQuery, TKey> query, IEnumerable<AbstractPermission> permissions)
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
                return Task.FromResult(query);
            }
            else
            {
                // The user has access to part of the data set based on a list of filters that will 
                // be ORed together in a dynamic linq query
                IEnumerable<string> criteriaList = permissions.Select(e => e.Criteria);
                var oredCrtieria = criteriaList.Aggregate((l1, l2) => $"({l1}) or ({l2})");
                query = query.Filter(oredCrtieria);
            }

            return Task.FromResult(query);
        }

        /// <summary>
        /// Applies the search argument, which is handled differently in every controller
        /// </summary>
        protected abstract ODataQuery<TDtoForQuery, TKey> Search(ODataQuery<TDtoForQuery, TKey> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions);

        /// <summary>
        /// Orders the query as per the orderby and desc arguments
        /// </summary>
        /// <param name="query">The base query to order</param>
        /// <param name="orderby">The orderby parameter which has the format 'A/B/C desc,D/E'</param>
        /// <param name="desc">True for a descending order</param>
        /// <returns>Ordered query</returns>
        protected virtual ODataQuery<TDtoForQuery, TKey> OrderBy(ODataQuery<TDtoForQuery, TKey> query, string orderby)
        {
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                query.OrderBy(orderby);
            }
            else
            {
                query = DefaultOrder(query);
            }

            return query;
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
        protected virtual ODataQuery<TDtoForQuery, TKey> DefaultOrder(ODataQuery<TDtoForQuery, TKey> query)
        {
            return query.OrderBy("Id desc");
        }

        /// <summary>
        /// If the user is subject to field-level access control, this method hides all the fields
        /// that the user has no access to and modifies the metadata of the DTOs accordingly
        /// </summary>
        protected virtual async Task ApplyReadPermissionsMask(List<TDtoForQuery> memoryList, ODataQuery<TDtoForQuery, TKey> query, IEnumerable<AbstractPermission> permissions, MaskTree defaultMask)
        {
            bool defaultMaskIsUnrestricted = defaultMask == null || defaultMask.IsUnrestricted;
            bool allPermissionMasksAreEmpty = permissions.All(e => string.IsNullOrWhiteSpace(e.Mask));
            bool anEmptyCriteriaIsPairedWithEmptyMask =
                permissions.Any(e => string.IsNullOrWhiteSpace(e.Mask) && string.IsNullOrWhiteSpace(e.Criteria));

            if ((allPermissionMasksAreEmpty || anEmptyCriteriaIsPairedWithEmptyMask) && defaultMaskIsUnrestricted)
            {
                // Optimization: if all masks are unrestricted, or an empty criteria is paired with an empty mask then we can skip this whole ordeal
                return;
            }
            else
            {
                // Maps every DTO to its list of masks
                var maskedDtos = new Dictionary<DtoBase, HashSet<string>>();
                var unrestrictedDtos = new HashSet<DtoBase>();

                // Marks the DTO and all DTOs reachable from it as unrestricted
                void MarkUnrestricted(DtoBase dto, Type dtoType)
                {
                    if (dto == null)
                    {
                        return;
                    }

                    if (maskedDtos.ContainsKey(dto))
                    {
                        maskedDtos.Remove(dto);
                    }

                    if (!unrestrictedDtos.Contains(dto))
                    {
                        unrestrictedDtos.Add(dto);
                        foreach (var key in dto.EntityMetadata.Keys)
                        {
                            var prop = dtoType.GetProperty(key);
                            if (prop.PropertyType.IsList())
                            {
                                // This is a navigation collection, iterate over the rows
                                var collection = prop.GetValue(dto);
                                if (collection != null)
                                {
                                    var collectionType = prop.PropertyType.CollectionType();
                                    foreach (var row in collection.Enumerate<DtoBase>())
                                    {
                                        MarkUnrestricted(row, collectionType);
                                    }
                                }
                            }
                            else
                            {
                                // This is a normal navigation property
                                var propValue = prop.GetValue(dto) as DtoBase;
                                var propType = prop.PropertyType;
                                MarkUnrestricted(propValue, propType);
                            }
                        }
                    }
                }

                // Goes over this DTO and every dto reachable from it and marks each one with the accessible fields
                void MarkMask(DtoBase dto, Type dtoType, MaskTree mask)
                {
                    if (dto == null)
                    {
                        return;
                    }

                    if (mask.IsUnrestricted)
                    {
                        MarkUnrestricted(dto, dtoType);
                    }
                    else
                    {
                        if (unrestrictedDtos.Contains(dto))
                        {
                            // Nothing to mask in an unrestricted DTO
                            return;
                        }
                        else
                        {
                            if (!maskedDtos.ContainsKey(dto))
                            {
                                // All DTOs will have their basic fields accessible
                                var accessibleFields = new HashSet<string>();
                                foreach (var basicField in dtoType.BasicFields())
                                {
                                    accessibleFields.Add(basicField.Name);
                                }

                                maskedDtos[dto] = accessibleFields;
                            }

                            {
                                var accessibleFields = maskedDtos[dto];
                                foreach (var requestedField in dto.EntityMetadata.Keys)
                                {
                                    var prop = dtoType.GetProperty(requestedField);
                                    if (mask.ContainsKey(requestedField))
                                    {
                                        // If the field is included in the mask, make it accessible
                                        if (!accessibleFields.Contains(requestedField))
                                        {
                                            accessibleFields.Add(requestedField);
                                        }

                                        if (prop.PropertyType.IsList())
                                        {
                                            // This is a navigation collection, iterate over the rows and apply the mask subtree
                                            var collection = prop.GetValue(dto);
                                            if (collection != null)
                                            {
                                                var collectionType = prop.PropertyType.CollectionType();
                                                foreach (var row in collection.Enumerate<DtoBase>())
                                                {
                                                    MarkMask(row, collectionType, mask[requestedField]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (prop.IsNavigationField())
                                            {
                                                // Make sure if the navigation property is included that its foreign key is included as well
                                                var foreignKeyNameAtt = prop.GetCustomAttribute<NavigationPropertyAttribute>();
                                                var foreignKeyName = foreignKeyNameAtt.ForeignKey;
                                                if (!string.IsNullOrWhiteSpace(foreignKeyName) && !accessibleFields.Contains(foreignKeyName))
                                                {
                                                    accessibleFields.Add(foreignKeyName);
                                                }

                                                // Use recursion to update the rest of the tree
                                                var propValue = prop.GetValue(dto) as DtoBase;
                                                var propType = prop.PropertyType;
                                                MarkMask(propValue, propType, mask[requestedField]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (permissions.All(e => string.IsNullOrWhiteSpace(e.Criteria)))
                {
                    // Having no criteria is a very common case that can be optimized by skipping the database call
                    var addDefault = permissions.Any(p => string.IsNullOrWhiteSpace(p.Mask));
                    var masks = permissions.Select(e => e.Mask).Where(e => !string.IsNullOrWhiteSpace(e));
                    var maskTrees = masks.Select(mask => MaskTree.GetMaskTree(MaskTree.Split(mask))).ToList();
                    if (addDefault)
                    {
                        maskTrees.Add(defaultMask);
                    }

                    // Calculate the union of all the mask fields
                    var maskUnion = maskTrees.Aggregate(MaskTree.BasicFieldsMaskTree(), (t1, t2) => t1.UnionWith(t2));

                    // Mark all the DTOs
                    var dtoForQueryType = typeof(TDtoForQuery);
                    foreach (var item in memoryList)
                    {
                        MarkMask(item, dtoForQueryType, maskUnion);
                    }
                }
                else
                {
                    // an array of every criteria and every mask
                    var maskAndCriteriaArray = permissions
                        .Where(e => !string.IsNullOrWhiteSpace(e.Criteria)) // Optimization: a null criteria is satisfied by the entire list of DTOs
                        .GroupBy(e => e.Criteria)
                        .Select(g => new
                        {
                            Criteria = g.Key,
                            Mask = g.Select(e => string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : MaskTree.GetMaskTree(MaskTree.Split(e.Mask)))
                            .Aggregate((t1, t2) => t1.UnionWith(t2)) // takes the union of all the mask trees
                         }).ToArray();

                    // This mask applies to every single DTO since the criteria is null
                    var universalMask = permissions
                        .Where(e => string.IsNullOrWhiteSpace(e.Criteria))
                        .Distinct()
                        .Select(e => string.IsNullOrWhiteSpace(e.Mask) ? defaultMask : MaskTree.GetMaskTree(MaskTree.Split(e.Mask)))
                        .Aggregate(MaskTree.BasicFieldsMaskTree(), (t1, t2) => t1.UnionWith(t2)); // we use a seed here since if the collection is empty this will throw an error

                    var criteriaWithIndexes = maskAndCriteriaArray
                        .Select((e, index) => new IndexAndCriteria { Criteria = e.Criteria, Index = index });

                    var criteriaMapList = await query.GetIndexToIdMap(criteriaWithIndexes);

                    // Go over the Ids in the result and apply all relevant masks to said DTO
                    var dtoForQueryType = typeof(TDtoForQuery);
                    var criteriaMapDictionary = criteriaMapList
                        .GroupBy(e => e.Id)
                        .ToDictionary(e => e.Key, e => e.ToList());

                    foreach (var dto in memoryList)
                    {
                        var id = dto.Id;
                        MaskTree mask;

                        if (criteriaMapDictionary.ContainsKey(id))
                        {
                            // Those are DTOs that satisfy one or more non-null Criteria
                            mask = criteriaMapDictionary[id]
                                .Select(e => maskAndCriteriaArray[e.Index].Mask)
                                .Aggregate((t1, t2) => t1.UnionWith(t2))
                                .UnionWith(universalMask);
                        }
                        else
                        {
                            // Those are DTOs that belong to the universal mask of null criteria
                            mask = universalMask;
                        }

                        MarkMask(dto, dtoForQueryType, mask);
                    }
                }

                // This where field-level security is applied, we read all masked DTOs and apply the
                // masks on them by setting the field to null and adjusting the metadata accordingly
                foreach (var pair in maskedDtos)
                {
                    var dto = pair.Key;
                    var accessibleFields = pair.Value;

                    List<Action> updates = new List<Action>(dto.EntityMetadata.Keys.Count);
                    foreach (var requestedField in dto.EntityMetadata.Keys)
                    {
                        if (!accessibleFields.Contains(requestedField))
                        {
                            // Mark the field as restricted (we delay the call to avoid the dreadful "collection-was-modified" Exception)
                            updates.Add(() => dto.EntityMetadata[requestedField] = FieldMetadata.Restricted);

                            // Set the field to null
                            var prop = dto.GetType().GetProperty(requestedField);
                            try
                            {
                                prop.SetValue(dto, null);
                            }
                            catch (Exception ex)
                            {
                                if (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
                                {
                                    // Programmer mistake
                                    throw new InvalidOperationException($"DTO field {prop.Name} has a non nullable type, all non-basic DTO fields must have a nullable type");
                                }
                                else
                                {
                                    throw ex;
                                }
                            }
                        }
                    }

                    updates.ForEach(a => a());
                }
            }
        }

        /// <summary>
        /// For every model in the list, the method will traverse the object graph and group all related
        /// models it can find (navigation properties) into a dictionary, after mapping them to their DTOs
        /// </summary>
        protected virtual Dictionary<string, IEnumerable<DtoBase>> FlattenRelatedEntitiesAndTrim(List<TDtoForQuery> models, string expand)
        {
            if (models == null || !models.Any())
            {
                return new Dictionary<string, IEnumerable<DtoBase>>();
            }

            var mainCollection = models.ToHashSet();

            // EF ensures that source DTOs for query are not duplicated, this dictionary ensures the same for mapped DTOs for read
            Dictionary<DtoBase, DtoBase> map = new Dictionary<DtoBase, DtoBase>();
            DtoBase Map(DtoBase dto)
            {
                if (!map.ContainsKey(dto))
                {
                    map[dto] = Mapper.Map<DtoBase>(dto);
                }

                return map[dto];
            }

            // An inline function that recursively traverses the dto tree, takes all navigation properties
            // that only exist in the ForQuery part and moves them to a flat collection
            void Flatten(DtoBase dto, DtoBase mappedDto, Type forQueryType, Type forReadType, HashSet<DtoBase> accRelatedModels)
            {
                if (dto == null)
                {
                    return;
                }

                var propertiesForRead = forReadType.GetProperties().ToDictionary(e => e.Name);
                foreach (var prop in forQueryType.GetProperties())
                {
                    if (!propertiesForRead.ContainsKey(prop.Name))
                    {
                        if (prop.GetValue(dto) is DtoBase relatedDto  /* This checks for null */
                            && !accRelatedModels.Contains(relatedDto) && !mainCollection.Contains(relatedDto))
                        {
                            // This property is only on the ForQuery side, make sure it is added to accRelatedModels
                            accRelatedModels.Add(relatedDto);

                            var mappedRelatedDto = Map(relatedDto);
                            Flatten(relatedDto, mappedRelatedDto, prop.PropertyType, mappedRelatedDto.GetType(), accRelatedModels);
                        }
                    }
                    else
                    {
                        // The corresponding property in ForRead
                        var mappedProp = propertiesForRead[prop.Name];

                        // Navigation property
                        if (prop.GetValue(dto) is DtoBase relatedDto)
                        {
                            var mappedRelatedDto = mappedProp.GetValue(mappedDto) as DtoBase;
                            Flatten(relatedDto, mappedRelatedDto, prop.PropertyType, mappedProp.PropertyType, accRelatedModels);
                        }

                        // Navigation collection
                        else if (prop.PropertyType.IsList())
                        {
                            var collection = prop.GetValue(dto);
                            if (collection != null)
                            {
                                var list = collection.Enumerate<DtoBase>().ToList();
                                var listType = prop.PropertyType.CollectionType();

                                var mappedList = mappedProp.GetValue(mappedDto).Enumerate<DtoBase>().ToList();
                                var mappedListType = mappedProp.PropertyType.CollectionType();

                                for (var i = 0; i < list.Count; i++)
                                {
                                    var line = list[i];
                                    var mappedLine = mappedList[i];

                                    Flatten(line, mappedLine, listType, mappedListType, accRelatedModels);
                                }
                            }
                        }
                    }
                }
            }

            var relatedDtos = new HashSet<DtoBase>();
            var dtoForQueryType = typeof(TDtoForQuery);
            var dtoForReadType = typeof(TDto);
            List<DtoBase> mappedModels = Mapper.Map<List<DtoBase>>(models);
            for (var i = 0; i < models.Count; i++)
            {
                var model = models[i];
                var mappedModel = mappedModels[i];
                Flatten(model, mappedModel, dtoForQueryType, dtoForReadType, relatedDtos);
            }

            var mappedRelatedDtos = relatedDtos.Select(e => Map(e));

            // This groups the related entities by collection name
            var result = mappedRelatedDtos.GroupBy(e => GetCollectionName(e.GetType()))
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return result;
        }

        /// <summary>
        /// Retrieves the collection name from the DTO type
        /// </summary>
        protected static string GetCollectionName(Type dtoType)
        {
            if (!_getCollectionNameCache.ContainsKey(dtoType))
            {
                string collectionName;
                var attribute = dtoType.GetCustomAttributes<StrongDtoAttribute>(inherit: true).FirstOrDefault();
                if (attribute != null)
                {
                    collectionName = attribute.CollectionName;
                }
                else
                {
                    collectionName = dtoType.Name;
                }

                _getCollectionNameCache[dtoType] = collectionName;
            }

            return _getCollectionNameCache[dtoType];
        }

        /// <summary>
        /// Transforms a DTO response into an abstract grid that can be transformed into an file
        /// </summary>
        protected abstract AbstractDataGrid DtosToAbstractGrid(GetResponse<TDto> response, ExportArguments args);

        protected ODataQuery<TDtoForQuery, TKey> CreateODataQuery()
        {
            var conn = GetDbContext().Database.GetDbConnection();
            var sources = GetSources();
            ODataQuery<TDtoForQuery, TKey> query = _odataFactory.MakeODataQuery<TDtoForQuery, TKey>(conn, sources);

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
