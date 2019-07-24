using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.OData;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
namespace BSharp.Controllers
{

    public abstract class ReadEntitiesControllerBase<TDto, TKey> : ReadControllerBase<TDto>
        where TDto : DtoKeyBase<TKey>
    {
        // Private Fields
        private readonly ILogger _logger;

        // Constructor
        public ReadEntitiesControllerBase(ILogger logger, IStringLocalizer localizer, IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _logger = logger;
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
            string permissionsCriteria = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsCriteria);

            // Take a copy of the query without the expand or the select
            var qClone = query.Clone();

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            query.Expand(args.Expand);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            query.Select(args.Select);

            // Load
            var result = await query.FirstOrDefaultAsync();
            if (result == null)
            {
                // We already checked for not found earlier,
                // This can only mean lack of permissions
                throw new ForbiddenException();
            }

            var collectionName = GetCollectionName(typeof(TDto));

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var singleton = new List<TDto> { result };
            await ApplyReadPermissionsMask(singleton, collectionName, qClone, permissions, GetDefaultMask());

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(singleton, args.Expand);

            // Return
            return new GetByIdResponse<TDto>
            {
                Result = result,
                CollectionName = collectionName,
                RelatedEntities = relatedEntities
            };
        }

        protected async Task<EntitiesResponse<TDto>> GetByIdListAsync(TKey[] ids, string expand, System.Data.Common.DbTransaction trx = null)
        {
            // Prepare a query of the result, and clone it
            var query = CreateODataQuery();
            if (trx != null)
            {
                query.UseTransaction(trx);
            }

            query.FilterByIds(ids);
            var qClone = query.Clone();

            // Expand the result as specified in the OData agruments and load into memory
            query.Expand(expand);
            query.OrderBy("Id"); // Required
            var result = await query.ToListAsync(); // this is potentially unordered, should that be a concern?

            // Apply the permissions on the result
            var permissions = await UserPermissions(PermissionLevel.Read);
            var defaultMask = GetDefaultMask();
            var collectionName = GetCollectionName(typeof(TDto));
            await ApplyReadPermissionsMask(result, collectionName, qClone, permissions, defaultMask);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(result, expand);

            // Sort the entities according to the original Ids, as a good practice
            TDto[] sortedResult = new TDto[ids.Length];
            Dictionary<TKey, TDto> affectedEntitiesDic = result.ToDictionary(e => e.Id);
            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                TDto entity = null;
                if (affectedEntitiesDic.ContainsKey(id))
                {
                    entity = affectedEntitiesDic[id];
                }

                sortedResult[i] = entity;
            }

            // Prepare the result in a response object
            return new EntitiesResponse<TDto>
            {
                Result = sortedResult,
                RelatedEntities = relatedEntities,
                CollectionName = collectionName
            };
        }

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
            //    var eParam = Expression.Parameter(typeof(TDto));

            //    // Prepare the lambda
            //    Expression whereClause = ToORedWhereClause<TDto>(criteriaList, eParam);
            //    var lambda = Expression.Lambda<Func<TDto, bool>>(whereClause, eParam);

            //    await CheckPermissionsForOld(entityIds, lambda);
            //}
        }

        protected override string DefaultOrderBy()
        {
            return "Id desc";
        }


        /// <summary>
        /// If the user is subject to field-level access control, this method hides all the fields
        /// that the user has no access to and modifies the metadata of the DTOs accordingly
        /// </summary>
        protected override async Task ApplyReadPermissionsMask(
            List<TDto> resultEntities,
            string collectionName,
            ODataQuery<TDto> query,
            IEnumerable<AbstractPermission> permissions,
            MaskTree defaultMask)
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
                    var dtoType = typeof(TDto);
                    foreach (var item in resultEntities)
                    {
                        MarkMask(item, dtoType, maskUnion);
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

                    var criteriaMapList = await query.GetIndexToIdMap<TKey>(criteriaWithIndexes);

                    // Go over the Ids in the result and apply all relevant masks to said DTO
                    var dtoType = typeof(TDto);
                    var criteriaMapDictionary = criteriaMapList
                        .GroupBy(e => e.Id)
                        .ToDictionary(e => e.Key, e => e.ToList());

                    foreach (var dto in resultEntities)
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

                        MarkMask(dto, dtoType, mask);
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
    }
}
