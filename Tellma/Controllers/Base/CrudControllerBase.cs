using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Controllers.Dto;
using Tellma.Controllers.ImportExport;
using Tellma.Controllers.Utilities;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Entities.Descriptors;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id, as well as updating, deleting and importing lists
    /// of that entity
    /// </summary>
    public abstract class CrudControllerBase<TEntityForSave, TEntity, TKey> : FactGetByIdControllerBase<TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        public CrudControllerBase(IServiceProvider sp) : base(sp)
        {
        }

        // HTTP Methods

        [HttpPost]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> Save([FromBody] List<TEntityForSave> entities, [FromQuery] SaveArguments args)
        {
            // Note here we use lists https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netcore-2.1
            // since the order is semantically relevant for reporting validation errors

            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Basic sanity check, to prevent null entities
                if (entities == null && !ModelState.IsValid)
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest("Body was empty");
                    } 
                    else
                    {
                        return UnprocessableEntity(ModelState);
                    }
                }

                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetCrudService();
                var (data, extras) = await service.Save(entities, args);

                await OnSuccessfulSave(data, extras);

                // Transform it and return the result
                var returnEntities = args?.ReturnEntities ?? false;
                if (returnEntities)
                {
                    // Transform the entities as an EntitiesResponse
                    var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                    // Return the response
                    return Ok(response);
                }
                else
                {
                    // Return 200
                    return Ok();
                }
            }, _logger);
        }

        [HttpDelete]
        public virtual async Task<ActionResult> Delete([FromQuery] List<TKey> i)
        {
            // "i" parameter is given a short name to allow a large number of
            // ids to be passed in the query string before the url size limit
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                await service.Delete(ids: i);

                await OnSuccessfulDelete(ids: i);

                return Ok();
            }, _logger);
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult> DeleteId(TKey id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var ids = new List<TKey> { id };
                var service = GetCrudService();
                await service.Delete(ids);

                await OnSuccessfulDelete(ids: ids);

                return Ok();
            }, _logger);
        }

        // Helpers

        protected override FactGetByIdServiceBase<TEntity, TKey> GetFactGetByIdService()
        {
            return GetCrudService();
        }

        protected abstract CrudServiceBase<TEntityForSave, TEntity, TKey> GetCrudService();

        /// <summary>
        /// Gives an opportunity for implementations to add headers to the response if a save was successful,
        /// useful to set x-version headers for controllers that cause changes that invalidate the cache
        /// </summary>
        protected virtual Task OnSuccessfulSave(List<TEntity> data, Extras extras)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gives an opportunity for implementations to add headers to the response if a delete was successful,
        /// useful to set x-version headers for controllers that cause changes that invalidate the cache
        /// </summary>
        protected virtual Task OnSuccessfulDelete(List<TKey> ids)
        {
            return Task.CompletedTask;
        }

        [HttpPost("import"), RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        public async Task<ActionResult<ImportResult>> Import([FromQuery] ImportArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                IFormFile formFile = Request.Form.Files.FirstOrDefault();
                var contentType = formFile?.ContentType;
                var fileName = formFile?.FileName;
                using var fileStream = formFile?.OpenReadStream();

                var service = GetCrudService();
                var result = await service.Import(fileStream, fileName, contentType, args);

                return Ok(result);
            }, _logger);
        }

        [HttpGet("template")]
        public async Task<ActionResult> CsvTemplate()
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                Stream template = service.CsvTemplate();

                return await Task.FromResult(File(template, MimeTypes.Csv));
            }, _logger);
        }

        [HttpGet("export")]
        public async Task<ActionResult> Export([FromQuery] ExportArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                Stream fileStream = await service.Export(args, cancellation);

                return File(fileStream, MimeTypes.Csv);
            }, _logger);
        }

        // TODO: Move to FactControllerBase
        [HttpGet("export-by-ids")]
        public async Task<ActionResult> ExportByIds([FromQuery] ExportByIdsArguments<TKey> args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                Stream fileStream = await service.ExportByIds(args, cancellation);

                return File(fileStream, MimeTypes.Csv);
            }, _logger);
        }
    }

    public abstract class CrudServiceBase<TEntityForSave, TEntity, TKey> : FactGetByIdServiceBase<TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        private readonly MetadataProvider _metadata;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IServiceProvider _sp;

        public CrudServiceBase(IServiceProvider sp) : base(sp)
        {
            _metadata = sp.GetRequiredService<MetadataProvider>();
            _tenantIdAccessor = sp.GetRequiredService<ITenantIdAccessor>();
            _sp = sp;
        }

        protected virtual int? DefinitionId => null;

        #region Save

        /// <summary>
        /// Saves the entities (Insert or Update) into the database after authorization and validation.
        /// </summary>
        /// <returns>Optionally returns the same entities in their persisted READ form.</returns>
        public virtual async Task<(List<TEntity>, Extras)> Save(List<TEntityForSave> entities, SaveArguments args)
        {
            try
            {
                // Trim all strings as a preprocessing step
                entities.ForEach(e => TrimStringProperties(e));

                // Check that any updated Ids are 
                FilterExpression updateFilter = await CheckUpdatePermissionBefore(entities);

                // Validate
                // Check that non-null non-0 Ids are unique
                ControllerUtilities.ValidateUniqueIds(entities, ModelState, _localizer);

                // Basic Validation (before preprocessing)
                var meta = GetMetadataForSave();
                ValidateList(entities, meta);
                ModelState.ThrowIfInvalid();

                // Start a transaction scope for save since it causes data modifications
                using var trx = ControllerUtilities.CreateTransaction(null, GetSaveTransactionOptions());

                // Optional preprocessing
                await SavePreprocessAsync(entities);

                // Custom validation
                await SaveValidateAsync(entities);
                ModelState.ThrowIfInvalid();

                // Save and retrieve Ids
                var returnEntities = args?.ReturnEntities ?? false;
                var ids = await SaveExecuteAsync(entities, returnEntities || updateFilter != null);

                // Load the entities (using the update permissions to check for RLS)
                List<TEntity> data = null;
                Extras extras = null;
                if (returnEntities)
                {
                    (data, extras) = await GetByIds(ids, args, Constants.Update, cancellation: default);
                }

                // Check that the saved entities satisfy the user's row level security filter
                await CheckActionPermissionsAfter(updateFilter, ids, data);

                // Perform side effects of save that are not transactional, just before committing the transaction
                await NonTransactionalSideEffectsForSave(entities, data);

                // Commit and return
                await OnSaveCompleted();
                trx.Complete();

                return (data, extras);
            }
            catch (Exception ex)
            {
                await OnSaveError(ex);
                throw;
            }
        }

        /// <summary>
        /// Optional preprocessing of entities before they are validated and saved
        /// </summary>
        protected virtual Task<List<TEntityForSave>> SavePreprocessAsync(List<TEntityForSave> entities)
        {
            return Task.FromResult(entities);
        }

        /// <summary>
        /// Performs server side validation on the entities, this method is expected to 
        /// call AddModelError on the controller's ModelState if there is a validation problem,
        /// the method should NOT do validation that is already handled by validation attributes.
        /// Note: Don't check for unique Ids as this is already taken care of
        /// </summary>
        protected abstract Task SaveValidateAsync(List<TEntityForSave> entities);

        /// <summary>
        /// Persists the entities in the database, either creating them or updating, the call to this method is already wrapped inside a transaction
        /// </summary>
        protected abstract Task<List<TKey>> SaveExecuteAsync(List<TEntityForSave> entities, bool returnIds);

        /// <summary>
        /// Any save side effects to Save that are not transactional (such as saving to Blob storage or sending emails) should be implemented in this method,
        /// this method is the last step before committing the save transaction, so an error here is the last opportunity to roll back the transaction
        /// </summary>
        protected virtual Task NonTransactionalSideEffectsForSave(List<TEntityForSave> entities, List<TEntity> data)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves the <see cref="TransactionOptions"/> that are used in <see cref="Save(List{TEntityForSave}, SaveArguments)"/>,
        /// there is a default implementation that uses <see cref="IsolationLevel.ReadCommitted"/> and a timeout of 5 minutes
        /// </summary>
        protected virtual TransactionOptions GetSaveTransactionOptions()
        {
            return new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = ControllerUtilities.DefaultTransactionTimeout()
            };
        }

        /// <summary>
        /// Invoked just before committing the Save transaction and returning the result, errors thrown here will roll back the save transactions
        /// </summary>
        protected virtual Task OnSaveCompleted()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when an error occurs in the save pipleline, good place to perform cleaning up of resources
        /// </summary>
        protected virtual Task OnSaveError(Exception ex)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Trims all string properties of the entity
        /// </summary>
        protected virtual void TrimStringProperties(TEntityForSave entity)
        {
            entity.TrimStringProperties();
        }

        /// <summary>
        /// Verifies that the user has the necessary permissions to save the givenentities, if all user permissions
        /// have RLS criteria, the method returns true indicating that an AfterSave check is also required.
        /// </summary>
        /// <param name="entities">The entities being saved</param>
        /// <returns>True if post save check is required</returns>
        private async Task<FilterExpression> CheckUpdatePermissionBefore(List<TEntityForSave> entities)
        {
            if (entities == null || !entities.Any())
            {
                // No point verifying anything
                return null; 
            }

            var updateFilter = await UserPermissionsFilter(Constants.Update, cancellation: default);
            if (updateFilter == null)
            {
                // User has unfiltered update permission on the table => Cleared to proceed, no post check required
                return null;
            } 
            else
            {                
                // First determine the entities that are being UPDATED
                IEnumerable<object> updatedIds;
                if (typeof(TKey) == typeof(int))
                {
                    // For integer Ids, zero means INSERT, non-zero means UPDATE, can be determined in memory
                    object zero = 0;
                    updatedIds = entities
                        .Where(e => e.GetId() != null && !e.GetId().Equals(zero))
                        .Select(e => e.GetId());
                }
                else
                {
                    // For string Ids, we can only distinguish INSERT from UPDATE by consulting the database, luckily string Ids are rare
                    var allIds = entities.Select(e => e.GetId());
                    var dbEntities = await GetRepository()
                                    .Query<TEntity>()
                                    .Select("Id")
                                    .FilterByIds(allIds)
                                    .ToListAsync(cancellation: default);

                    updatedIds = dbEntities
                        .Select(e => e.GetId());
                }

                var updatedIdsCount = updatedIds.Count();
                if (updatedIdsCount == 0)
                {
                    // Only INSERT, those are only verified later after saving is completed and before the transaction is committed
                    return updateFilter;
                }

                // Now a single database query should tell us if the updated entities in the DB satisfy the user's permission filters
                var baseQuery = GetRepository()
                                .Query<TEntity>()
                                .FilterByIds(updatedIds);

                var updatableIdsCount = await baseQuery
                                .Filter(updateFilter)
                                .CountAsync(cancellation: default);

                if (updatableIdsCount == updatedIdsCount)
                {
                    // All updated DB entities satisfy the filter => good to go, but a post check is required
                    return updateFilter;
                }
                else
                {
                    // Check that all Ids are readable
                    var readFilter = await UserPermissionsFilter(Constants.Read, cancellation: default);

                    var readableIds = await baseQuery
                                .Select("Id")
                                .Filter(readFilter)
                                .ToListAsync(cancellation: default);

                    if (readableIds.Count == updatedIdsCount)
                    {
                        // All Ids exist and the user can see them, throw a 403 Forbidden
                        throw new ForbiddenException();
                    }
                    else
                    {
                        var readableIdsHash = readableIds.Select(e => e.GetId()).ToHashSet();
                        var missingUpdatedIds = updatedIds.Cast<TKey>().Where(id => !readableIdsHash.Contains(id));

                        // At least one Id does not exist, or the user cannot see it, throw a 404 Not Found
                        throw new NotFoundException<TKey>(missingUpdatedIds);
                    }
                }
            }
        }

        #region Mask Stuff

        //        private async Task<List<TEntityForSave>> ApplyUpdatePermissionsMask(List<TEntityForSave> entities)
        //        {
        //            //  var entityMasks = GetMasksForSavedEntities(entities);
        //            // var permissions = await UserPermissions(Constants.Update);

        //            // TODO

        //            /* 
        //             * Step 1: Get complete mask for TEntityForSave
        //             * 
        //             * 
        //If there are no permissions: throw forbidden exception
        //else if ((1) at least one permission is criteria free and mask free, AND (2) all update entities (including nav entities) have a full mask in EntityMetadata) return safely
        //else {
        //  Do the magic to determine the mask that each entity is based on 

        //we need to do 2 things:

        //for each updated entity (including nav entities), construct the flat Mask in Entity Metadata (only relevant if (2) is false)
        //for each entity (including nav entity), determine the flat permission Mask applicable, (only relevant if (1) is false) <- throw forbidden exception if any permission has no mask

        //For every entity intersect the two masks into a MegaMask

        //Load entities by Ids from the DB,  <- need to know the EntityForSave for every entity ---> or do I??
        //  For every Update entity, any property that is missing from its MegaMask, copy that property value from the corresponding DB entity
        //  For every Insert entity, any property that is missing from its MegaMask, set that property to NULL
        //}

        //return the entities
        //             * 
        //             */

        //            return await Task.FromResult(entities);
        //        }

        ///// <summary>
        ///// For each saved entity, determines the applicable mask.
        ///// Verifies that the user has sufficient permissions to update the list of entities provided.
        ///// </summary>
        //protected virtual async Task<Dictionary<TEntityForSave, MaskTree>> GetMasksForSavedEntities(List<TEntityForSave> entities)
        //{
        //    if (entities == null || !entities.Any())
        //    {
        //        return new Dictionary<TEntityForSave, MaskTree>();
        //    }

        //    var unrestrictedMask = new MaskTree();
        //    var permissions = await UserPermissions(Constants.Update, cancellation: default); // non-cancellable
        //    if (!permissions.Any())
        //    {
        //        // User has no permissions on this table whatsoever; forbid
        //        throw new ForbiddenException();
        //    }
        //    else if (permissions.Any(e => string.IsNullOrWhiteSpace(e.Criteria) && string.IsNullOrWhiteSpace(e.Mask)))
        //    {
        //        // User has unfiltered update permission on the table => proceed
        //        return entities.ToDictionary(e => e, e => unrestrictedMask);
        //    }
        //    else
        //    {
        //        var resultDic = new Dictionary<TEntityForSave, MaskTree>();

        //        // An array of every criteria and every mask
        //        var maskAndCriteriaArray = permissions
        //            .Where(e => !string.IsNullOrWhiteSpace(e.Criteria)) // Optimization: a null criteria is satisfied by the entire list of entities
        //            .GroupBy(e => e.Criteria)
        //            .Select(g => new
        //            {
        //                Criteria = g.Key,
        //                Mask = g.Select(e => string.IsNullOrWhiteSpace(e.Mask) ? unrestrictedMask : MaskTree.Parse(e.Mask))
        //                .Aggregate((t1, t2) => t1.UnionWith(t2)) // Takes the union of all the mask trees
        //            }).ToArray();

        //        var universalPermissions = permissions
        //            .Where(e => string.IsNullOrWhiteSpace(e.Criteria));

        //        bool hasUniversalPermissions = universalPermissions.Count() > 0;

        //        // This mask (if exists) applies to every single entity since the criteria is null
        //        var universalMask = hasUniversalPermissions ? universalPermissions
        //            .Distinct()
        //            .Select(e => MaskTree.Parse(e.Mask))
        //            .Aggregate((t1, t2) => t1.UnionWith(t2)) : null;

        //        // Every criteria to every index of maskAndCriteriaArray
        //        var criteriaWithIndexes = maskAndCriteriaArray
        //            .Select((e, index) => new IndexAndCriteria { Criteria = e.Criteria, Index = index });

        //        /////// Part (1) Permissions must allow manipulating the original data before the update

        //        var existingEntities = entities.Where(e => !0.Equals(e.Id));
        //        if (existingEntities.Any())
        //        {
        //            // Get the Ids
        //            TKey[] existingIds = existingEntities
        //                .Select(e => e.Id).ToArray();

        //            // Prepare the query
        //            var query = GetRepository()
        //                .Query<TEntity>()
        //                .FilterByIds(existingIds);

        //            // id => index in maskAndCriteriaArray
        //            var criteriaMapList = await query
        //                .GetIndexToIdMap<TKey>(criteriaWithIndexes, cancellation: default);

        //            // id => indices in maskAndCriteriaArray
        //            var criteriaMapDictionary = criteriaMapList
        //                .GroupBy(e => e.Id)
        //                .ToDictionary(e => e.Key, e => e.Select(r => r.Index));

        //            foreach (var entity in existingEntities)
        //            {
        //                var id = entity.Id;
        //                MaskTree mask;

        //                if (criteriaMapDictionary.ContainsKey(id))
        //                {
        //                    // Those are entities that satisfy one or more non-null Criteria
        //                    mask = criteriaMapDictionary[id]
        //                        .Select(i => maskAndCriteriaArray[i].Mask)
        //                        .Aggregate((t1, t2) => t1.UnionWith(t2))
        //                        .UnionWith(universalMask);
        //                }
        //                else
        //                {
        //                    if (hasUniversalPermissions)
        //                    {
        //                        // Those are entities that belong to the universal mask of null criteria
        //                        mask = universalMask;
        //                    }
        //                    else
        //                    {
        //                        // Cannot update or delete this record, it doesn't satisfy any criteria
        //                        throw new ForbiddenException();
        //                    }
        //                }

        //                resultDic.Add(entity, mask);
        //            }
        //        }


        //        /////// Part (2) Permissions must work for the new data after the update, only for the modified properties
        //        {
        //            // index in newItems => index in maskAndCriteriaArray
        //            var criteriaMapList = await GetAsQuery(entities)
        //                .GetIndexToIndexMap(criteriaWithIndexes, cancellation: default);

        //            var criteriaMapDictionary = criteriaMapList
        //                .GroupBy(e => e.Id)
        //                .ToDictionary(e => e.Key, e => e.Select(r => r.Index));

        //            foreach (var (entity, index) in entities.Select((entity, i) => (entity, i)))
        //            {
        //                MaskTree mask;

        //                if (criteriaMapDictionary.ContainsKey(index))
        //                {
        //                    // Those are entities that satisfy one or more non-null Criteria
        //                    mask = criteriaMapDictionary[index]
        //                        .Select(i => maskAndCriteriaArray[i].Mask)
        //                        .Aggregate((t1, t2) => t1.UnionWith(t2))
        //                        .UnionWith(universalMask);
        //                }
        //                else
        //                {
        //                    if (hasUniversalPermissions)
        //                    {
        //                        // Those are entities that belong to the universal mask of null criteria
        //                        mask = universalMask;
        //                    }
        //                    else
        //                    {
        //                        // Cannot insert or update this record, it doesn't satisfy any criteria
        //                        throw new ForbiddenException();
        //                    }
        //                }

        //                if (resultDic.ContainsKey(entity))
        //                {
        //                    var entityMask = resultDic[entity];
        //                    resultDic[entity] = resultDic[entity].IntersectionWith(mask);

        //                }
        //                else
        //                {
        //                    resultDic.Add(entity, mask);
        //                }
        //            }
        //        }

        //        return resultDic; // preserve the original order
        //    }
        //}

        ///// <summary>
        ///// Implementation should prepare a select statement that returns the provided entities 
        ///// as an SQL result from a user-defined table type variable or a temporary table, using
        ///// the index of the entities as the Id (even if the Id of the entity is not integer).
        ///// This SQL result will be used to determine which of these entities earn which permission
        ///// masks.
        ///// </summary>
        //protected virtual Query<TEntity> GetAsQuery(List<TEntityForSave> entities)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #endregion

        #region Delete

        /// <summary>
        /// Assumes that the view does not allow 'Create' permission level, if it does
        /// need to override it
        /// </summary>
        public virtual async Task Delete(List<TKey> ids)
        {
            if (ids == null || !ids.Any())
            {
                return;
            }

            // Permissions
            var deleteFilter = await UserPermissionsFilter(Constants.Delete, cancellation: default);
            ids = await CheckActionPermissionsBefore(deleteFilter, ids);

            // Validate
            await DeleteValidateAsync(ids);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Execute
            await DeleteExecuteAsync(ids);
        }

        /// <summary>
        /// Deletes the entities specified by the list of Ids, call to this method is NOT wrapped inside a transaction
        /// </summary>
        protected abstract Task DeleteExecuteAsync(List<TKey> ids);

        /// <summary>
        /// Validates the delete operation before it happens
        /// </summary>
        protected abstract Task DeleteValidateAsync(List<TKey> ids);

        #endregion

        #region Import & Export

        public async Task<Stream> ExportByIds(ExportByIdsArguments<TKey> args, CancellationToken cancellation)
        {
            var metaForSave = GetMetadataForSave();
            var meta = GetMetadata();

            // Get the default mapping, auto calculated from the entity for save metadata
            MappingInfo mapping = GetDefaultMapping(metaForSave, meta);

            // Create headers
            string[] headers = HeadersFromMapping(mapping);

            // Load entities
            string select = SelectFromMapping(mapping);
            var (entities, _) = await GetByIds(args.I, new SelectExpandArguments
            {
                Select = select
            }, 
            Constants.Read,
            cancellation);

            // Create content
            var composer = new DataComposer();
            var dataWithoutHeaders = composer.Compose(entities, mapping);

            // Final result
            var data = new List<string[]> { headers }.Concat(dataWithoutHeaders);
            var csvHandler = new CsvPackager();
            return csvHandler.Package(data);
        }

        //private (string[] headers, Func<TEntity, string>[] dataGetters) SoftMappingFromSelect(string select)
        //{
        //    var selectExp = SelectExpression.Parse(select);
        //    var dataGetters = new Func<TEntity, string>[selectExp.Count];
        //    var headers = new string[selectExp.Count];

        //    var meta = GetMetadata();

        //    foreach (var (atom, index) in selectExp.Select((a, i) => (a, i)))
        //    {
        //        List<Func<Entity, object>> entityGetters = new List<Func<Entity, object>>(atom.Path.Length);
        //        List<string> headersTrail = new List<string>(atom.Path.Length + 1);

        //        // Do the path
        //        var currentMeta = meta;
        //        foreach (var step in atom.Path)
        //        {
        //            var navPropMeta = currentMeta.NavigationProperty(step) ?? throw new BadRequestException($"Navigation property {step} does not exist on type {currentMeta.Descriptor.Name}");
        //            entityGetters.Add(navPropMeta.Descriptor.GetValue);
        //            headersTrail.Add(navPropMeta.Display());

        //            currentMeta = navPropMeta.EntityMetadata;
        //        }

        //        // Do the property
        //        var propMeta = currentMeta.Property(atom.Property) ?? throw new BadRequestException($"Property {atom.Property} does not exist on type {currentMeta.Descriptor.Name}");
        //        Func<Entity, object> getPropertyValue = propMeta.Descriptor.GetValue;
        //        Func<object, string> formatValue = propMeta.Format;

        //        headersTrail.Add(propMeta.Display());

        //        // Get the header
        //        headers[index] = string.Join(" / ", headersTrail);

        //        // Build the data getter
        //        dataGetters[index] = (entity) =>
        //        {
        //            Entity current = entity;
        //            foreach (var getEntity in entityGetters)
        //            {
        //                current = getEntity(current) as Entity;
        //                if (current == null)
        //                {
        //                    return null;
        //                }
        //            }

        //            object value = getPropertyValue(current);
        //            return formatValue(value);
        //        };
        //    }

        //    return (headers, dataGetters);
        //}

        public async Task<Stream> Export(ExportArguments args, CancellationToken cancellation)
        {
            var metaForSave = GetMetadataForSave();
            var meta = GetMetadata();

            // Get the default mapping, auto calculated from the entity for save metadata
            MappingInfo mapping = GetDefaultMapping(metaForSave, meta);

            // Create headers
            string[] headers = HeadersFromMapping(mapping);

            // Load entities
            string select = SelectFromMapping(mapping);
            var (entities, _, _, _) = await GetFact(new GetArguments
            {
                Top = args.Top,
                Skip = args.Skip,
                Filter = args.Filter,
                Search = args.Search,
                OrderBy = args.OrderBy,
                Select = select,
                CountEntities = false
            },
            cancellation);

            // Create content
            var composer = new DataComposer();
            var dataWithoutHeaders = composer.Compose(entities, mapping);

            // Final result
            var data = new List<string[]> { headers }.Concat(dataWithoutHeaders);
            var csvHandler = new CsvPackager();
            return csvHandler.Package(data);
        }

        public Stream CsvTemplate()
        {
            var metaForSave = GetMetadataForSave();
            var meta = GetMetadata();

            // Get the default mapping, auto calculated from the entity for save metadata
            var mapping = GetDefaultMapping(metaForSave, meta);

            // Get the headers from the mapping
            string[] headers = HeadersFromMapping(mapping);

            // Create a CSV file containing only those headers
            var csvHandler = new CsvPackager();
            return csvHandler.Package(new List<string[]> { headers });
        }

        public async Task<ImportResult> Import(Stream fileStream, string fileName, string contentType, ImportArguments args)
        {
            var sw = new Stopwatch();
            sw.Start();

            // Validation

            args.Mode ??= ImportModes.Insert; // Default
            if (!ImportModes.All.Contains(args.Mode))
            {
                var allowedValues = string.Join(", ", ImportModes.All);
                throw new BadRequestException(_localizer["Error_UnknownImportMode0AllowedValuesAre1", args.Mode, allowedValues]);
            }

            if (args.Mode != ImportModes.Insert && string.IsNullOrWhiteSpace(args.Key))
            {
                // Key parameter is required for import modes update and merge
                throw new BadRequestException(_localizer[Constants.Error_Field0IsRequired, _localizer["KeyProperty"]]);
            }

            if (fileStream == null)
            {
                throw new BadRequestException(_localizer["Error_NoFileWasUploaded"]);
            }

            // Extract the raw data from the file stream
            IEnumerable<string[]> data = ExtractStringsFromFile(fileStream, fileName, contentType);
            if (!data.Any())
            {
                throw new BadRequestException(_localizer["Error_UploadedFileWasEmpty"]);
            }

            // Map the columns
            var importErrors = new ImportErrors();
            var headers = data.First();
            var mapping = MappingFromHeaders(headers, importErrors);

            // Abort if there are validation errors
            importErrors.ThrowIfInvalid(_localizer);

            // Parse the data to entities
            var parser = _sp.GetRequiredService<DataParser>();
            var entitiesEnum = await parser.ParseAsync<TEntityForSave>(data.Skip(1), mapping, importErrors);
            importErrors.ThrowIfInvalid(_localizer);

            // Handle Update and Merge modes
            if (args.Mode == ImportModes.Update || args.Mode == ImportModes.Merge)
            {
                await HydrateIds(entitiesEnum, args, mapping, importErrors);
                importErrors.ThrowIfInvalid(_localizer);
            }

            // Save the entities
            var entities = entitiesEnum.ToList();
            try
            {
                // Save the data
                var saveArgs = new SaveArguments { ReturnEntities = false };
                await Save(entities, saveArgs);

                // Report success result
                int inserted = entitiesEnum.Count(e => e.Id == null || e.Id.Equals(0));
                int updated = entitiesEnum.Count(e => e.Id != null && !e.Id.Equals(0));
                sw.Stop();

                return new ImportResult
                {
                    Inserted = inserted,
                    Updated = updated,
                    Milliseconds = sw.ElapsedMilliseconds,
                };
            }
            catch (UnprocessableEntityException ex)
            {
                // Map errors to row numbers
                var validationErrors = ex.ModelState;
                if (validationErrors.IsValid)
                {
                    throw new InvalidOperationException("Bug: UnprocessableEntityException without validation errors");
                }

                MapErrors(validationErrors, importErrors, entities, mapping);
                if (importErrors.IsValid)
                {
                    throw new InvalidOperationException("Bug: UnprocessableEntityException validation errors were incorrectly mapped to an empty collection");
                }

                string errorMsg = importErrors.ToString(_localizer);
                throw new BadRequestException(errorMsg);
            }
        }

        private async Task HydrateIds(IEnumerable<TEntityForSave> entities, ImportArguments args, MappingInfo mapping, ImportErrors errors)
        {
            // If key property is ID, there is nothing to do
            if (args.Key == "Id")
            {
                return;
            }

            var propMapping = mapping.SimpleProperty(args.Key);
            if (propMapping == null)
            {
                throw new BadRequestException(_localizer["Error_KeyProperty0MustBeInTheImportedFile", args.Key]);
            }

            var propMeta = propMapping.Metadata;
            var propDesc = propMeta.Descriptor;
            if (propDesc.Type != typeof(string) && propDesc.Type != typeof(int) && propDesc.Type != typeof(int?))
            {
                throw new BadRequestException(_localizer["Error_KeyProperty0NotValidItMustIntOrString", propMeta.Display()]);
            }

            Func<Entity, object> forSaveKeyGet = propDesc.GetValue;

            // For update mode, check that all keys are present
            if (args.Mode == ImportModes.Update)
            {
                foreach (var entity in entities.Where(e => forSaveKeyGet(e) == null))
                {
                    // In update mode, the 
                    string errorMsg = _localizer["Error_Property0IsKeyPropertyThereforeRequiredForUpdate", propMeta.Display()];
                    if (!errors.AddImportError(entity.EntityMetadata.RowNumber, propMapping.ColumnNumber, errorMsg))
                    {
                        return;
                    }
                }
            }

            // Check that non-null user keys are unique
            foreach (var g in entities.GroupBy(e => forSaveKeyGet(e)).Where(g => g.Key != null && g.Count() > 1))
            {
                foreach (var entity in g)
                {
                    // In update mode, the 
                    var duplicateKeyValue = forSaveKeyGet(entity).ToString();
                    string errorMsg = _localizer["Error_Value0IsDuplicatedEvenThoughItIsKey1", duplicateKeyValue, propMeta.Display()];
                    if (!errors.AddImportError(entity.EntityMetadata.RowNumber, propMapping.ColumnNumber, errorMsg))
                    {
                        return;
                    }
                }
            }

            if (!errors.IsValid)
            {
                // Later code may fail if there are key uniqueness errors
                return;
            }

            // Load entities from the DB
            var userKeys = entities.Select(e => forSaveKeyGet(e)).Where(e => e != null);
            var getArgs = new SelectExpandArguments { Select = propDesc.Name };
            var (dbEntities, _) = await GetByPropertyValues(propDesc.Name, userKeys, getArgs, cancellation: default);
            if (dbEntities.Any())
            {
                // Prepare the key property description of TEntity
                var typeDesc = TypeDescriptor.Get<TEntity>();
                var prop = typeDesc.Property(args.Key);
                if (prop == null)
                {
                    throw new InvalidOperationException($"Bug: Type {nameof(TEntityForSave)} has property {args.Key} but not type {nameof(TEntity)}");
                }

                Func<Entity, object> keyGet = prop.GetValue;

                // group the DB entities by key property
                var dbEntitiesDic = dbEntities.GroupBy(e => keyGet(e))
                    .ToDictionary(g => g.Key, g => (IEnumerable<TEntity>)g);

                foreach (var entity in entities)
                {
                    var key = forSaveKeyGet(entity);
                    if (key == null)
                    {
                        // IF Update mode add an error, but this was handled earlier before the database call
                    }
                    else
                    {
                        if (dbEntitiesDic.TryGetValue(key, out IEnumerable<TEntity> matches))
                        {
                            if (matches.Skip(1).Any())
                            {
                                var typeDisplay = mapping.Metadata.SingularDisplay();
                                var keyPropDisplay = propMeta.Display();
                                var stringField = key.ToString();
                                if (!errors.AddImportError(entity.EntityMetadata.RowNumber, propMapping.ColumnNumber, _localizer["Error_MoreThanOne0FoundWhere1Equals2", typeDisplay, keyPropDisplay, stringField]))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                // Copy the Id from the entity to the entity for save
                                var dbEntity = matches.Single();
                                entity.SetId(dbEntity.GetId());
                            }
                        }
                        else if (args.Mode == ImportModes.Update)
                        {
                            var typeDisplay = mapping.Metadata.SingularDisplay();
                            var keyPropDisplay = propMeta.Display();
                            var stringField = key.ToString();
                            if (!errors.AddImportError(entity.EntityMetadata.RowNumber, propMapping.ColumnNumber, _localizer["Error_No0WasFoundWhere1Equals2", typeDisplay, keyPropDisplay, stringField]))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the default mapping based on the properties in meta (not meta for save)
        /// </summary>
        protected virtual MappingInfo GetDefaultMapping(TypeMetadata metaForSave, TypeMetadata meta)
        {
            // Inner recursive function, returns the mapping and the next available column index
            static (MappingInfo mapping, int nextAvailableIndex) GetDefaultMappingInner(TypeMetadata metaForSave, TypeMetadata meta, int nextAvailableIndex, CollectionPropertyMetadata collPropMeta = null)
            {
                Dictionary<string, NavigationPropertyMetadata> fkNames = meta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);

                // Prepare simple props
                List<PropertyMappingInfo> simpleProps = new List<PropertyMappingInfo>();
                foreach (var propMetaForSave in metaForSave.SimpleProperties)
                {
                    string propName = propMetaForSave.Descriptor.Name;
                    var propMeta = meta.Property(propName) ??
                        throw new InvalidOperationException($"Bug: Property '{propName}' exists on type for save {metaForSave.Descriptor.Name} but not on {meta.Descriptor.Name}");

                    if (propMeta.Descriptor.Name == "Id" && propMeta.Descriptor.Type != typeof(string))
                    {
                        continue; // INT properties are autogenerated
                    }

                    if (fkNames.TryGetValue(propMeta.Descriptor.Name, out NavigationPropertyMetadata navPropMetadata))
                    {
                        // Foreign Key
                        simpleProps.Add(new ForeignKeyMappingInfo
                        {
                            Index = nextAvailableIndex++,
                            Metadata = propMeta,

                            // FK stuff
                            NavPropertyMetadata = navPropMetadata,
                            KeyPropertyMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty
                        });
                    }
                    else
                    {
                        // Simple property
                        simpleProps.Add(new PropertyMappingInfo
                        {
                            Index = nextAvailableIndex++,
                            Metadata = propMeta
                        });
                    }
                }

                // Prepare collection props
                List<MappingInfo> collectionProps = new List<MappingInfo>();
                foreach (var nextCollPropMetaForSave in metaForSave.CollectionProperties)
                {
                    string propName = nextCollPropMetaForSave.Descriptor.Name;
                    var nextCollPropMeta = meta.CollectionProperty(propName) ??
                        throw new InvalidOperationException($"Bug: Collection property '{propName}' exists on type for save {metaForSave.Descriptor.Name} but not on {meta.Descriptor.Name}");

                    TypeMetadata nextMetaForSave = nextCollPropMetaForSave.CollectionTargetTypeMetadata;
                    TypeMetadata nextMeta = nextCollPropMeta.CollectionTargetTypeMetadata;

                    // Recursive call
                    var (nextMapping, nextIndex) = GetDefaultMappingInner(nextMetaForSave, nextMeta, nextAvailableIndex, nextCollPropMeta);
                    collectionProps.Add(nextMapping);
                    nextAvailableIndex = nextIndex;
                }

                // Return the mapping and the next available index
                var mapping = new MappingInfo(meta, simpleProps, collectionProps, collPropMeta);
                return (mapping, nextAvailableIndex);
            }

            // Call the inner recursive function and return the result;
            var (mapping, _) = GetDefaultMappingInner(metaForSave, meta, 0);
            return ProcessDefaultMapping(mapping);
        }

        /// <summary>
        /// Provides a chance for services to alter the default mapping info used for
        /// generating the import template and exporting for import
        /// </summary>
        protected virtual MappingInfo ProcessDefaultMapping(MappingInfo mapping)
        {
            return mapping;
        }

        private string SelectFromMapping(MappingInfo mapping)
        {
            static void SelectFromMappingInner(MappingInfo mapping, StringBuilder bldr, string prefix, bool notFirstAtom)
            {
                foreach (var simpleProp in mapping.SimpleProperties)
                {
                    // Append a comma if this is the second atom onward
                    if (notFirstAtom)
                    {
                        bldr.Append(",");
                    }

                    notFirstAtom = true;

                    // Append the prefix if any
                    if (prefix != null)
                    {
                        bldr.Append(prefix);
                        bldr.Append("/");
                    }

                    // Append the property name
                    if (simpleProp is ForeignKeyMappingInfo fkProp && fkProp.NotUsingIdAsKey)
                    {
                        // Append navigation property name followed by key. E.g. Resource/Code
                        bldr.Append(fkProp.NavPropertyMetadata.Descriptor.Name);
                        bldr.Append("/");
                        bldr.Append(fkProp.KeyPropertyMetadata.Descriptor.Name);
                    }
                    else
                    {
                        // Append simple property name. E.g. DateOfBirth
                        bldr.Append(simpleProp.Metadata.Descriptor.Name);
                    }
                }

                foreach (var collProp in mapping.CollectionProperties)
                {
                    string nextPrefix;
                    if (prefix == null)
                    {
                        nextPrefix = collProp.ParentCollectionPropertyMetadata.Descriptor.Name;
                    }
                    else
                    {
                        nextPrefix = $"{prefix}/{collProp.ParentCollectionPropertyMetadata.Descriptor.Name}";
                    }

                    SelectFromMappingInner(collProp, bldr, nextPrefix, notFirstAtom);
                }
            }

            StringBuilder bldr = new StringBuilder();
            SelectFromMappingInner(mapping, bldr, prefix: null, notFirstAtom: false);
            return bldr.ToString();
        }

        private string[] HeadersFromMapping(MappingInfo mapping)
        {
            static string Escape(string propDisplay)
            {
                return propDisplay.Replace("/", "//").Replace("-", "--");
            }

            static void PopulateHeadersArray(string[] headers, MappingInfo mapping, string path = null)
            {
                foreach (var g in mapping.SimpleProperties.GroupBy(e => e.Metadata.Display()))
                {
                    string escapedDisplay = Escape(g.Key);
                    int counter = 1;
                    bool counterIsNeeded = g.Count() > 1;
                    foreach (var propMapping in g)
                    {
                        var propDisplay = escapedDisplay;

                        // Append disambiguation counter for the rare case when two simple properties have the exact same label (may happen with definitioned entities)
                        if (counterIsNeeded)
                        {
                            propDisplay = $"{propDisplay}.{counter++}";
                        }

                        // If foreign key, add the suggested key property on the target type
                        if (propMapping is ForeignKeyMappingInfo fkMapping)
                        {
                            PropertyMetadata keyPropMeta = fkMapping.KeyPropertyMetadata;
                            string keyDisplay = Escape(keyPropMeta.Display());
                            propDisplay = $"{propDisplay} - {keyDisplay}";
                        }

                        // add the result to the headers array
                        if (path == null)
                        {
                            headers[propMapping.Index] = propDisplay;
                        }
                        else
                        {
                            headers[propMapping.Index] = $"{path} / {propDisplay}";
                        }
                    }
                }

                foreach (var g in mapping.CollectionProperties.GroupBy(e => e.ParentCollectionPropertyMetadata.Display()))
                {
                    string escapedDisplay = Escape(g.Key);
                    int counter = 1;
                    bool counterIsNeeded = g.Count() > 1;
                    foreach (var collProp in g)
                    {
                        var propDisplay = escapedDisplay;

                        // Append disambiguation counter for the rare case when two collection properties have the exact same label (may happen with definitioned entities)
                        if (counterIsNeeded)
                        {
                            propDisplay = $"{propDisplay}.{counter++}";
                        }

                        // Prepare the next path
                        string nextPath;
                        if (path == null)
                        {
                            nextPath = propDisplay;
                        }
                        else
                        {
                            nextPath = $"{path} / {propDisplay}";
                        }

                        // Call the function recursively
                        PopulateHeadersArray(headers, collProp, nextPath);
                    }
                }
            }

            int columnCount = mapping.ColumnCount();
            var headers = new string[columnCount];

            PopulateHeadersArray(headers, mapping);

            return headers;
        }

        protected virtual MappingInfo MappingFromHeaders(string[] headers, ImportErrors errors)
        {
            // Create the trie of 
            var trie = new LabelPathTrie();
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                if (string.IsNullOrWhiteSpace(header))
                {
                    if (!errors.AddImportError(1, i + 1, _localizer["Error_EmptyHeadersNotAllowed"]))
                    {
                        return null;
                    }
                }

                var (steps, key) = SplitHeader(header);
                trie.AddPath(steps, key, index: i);
            }

            // Get the metadatas
            var rootMetadata = GetMetadata();
            var rootMetadataForSave = GetMetadataForSave();

            // Create the mapping recurisvely using the trie
            var result = trie.CreateMapping(rootMetadata, rootMetadataForSave, errors, _localizer);
            return result;
        }

        protected TypeMetadata GetMetadata()
        {
            int? tenantId = _tenantIdAccessor.GetTenantIdIfAny();
            int? definitionId = DefinitionId;
            Type type = typeof(TEntity);

            return _metadata.GetMetadata(tenantId, type, definitionId);
        }

        protected TypeMetadata GetMetadataForSave()
        {
            int? tenantId = _tenantIdAccessor.GetTenantIdIfAny();
            int? definitionId = DefinitionId;
            Type typeForSave = typeof(TEntityForSave);

            return _metadata.GetMetadata(tenantId, typeForSave, definitionId);
        }

        private class LabelPathTrie : Dictionary<string, LabelPathTrie>
        {
            private readonly HashSet<LabelPathProperty> _props = new HashSet<LabelPathProperty>();

            public void AddPath(IEnumerable<string> steps, string key, int index)
            {
                if (steps == null || !steps.Any())
                {
                    throw new BadRequestException($"Bug: Attempt to add an empty header to the trie");
                }

                var current = this;
                foreach (var step in steps.SkipLast(1))
                {
                    if (!current.TryGetValue(step, out LabelPathTrie match))
                    {
                        match = current[step] = new LabelPathTrie();
                    }

                    // Go one step below
                    current = match;
                }

                var lastStep = steps.Last();
                current._props.Add(new LabelPathProperty
                {
                    PropLabel = lastStep,
                    KeyLabel = key,
                    Index = index
                });
            }

            public MappingInfo CreateMapping(TypeMetadata meta, TypeMetadata metaForSave, ImportErrors errors, IStringLocalizer localizer, CollectionPropertyMetadata collPropMeta = null)
            {
                // Collect the names of all foreign keys in a dictionary
                var fkNames = meta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);

                // Collect the display names of all the simple properties in a dictionary
                Dictionary<string, PropertyMetadata> simpleProps = new Dictionary<string, PropertyMetadata>();
                foreach (var g in metaForSave.SimpleProperties.GroupBy(p => p.Display()))
                {
                    string display = g.Key;
                    if (g.Count() > 1)
                    {
                        // If multiple properties have the same name, disambiguate them with a postfix number
                        int counter = 1;
                        foreach (var propMetadata in g)
                        {
                            simpleProps.Add($"{display}.{counter++}", propMetadata);
                        }
                    }
                    else
                    {
                        simpleProps.Add(display, g.Single());
                    }
                }

                // Collect the mappings of this level in a list
                List<PropertyMappingInfo> simplePropMappings = new List<PropertyMappingInfo>();

                HashSet<string> simplePropsLabels = null;
                HashSet<string> simplePropsLabelsIgnoreCase = null;
                foreach (var prop in _props)
                {

                    // Try to match the property, if no match is found add a suitable error
                    if (prop.PropLabel == "###" && false) // Maybe this isn't needed
                    {
                        // This here is a simple placeholder to trigger creation of entity but not set any property on it
                        simplePropMappings.Add(new PropertyMappingInfo
                        {
                            Ignore = true,
                            Index = prop.Index
                        });
                    }
                    else if (!simpleProps.TryGetValue(prop.PropLabel, out PropertyMetadata propMetadata))
                    {
                        simplePropsLabels ??= metaForSave.SimpleProperties.Select(e => e.Display()).ToHashSet();
                        simplePropsLabelsIgnoreCase ??= metaForSave.SimpleProperties.Select(e => e.Display()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (simplePropsLabels.Contains(prop.PropLabel))
                        {
                            // Common mistake: label isn't unique and must be postfixed with a number to disambiguate it
                            errors.AddImportError(1, prop.ColumnNumber, localizer["Error_Label0MatchesMultipleFieldsOnType1", prop.PropLabel, metaForSave.SingularDisplay()]);
                        }
                        else if (simplePropsLabelsIgnoreCase.TryGetValue(prop.PropLabel, out string actualLabel))
                        {
                            // Common mistake: using the wrong case
                            errors.AddImportError(1, prop.ColumnNumber, localizer["Error_Label0DoesNotMatchAnyFieldOnType1DidYouMean2", prop.PropLabel, metaForSave.SingularDisplay(), actualLabel]);
                        }
                        else
                        {
                            errors.AddImportError(1, prop.ColumnNumber, localizer["Error_Label0DoesNotMatchAnyFieldOnType1", prop.PropLabel, metaForSave.SingularDisplay()]);
                        }

                        continue;
                    }
                    else if (fkNames.TryGetValue(propMetadata.Descriptor.Name, out NavigationPropertyMetadata navPropMeta))
                    {
                        // This is a foreign key
                        string keyLabel = prop.KeyLabel;
                        if (string.IsNullOrWhiteSpace(keyLabel))
                        {
                            // FK without a key property
                            errors.AddImportError(1, prop.ColumnNumber, localizer["Error_KeyPropertyIsRequiredToSet0Field", propMetadata.Display()]);
                            continue;
                        }

                        TypeMetadata targetTypeMeta = navPropMeta.TargetTypeMetadata;
                        PropertyMetadata keyPropMetadata = targetTypeMeta.SimpleProperties.FirstOrDefault(p => p.Display() == keyLabel);
                        if (keyPropMetadata == null)
                        {
                            // FK with a key property that doesn't exist
                            PropertyMetadata caseInsensitiveMatch = targetTypeMeta.SimpleProperties.FirstOrDefault(p => p.Display().ToLower() == keyLabel.ToLower());
                            if (caseInsensitiveMatch != null)
                            {
                                // There is a case insensitive match: suggest
                                string suggestion = caseInsensitiveMatch.Display();
                                errors.AddImportError(1, prop.ColumnNumber, localizer["Error_Label0DoesNotMatchAnyFieldOnType1DidYouMean2", keyLabel, targetTypeMeta.SingularDisplay(), suggestion]);
                            }
                            else
                            {
                                // Error without suggestion
                                errors.AddImportError(1, prop.ColumnNumber, localizer["Error_Label0DoesNotMatchAnyFieldOnType1", keyLabel, targetTypeMeta.SingularDisplay()]);
                            }
                            continue;
                        }

                        simplePropMappings.Add(new ForeignKeyMappingInfo
                        {
                            Metadata = propMetadata,
                            Index = prop.Index,

                            // FK stuff
                            NavPropertyMetadata = navPropMeta,
                            KeyPropertyMetadata = keyPropMetadata
                        });
                    }
                    else
                    {
                        // This is a simpe prop
                        simplePropMappings.Add(new PropertyMappingInfo
                        {
                            Metadata = propMetadata,
                            Index = prop.Index,
                        });
                    }
                }

                // Collect the display names of all the collection properties in a dictionary
                Dictionary<string, CollectionPropertyMetadata> collectionProps = new Dictionary<string, CollectionPropertyMetadata>();
                foreach (var g in metaForSave.CollectionProperties.GroupBy(p => p.Display()))
                {
                    string display = g.Key;
                    if (g.Count() > 1)
                    {
                        // If multiple properties have the same name, disambiguate them with a postfix number
                        int counter = 1;
                        foreach (var propMetadata in g)
                        {
                            collectionProps.Add($"{display}.{counter++}", propMetadata);
                        }
                    }
                    else
                    {
                        collectionProps.Add(display, g.Single());
                    }
                }

                // Collect the mappings of the next levels in a list
                List<MappingInfo> collectionPropMappings = new List<MappingInfo>();
                HashSet<string> collectionPropsLabels = null;
                HashSet<string> collectionPropsLabelsIgnoreCase = null;
                foreach (var (collectionPropName, trie) in this)
                {
                    if (!collectionProps.TryGetValue(collectionPropName, out CollectionPropertyMetadata propMetadataForSave))
                    {
                        collectionPropsLabels ??= metaForSave.CollectionProperties.Select(e => e.Display()).ToHashSet();
                        collectionPropsLabelsIgnoreCase ??= metaForSave.CollectionProperties.Select(e => e.Display()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (collectionPropsLabels.Contains(collectionPropName))
                        {
                            // Common mistake: label isn't unique and must be postfixed with a number to disambiguate it
                            errors.AddImportError(1, trie.FirstColumnNumber(), localizer["Error_Label0MatchesMultipleCollectionsOnType1", collectionPropName, metaForSave.SingularDisplay()]);
                        }
                        else if (collectionPropsLabelsIgnoreCase.TryGetValue(collectionPropName, out string actualLabel))
                        {
                            // Common mistake: using the wrong case
                            errors.AddImportError(1, trie.FirstColumnNumber(), localizer["Error_Label0DoesNotMatchAnyCollectionOnType1DidYouMean2", collectionPropName, metaForSave.SingularDisplay(), actualLabel]);
                        }
                        else
                        {
                            errors.AddImportError(1, trie.FirstColumnNumber(), localizer["Error_Label0DoesNotMatchAnyCollectionOnType1", collectionPropName, metaForSave.SingularDisplay()]);
                        }

                        continue;
                    }

                    var propTypeMetadataForSave = propMetadataForSave.CollectionTargetTypeMetadata;
                    var propTypeMetadata = meta.CollectionProperty(propMetadataForSave.Descriptor.Name)?.CollectionTargetTypeMetadata ??
                        throw new InvalidOperationException($"Property {propMetadataForSave.Descriptor.Name} is present on {metaForSave.Descriptor.Name} but not {meta.Descriptor.Name}");

                    collectionPropMappings.Add(trie.CreateMapping(propTypeMetadata, propTypeMetadataForSave, errors, localizer, propMetadataForSave));
                }

                return new MappingInfo(metaForSave, simplePropMappings, collectionPropMappings, collPropMeta);
            }

            private int FirstColumnNumber()
            {
                if (_props.Count > 0)
                {
                    return _props.Min(e => e.ColumnNumber);
                }
                else
                {
                    return Values.Min(e => e.FirstColumnNumber());
                }
            }

            private struct LabelPathProperty
            {
                public string PropLabel { get; set; }
                public string KeyLabel { get; set; }
                public int Index { get; set; }
                public int ColumnNumber => Index + 1;
            }
        }

        /// <summary>
        /// Splits header label into a collection of steps
        /// </summary>
        /// <param name="headerLabel"></param>
        /// <returns></returns>
        private (IEnumerable<string>, string) SplitHeader(string headerLabel)
        {
            List<string> result = new List<string>();
            var builder = new StringBuilder();
            for (int i = 0; i < headerLabel.Length; i++)
            {
                char c = headerLabel[i];
                if (c == '/')
                {
                    if (i + 1 < headerLabel.Length && headerLabel[i + 1] == '/') // Escaped
                    {
                        builder.Append(c);
                        i++; // Ignore the second forward slash
                    }
                    else
                    {
                        result.Add(builder.ToString().Trim());
                        builder = new StringBuilder();
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }

            var (prop, key) = SplitStep(builder.ToString());
            result.Add(prop);

            return (result, key);
        }

        private (string prop, string key) SplitStep(string stepLabel)
        {
            string prop = null;
            string key = null;

            var builder = new StringBuilder();
            for (int i = 0; i < stepLabel.Length; i++)
            {
                char c = stepLabel[i];
                if (c == '-' && prop == null)
                {
                    if (i + 1 < stepLabel.Length && stepLabel[i + 1] == '-') // Escaped
                    {
                        builder.Append(c);
                        i++; // Ignore the second opening square bracket
                    }
                    else
                    {
                        prop = builder.ToString();
                        builder = new StringBuilder();
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }

            if (prop == null)
            {
                prop = builder.ToString();
            }
            else
            {
                key = builder.ToString();
            }

            return (prop?.Trim(), key?.Trim());
        }

        protected IEnumerable<string[]> ExtractStringsFromFile(Stream stream, string fileName, string contentType)
        {
            IDataExtractor extracter;
            if (contentType == MimeTypes.Csv || (fileName?.ToLower()?.EndsWith(".csv") ?? false))
            {
                extracter = new CsvExtractor();
            }
            else if (contentType == MimeTypes.Excel || (fileName?.ToLower()?.EndsWith(".xlsx") ?? false))
            {
                extracter = new ExcelExtractor();
            }
            else
            {
                throw new FormatException(_localizer["Error_OnlyCsvOrExcelAreSupported"]);
            }

            // Extrat and return
            try
            {
                return extracter.Extract(stream).ToList();
            } 
            catch (Exception ex)
            {
                // Report any errors during extraction
                string msg = _localizer["Error_FailedToParseFileError0", ex.Message];
                throw new BadRequestException(msg);
            }
        }

        private void MapErrors(ValidationErrorsDictionary errorsDic, ImportErrors errors, List<TEntityForSave> entities, MappingInfo mapping)
        {
            foreach (var (key, errorMessages) in errorsDic.AllErrors)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new InvalidOperationException($"Bug: Empty validation error key");
                }

                var steps = key.Split('.').Select(e => e.Trim());

                // Get the root index
                string firstStep = steps.First().Trim();
                if (!firstStep.StartsWith('[') || !firstStep.EndsWith(']'))
                {
                    throw new InvalidOperationException($"Bug: validation error key '{key}' should start with the root index in square brackets []");
                }

                var rootIndexString = firstStep.Remove(firstStep.Length - 1).Substring(1);
                if (!int.TryParse(rootIndexString, out int rootIndex))
                {
                    throw new InvalidOperationException($"Bug: root index '{rootIndexString}' could not be parsed into an integer");
                }

                if (rootIndex >= entities.Count)
                {
                    throw new InvalidOperationException($"Bug: root index '{rootIndexString}' is larger than the size of the indexed list {entities.Count}");
                }

                MappingInfo currentMapping = mapping;
                Entity currentEntity = entities[rootIndex];
                TypeDescriptor currentTypeDesc = TypeDescriptor.Get<TEntityForSave>();
                PropertyMappingInfo propertyMapping = null; // They property that the error key may optionally terminate with
                bool lastPropWasCollectionWithoutIndexer = false;
                bool lastPropWasSimple = false;

                foreach (var step in steps.Skip(1))
                {
                    if (currentEntity == null)
                    {
                        throw new InvalidOperationException($"Bug: step '{step}' on validation error key '{key}' is applied to a null entity");
                    }
                    if (lastPropWasCollectionWithoutIndexer)
                    {
                        throw new InvalidOperationException($"Bug: step '{step}' on validation error key '{key}' is applied to a list");
                    }
                    if (lastPropWasSimple)
                    {
                        throw new InvalidOperationException($"Bug: step '{step}' on validation error key '{key}' is applied to a simple property");
                    }

                    var trimmedStep = step.Trim();
                    if (trimmedStep.EndsWith(']')) // Collection Property + Index
                    {
                        // Remove the ']' at the end;
                        trimmedStep = trimmedStep.Remove(trimmedStep.Length - 1);
                        var split = trimmedStep.Split('[');

                        var indexString = split.Last();
                        if (!int.TryParse(indexString, out int index))
                        {
                            throw new InvalidOperationException($"Bug: validation error key '{key}' contains index '{rootIndexString}' that could not be parsed into an integer");
                        }

                        var propName = string.Join('[', split.SkipLast(1));
                        if (string.IsNullOrWhiteSpace(propName))
                        {
                            throw new InvalidOperationException($"Bug: validation error key '{key}' cannot contain a lone indexer in the middle of it");
                        }

                        // Retrieve the next entity using descriptors
                        var propDesc = currentTypeDesc.CollectionProperty(propName) ??
                            throw new InvalidOperationException($"Bug: collection property '{propName}' on validation error key '{key}' could not be found on type {currentTypeDesc.Name}");

                        currentEntity = ((propDesc.GetValue(currentEntity) as IList)[index] as Entity) ??
                            throw new InvalidOperationException($"Bug: step '{step}' on validation error key '{key}' refers to a null entity");

                        currentTypeDesc = propDesc.CollectionTypeDescriptor;

                        // Retrieve the next mapping if possible
                        var nextMapping = currentMapping?.CollectionProperty(propName);
                    }
                    else // Property: either collection, navigation, or simple
                    {
                        var propName = step;
                        var propDesc = currentTypeDesc.Property(propName);

                        if (propDesc is null)
                        {
                            throw new InvalidOperationException($"Bug: property '{propName}' on validation error key '{key}' could not be found on type {currentTypeDesc.Name}");
                        }
                        else if (propDesc is CollectionPropertyDescriptor collPropDesc)
                        {
                            // A collection property without indexer cannot by succeeded by more steps
                            lastPropWasCollectionWithoutIndexer = true; // To prevent further steps
                        }
                        else if (propDesc is NavigationPropertyDescriptor)
                        {
                            // Won't implement for now, there aren't any cases in our model
                            throw new NotImplementedException("Navigation property errors not implemented");
                        }
                        else // Simple prop
                        {
                            // Retrieve the property mapping if possible
                            propertyMapping = currentMapping?.SimpleProperty(propName);
                            lastPropWasSimple = true; // To prevent further steps
                        }
                    }
                }

                // Now to use the goods
                int row = currentEntity.EntityMetadata.RowNumber;
                int? column = propertyMapping?.ColumnNumber;

                foreach (var errorMessage in errorMessages)
                {
                    errors.AddImportError(row, column, errorMessage);
                }
            }
        }

        #endregion
    }
}
