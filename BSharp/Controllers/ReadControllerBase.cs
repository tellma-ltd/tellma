using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.FilterParser;
using BSharp.Services.ImportExport;
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
        private readonly IFilterParser _filterParser;
        protected static ConcurrentDictionary<Type, string> _getCollectionNameCache = new ConcurrentDictionary<Type, string>(); // This cache never expires

        protected IMapper Mapper { get; }

        // Constructor
        public ReadControllerBase(ILogger logger, IStringLocalizer localizer, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _localizer = localizer;
            _filterParser = serviceProvider.GetRequiredService<IFilterParser>();
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
            // Get a readonly query
            IQueryable<TDtoForQuery> query = GetBaseQuery().AsNoTracking();

            // Retrieve the user permissions for the current view
            var permissions = await UserPermissions(PermissionLevel.Read);

            // Filter out permissions with masks that would be violated by the filter or order by arguments
            var defaultMask = GetDefaultMask() ?? new MaskTree();
            permissions = FilterViolatedPermissions(permissions, defaultMask, args.Filter, args.OrderBy);

            // Apply read permissions
            query = await ApplyReadPermissionsCriteria(query, permissions);

            // Include inactive
            query = IncludeInactive(query, inactive: args.Inactive);

            // Search
            query = Search(query, args.Search, permissions);

            // Filter
            query = Filter(query, args.Filter);

            // Before ordering or paging, retrieve the total count
            bool supportsAsync = query is IAsyncEnumerable<TDtoForQuery>;
            int totalCount = supportsAsync ? await query.CountAsync() : query.Count();

            // OrderBy
            query = OrderBy(query, args.OrderBy, args.Desc);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Take(top);

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            var expandedQuery = Expand(query, args.Expand);

            // Load the data in memory
            var memoryList = supportsAsync ? await expandedQuery.ToListAsync() : expandedQuery.ToList();

            // Apply the select argument and set the metadata (regardless of permissions)
            ApplySelectAndAddMetadata(memoryList, args.Expand, args.Select);

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(memoryList, query, permissions, defaultMask);

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
                Desc = args.Desc,
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
            // Single by Id
            var query = GetBaseQuery().Where(e => e.Id.Equals(id)).AsNoTracking();

            // Check that the entity exists
            bool supportsAsync = query is IAsyncEnumerable<TDtoForQuery>;
            int count = supportsAsync ? await query.CountAsync() : query.Count();
            if (count == 0)
            {
                throw new NotFoundException<TKey>(id);
            }

            // Apply read permissions
            var permissions = await UserPermissions(PermissionLevel.Read);
            query = await ApplyReadPermissionsCriteria(query, permissions);

            // Expand
            var expandedQuery = Expand(query, args.Expand);

            // Load
            var dtoForQuery = supportsAsync ? await expandedQuery.FirstOrDefaultAsync() : expandedQuery.FirstOrDefault();
            if (dtoForQuery == null)
            {
                // We already checked for not found earlier,
                // This can only mean lack of permissions
                throw new ForbiddenException();
            }

            // Apply the select argument and set the metadata (regardless of permissions)
            var singleton = new List<TDtoForQuery> { dtoForQuery };
            ApplySelectAndAddMetadata(singleton, args.Expand, args.Select);

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(singleton, query, permissions, GetDefaultMask());

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
        /// Get the IQueryable source on which the controller is based
        /// </summary>
        /// <returns></returns>
        protected abstract IQueryable<TDtoForQuery> GetBaseQuery();

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
        protected IEnumerable<AbstractPermission> FilterViolatedPermissions(IEnumerable<AbstractPermission> permissions, MaskTree defaultMask, string filter, string orderBy)
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
                var filterPaths = _filterParser.ExtractPaths(filter);
                var filterMask = MaskTree.GetMaskTree(filterPaths);
                var filterAccess = Normalize(filterMask);

                userMask = userMask.UnionWith(filterAccess);
            }

            if (!string.IsNullOrEmpty(orderBy))
            {
                var orderbyPaths = MaskTree.Split(orderBy);
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
        protected virtual Task<IQueryable<TDtoForQuery>> ApplyReadPermissionsCriteria(IQueryable<TDtoForQuery> query, IEnumerable<AbstractPermission> permissions)
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

                // The parameter on which the expression is based
                var eParam = Expression.Parameter(typeof(TDtoForQuery));
                var whereClause = ToORedWhereClause<TDtoForQuery>(criteriaList, eParam);
                var lambda = Expression.Lambda<Func<TDtoForQuery, bool>>(whereClause, eParam);

                query = query.Where(lambda);
            }

            return Task.FromResult(query);
        }

        /// <summary>
        /// Includes or excludes inactive items from the query depending on the boolean switch supplied
        /// </summary>
        protected abstract IQueryable<TDtoForQuery> IncludeInactive(IQueryable<TDtoForQuery> query, bool inactive);

        /// <summary>
        /// Applies the search argument, which is handled differently in every controller
        /// </summary>
        protected abstract IQueryable<TDtoForQuery> Search(IQueryable<TDtoForQuery> query, string search, IEnumerable<AbstractPermission> filteredPermissions);

        /// <summary>
        /// Filters the query based on the filter argument, the default implementation 
        /// assumes OData-like syntax
        /// </summary>
        protected virtual IQueryable<TDtoForQuery> Filter(IQueryable<TDtoForQuery> query, string filter)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                // The parameter on which the expression is based
                var eParam = Expression.Parameter(typeof(TDtoForQuery));
                var expression = _filterParser.ParseFilterExpression<TDtoForQuery>(filter, eParam);
                var lambda = Expression.Lambda<Func<TDtoForQuery, bool>>(expression, eParam);
                query = query.Where(lambda);
            }

            return query;
        }

        /// <summary>
        /// Orders the query as per the orderby and desc arguments
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query">The base query to order</param>
        /// <param name="orderby">The orderby parameter which has the format 'A/B/C'</param>
        /// <param name="desc">True for a descending order</param>
        /// <returns>Ordered query</returns>
        protected virtual IQueryable<TDtoForQuery> OrderBy(IQueryable<TDtoForQuery> query, string orderby, bool desc)
        {
            Type modelType = typeof(TDtoForQuery);
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                var steps = orderby.Split('/');

                // Validate that the steps represent a valid train of navigation properties
                {
                    PropertyInfo prop = null;
                    Type propType = modelType;
                    foreach (var step in steps)
                    {
                        prop = propType.GetProperty(step);
                        if (prop == null)
                        {
                            // Programmer mistake
                            throw new InvalidOperationException(
                                $"The property '{step}' is not a navigation property of entity type '{propType.Name}'. " +
                                $"The orderby parameter should have the general format: 'orderby=A/B'");
                        }

                        var isList = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                        if (isList)
                        {
                            // Programmer mistake
                            throw new InvalidOperationException(
                                $"The property '{step}' is a collection navigation property of type '{propType.Name}'. " +
                                $"Collection properties cannot be used in the orderby argument");
                        }

                        propType = prop.PropertyType;
                    }
                }

                // Create the key selector dynamically using LINQ expressions and apply it on the query
                {
                    var param = Expression.Parameter(modelType);
                    Expression exp = param;
                    Type propType = modelType;
                    foreach (var step in steps)
                    {
                        var prop = propType.GetProperty(step);
                        propType = prop.PropertyType;
                        exp = Expression.Property(exp, prop);

                        if (step == steps[steps.Length - 1])
                        {
                            exp = Expression.Convert(exp, typeof(object)); // To handle unboxing of e.g. int members
                        }
                    }

                    var keySelector = Expression.Lambda<Func<TDtoForQuery, object>>(exp, param);

                    // Order the query taking into account the "isDescending" parameter
                    query = desc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
                }
            }
            else
            {
                query = DefaultOrder(query);
            }

            return query;
        }

        /// <summary>
        /// Applies the default order which is over "Id" property descending
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual IQueryable<TDtoForQuery> DefaultOrder(IQueryable<TDtoForQuery> query)
        {
            return query.OrderByDescending(e => e.Id);
        }

        /// <summary>
        /// Specifies the maximum page size to be returned by GET, defaults to <see cref="DEFAULT_MAX_PAGE_SIZE"/>
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DEFAULT_MAX_PAGE_SIZE;
        }

        /// <summary>
        /// Includes in the query all navigation properties specified in the expand parameter
        /// </summary>
        /// <param name="query">The base query on which to include related properties</param>
        /// <param name="expand">The expand parameter which has the format 'A,B/C,D''</param>
        /// <returns>Expanded query</returns>
        protected virtual IQueryable<TDtoForQuery> Expand(IQueryable<TDtoForQuery> query, string expand)
        {
            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            if (!string.IsNullOrWhiteSpace(expand))
            {
                var paths = expand.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrWhiteSpace(e));
                Type modelType = typeof(TDtoForQuery);
                foreach (var path in paths)
                {
                    // Validate each step in the path
                    {
                        var steps = path.Split('/');
                        PropertyInfo prop = null;
                        Type propType = modelType;
                        foreach (var step in steps)
                        {
                            prop = propType.GetProperty(step);
                            if (prop == null)
                            {
                                throw new InvalidOperationException(
                                    $"The property '{step}' is not a navigation property of entity type '{propType.Name}'. " +
                                    $"The expand argument should have the general format: 'Expand=A,B/C,D'");
                            }

                            var isList = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                            propType = isList ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;
                        }
                    }

                    // Include
                    {
                        var includePath = path.Replace("/", ".");
                        query = query.Include(includePath);
                    }
                }
            }

            return query;
        }

        /// <summary>
        /// Applies the select argument and sets the fields of the dtos in the memory 
        /// list according to the user request without regard to field-level user permissions,
        /// those are applied in a later step
        /// </summary>
        protected virtual void ApplySelectAndAddMetadata(List<TDtoForQuery> memoryList, string expand, string select)
        {
            /* 
             * The DTO Metadata is a small dictionary attached to every DTO in the response, this dictionary 
             * specifies for every field on the DTO whether the field is 
             * - Loaded: Requested by the API call and user permissions allow access to it
             * - Restricted: Requested by the API call but the user permissions do not allow access to it
             * - Not Loaded: Not requested at all, such fields are not mentioned in the metadata
             * 
             * The method implementation below traverses the DTO tree 3 times:
             * (1) First time sets the metadata of every DTO in the tree according to the expand argument alone
             * (2) Second time adjusts the metadata of every DTO in the tree according to the select argument
             * (3) Third time examines the metadata in every DTO so far and every property that is not in the metadata is set to null
             * 
             * This method produces a result as if the user has unrestricted access to all data s/he is requesting
             * A later method goes over the DTOs again and removes all restricted fields and adjusts the metadata accordingly
             * 
             */

            var dtoForQueryType = typeof(TDtoForQuery);
            HashSet<DtoBase> allAreLoaded = new HashSet<DtoBase>();
            void ApplyExpandMetadata(DtoBase dto, Type dtoType, IEnumerable<string> path)
            {
                if (dto == null)
                {
                    return;
                }
                else
                {
                    // Any property touched by an Expand path, will have all its properties loaded by default
                    // unless it was later touched by a Select path
                    if (!allAreLoaded.Contains(dto))
                    {
                        // Mark it, and later we set all the properties
                        allAreLoaded.Add(dto);
                    }

                    if (path.Count() > 0)
                    {
                        // Note: the expand argument at this point has already been validated in the expand step
                        // so we can safely use the steps without another round of validation
                        var step = path.First();
                        var remainingPath = path.Skip(1);
                        dto.EntityMetadata[step] = FieldMetadata.Loaded;
                        var prop = dtoType.GetProperty(step);

                        if (prop == null)
                        {
                            // Programmer mistake
                            throw new BadRequestException($"Property {step} does not exist on type {dtoType.Name}");
                        }
                        else if (prop.PropertyType.IsList())
                        {
                            // This is a navigation collection, iterate over the rows
                            var collection = prop.GetValue(dto);
                            if (collection != null)
                            {
                                var collectionType = prop.PropertyType.CollectionType();
                                foreach (var row in collection.Enumerate<DtoBase>())
                                {
                                    ApplyExpandMetadata(row, collectionType, remainingPath);
                                }
                            }
                        }
                        else
                        {
                            var foreignKeyNameAtt = prop.GetCustomAttribute<NavigationPropertyAttribute>();
                            if (foreignKeyNameAtt == null)
                            {
                                // Programmer mishap
                                throw new InvalidOperationException($"Navigation property '{prop.Name}' on type '{dtoType.Name}' should be adorned with a NavigationProperty attribute");
                            }
                            else
                            {
                                string foreignKeyName = foreignKeyNameAtt.ForeignKey;
                                if (!string.IsNullOrWhiteSpace(foreignKeyName))
                                {
                                    var foreignKeyProp = dtoType.GetProperty(foreignKeyName);
                                    if (foreignKeyProp == null)
                                    {
                                        // Programmer mistake
                                        throw new InvalidOperationException($"Navigation property '{prop.Name}' on type '{dtoType.Name}' is adorned with a foreign key property name '{foreignKeyName}' that doesn't exist");
                                    }
                                    else if (foreignKeyProp.GetCustomAttribute<ForeignKeyAttribute>() == null)
                                    {
                                        // Programmer mistake
                                        throw new InvalidOperationException($"Foreign key property '{foreignKeyProp.Name}' on type '{dtoType.Name}' should be adorned with a ForeignKey attribute");
                                    }
                                    else
                                    {
                                        // Whenever a navigation property is loaded we make its corresponding foreign key loaded as well
                                        dto.EntityMetadata[foreignKeyName] = FieldMetadata.Loaded;
                                    }
                                }
                            }

                            // This is a normal navigation property
                            var propValue = prop.GetValue(dto) as DtoBase;
                            var propType = prop.PropertyType;
                            ApplyExpandMetadata(propValue, propType, remainingPath);
                        }
                    }
                }
            }

            expand = expand ?? "";
            {
                var paths = expand
                    .Split(',')
                    .Select(e => e?.Trim())
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Distinct()
                    .Select(path => path.Split('/').Select(e => e?.Trim())).ToList();

                // The empty path representing the principle entities is always present
                paths.Add(new string[] { });

                foreach (var dto in memoryList)
                {
                    foreach (var path in paths)
                    {
                        ApplyExpandMetadata(dto, dtoForQueryType, path);
                    }
                }
            }

            HashSet<DtoBase> dtosWithSetBasicFields = new HashSet<DtoBase>();
            void SetBasicField(DtoBase dto, Type dtoType)
            {
                // A select path specifies certain fields on this DTO, therefore not all the fields will be included anymore
                if (allAreLoaded.Contains(dto))
                {
                    allAreLoaded.Remove(dto);
                }

                // Always add the basic fields to the DTO metadata
                if (!dtosWithSetBasicFields.Contains(dto))
                {
                    // Add basic fields if they are not already added
                    foreach (var basicField in dtoType.BasicFields())
                    {
                        dto.EntityMetadata[basicField.Name] = FieldMetadata.Loaded;
                    }

                    // Mark the DTO as an optimization
                    dtosWithSetBasicFields.Add(dto);
                }
            }

            void ApplySelectMetadata(DtoBase dto, Type dtoType, IEnumerable<string> path)
            {
                if (dto == null || path.Count() == 0)
                {
                    return;
                }

                var step = path.First();
                var remainingSteps = path.Skip(1);
                if (step == MaskTree.BASIC_FIELDS_KEYWORD)
                {
                    if (remainingSteps.Count() > 0)
                    {
                        string message = _localizer["Error_BasicFieldsKeyword0ShouldComeEndOfPath", MaskTree.BASIC_FIELDS_KEYWORD];
                        throw new BadRequestException(message);
                    }

                    SetBasicField(dto, dtoType);
                }
                else
                {
                    var prop = dtoType.GetProperty(step);
                    if (prop == null)
                    {
                        throw new BadRequestException(_localizer["Error_Property0DoesNotExistOnType1", step, dtoType.Name]);
                    }
                    else if (prop.IsNavigationField())
                    {
                        if (!dto.EntityMetadata.ContainsKey(step))
                        {
                            string message = _localizer["Error_NavigationField0InSelectMustAlsoBeInExpand", prop.Name];
                            throw new BadRequestException(message);
                        }

                        // If there are more steps: keep going
                        else if (remainingSteps.Count() > 0) // Just an optimization
                        {
                            if (prop.PropertyType.IsList())
                            {
                                // This is a navigation collection, iterate over the rows
                                var collection = prop.GetValue(dto);
                                if (collection != null)
                                {
                                    var collectionType = prop.PropertyType.CollectionType();
                                    foreach (var row in collection.Enumerate<DtoBase>())
                                    {
                                        ApplySelectMetadata(row, collectionType, remainingSteps);
                                    }
                                }
                            }
                            else
                            {
                                // This is a normal navigation property
                                var propValue = prop.GetValue(dto) as DtoBase;
                                var propType = prop.PropertyType;
                                ApplySelectMetadata(propValue, propType, remainingSteps);
                            }
                        }
                    }
                    else
                    {
                        if (remainingSteps.Count() > 0)
                        {
                            string message = _localizer["Error_Field0OnType1IsNotANavigationField", prop.Name, dtoType.Name];
                            throw new BadRequestException(message);
                        }
                        else if (prop.IsForeignKey())
                        {
                            string message = _localizer["Error_ForeignKeys1eCannotBeUsedInSelectArgument", prop.Name];
                            throw new BadRequestException(message);
                        }
                        else
                        {
                            SetBasicField(dto, dtoType);

                            // Add the field to metadata if it is not the basic fields keyword
                            if (step != MaskTree.BASIC_FIELDS_KEYWORD)
                            {
                                dto.EntityMetadata[step] = FieldMetadata.Loaded;
                            }
                        }
                    }
                }
            }

            if (select != null)
            {
                var paths = select
                    .Split(',')
                    .Select(e => e?.Trim())
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(path => path.Split('/').Select(e => e?.Trim()));

                foreach (var dto in memoryList)
                {
                    foreach (var path in paths)
                    {
                        ApplySelectMetadata(dto, dtoForQueryType, path);
                    }
                }
            }

            // DTOs with all their properties added will have their metadata specified here
            foreach (var dto in allAreLoaded)
            {
                // Add non navigation properties
                foreach (var prop in dto.GetType().GetProperties())
                {
                    if (!prop.IsNavigationField() && !prop.IsForeignKey() && !prop.PropertyType.IsList() && !prop.IsIgnored())
                    {
                        dto.EntityMetadata[prop.Name] = FieldMetadata.Loaded;
                    }
                }
            }


            void ApplySelect(DtoBase dto, Type dtoType)
            {
                if (dto == null)
                {
                    return;
                }

                foreach (var prop in dtoType.GetProperties())
                {
                    if (prop.IsIgnored())
                    {
                        continue;
                    }
                    else if (!dto.EntityMetadata.ContainsKey(prop.Name))
                    {
                        // This means the property was not selected by the API caller, so set it to null
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
                    else
                    {
                        if (prop.PropertyType.IsList())
                        {
                            // This is a navigation collection, iterate over the rows
                            var collection = prop.GetValue(dto);
                            if (collection != null)
                            {
                                var collectionType = prop.PropertyType.CollectionType();
                                foreach (var row in collection.Enumerate<DtoBase>())
                                {
                                    ApplySelect(row, collectionType);
                                }
                            }
                        }
                        else
                        {
                            if (prop.IsNavigationField())
                            {
                                // This is a normal navigation property
                                var propValue = prop.GetValue(dto) as DtoBase;
                                var propType = prop.PropertyType;
                                ApplySelect(propValue, propType);
                            }
                        }
                    }
                }
            }

            foreach (var dto in memoryList)
            {
                ApplySelect(dto, dtoForQueryType);
            }
        }

        /// <summary>
        /// If the user is subject to field-level access control, this method hides all the fields
        /// that the user has no access to and modifies the metadata of the DTOs accordingly
        /// </summary>
        protected virtual async Task ApplyReadPermissionsMask(List<TDtoForQuery> memoryList, IQueryable<TDtoForQuery> query, IEnumerable<AbstractPermission> permissions, MaskTree defaultMask)
        {
            if (permissions.All(e => string.IsNullOrWhiteSpace(e.Mask)) && (defaultMask == null || defaultMask.IsUnrestricted))
            {
                // Optimization: if all masks are unrestricted, then we can skip this whole ordeal
                return;
            }
            else
            {

                // Maps every DTO to its list of masked
                Dictionary<DtoBase, HashSet<string>> maskedDtos = new Dictionary<DtoBase, HashSet<string>>();
                HashSet<DtoBase> unrestrictedDtos = new HashSet<DtoBase>();

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

                    // For every criteria create a query q.Where(criteria). And select the Dto Id and the criteria Index. And put all the queries in a list
                    var criteriaQueryList = maskAndCriteriaArray
                        .Select((maskAndCriteria, index) =>
                        {
                            var eParam = Expression.Parameter(typeof(TDtoForQuery));
                            var criteriaExpression = _filterParser.ParseFilterExpression<TDtoForQuery>(maskAndCriteria.Criteria, eParam);
                            var whereClause = Expression.Lambda<Func<TDtoForQuery, bool>>(criteriaExpression, eParam);
                            return query.Where(whereClause).Select(e => new CriteriaMap { Id = e.Id, Index = index });
                        });

                    // Union all the queries together and load the result in memory
                    var criteriaMapList = await criteriaQueryList
                        .Aggregate((q1, q2) => q1.Union(q2)) // Unions all the queries together
                        .ToListAsync();

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
            if(models == null || !models.Any())
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

        protected Expression ToORedWhereClause<T>(IEnumerable<string> criteriaList, ParameterExpression eParam)
        {
            return criteriaList.Select(criteria => _filterParser.ParseFilterExpression<T>(criteria, eParam))
                .Aggregate((exp1, exp2) => Expression.OrElse(exp1, exp2));
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

        /// <summary>
        /// Useful for determining which dtos satisfy the criteria of which permissions in order to apply the masks of those permissions on the DTOs
        /// </summary>
        protected class CriteriaMap
        {
            public int Index { get; set; }
            public TKey Id { get; set; }
        }
    }
}
