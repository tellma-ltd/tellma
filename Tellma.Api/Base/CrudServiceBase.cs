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
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Api.Metadata;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments
    /// and allow selecting a certain record by Id, as well as updating, deleting and importing lists
    /// of that entity.
    /// </summary>
    public abstract class CrudServiceBase<TEntityForSave, TEntity, TKey, TEntitiesResult, TEntityResult> : FactGetByIdServiceBase<TEntity, TKey, TEntitiesResult, TEntityResult>
        where TEntitiesResult : EntitiesResult<TEntity>
        where TEntityResult : EntityResult<TEntity>
        where TEntityForSave : EntityWithKey<TKey>
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        private readonly DataParser _parser;
        private readonly DataComposer _composer;
        private readonly IStringLocalizer _localizer;
        private readonly MetadataProvider _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrudServiceBase{TEntityForSave, TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="deps">The service dependencies.</param>
        public CrudServiceBase(CrudServiceDependencies deps) : base(deps)
        {
            _parser = deps.Parser;
            _composer = deps.Composer;
            _localizer = deps.Localizer;
            _metadata = deps.Metadata;
        }

        #endregion

        #region API

        /// <summary>
        /// Saves the entities (upsert) into the database after authorization and validation.
        /// </summary>
        /// <returns>Optionally returns the same entities in their persisted READ form as per the specs in <paramref name="args"/>.</returns>
        public virtual async Task<TEntitiesResult> Save(List<TEntityForSave> entities, SaveArguments args)
        {
            await Initialize();

            // Trim all strings as a preprocessing step
            entities.ForEach(e => e.StructuralPreprocess());

            // Check that any updated Ids are 
            ExpressionFilter updateFilter = await CheckUpdatePermissionBefore(entities);

            // Structural Validation (before preprocessing)
            // Check that non-null non-0 Ids are unique
            ValidateUniqueIds(entities);

            // Attribute validation
            var meta = await GetMetadataForSave(cancellation: default);
            ValidateList(entities, meta);
            ModelState.ThrowIfInvalid();

            // Start a transaction scope for save
            using var trx = TransactionFactory.ReadCommitted();

            // Preprocess the entities
            entities = await SavePreprocessAsync(entities);

            // Save and retrieve Ids
            var returnEntities = args?.ReturnEntities ?? false;
            var ids = await SaveExecuteAsync(entities, returnEntities || updateFilter != null);

            // Load the entities (using the update permissions to check for RLS)
            TEntitiesResult result = returnEntities ?
                await GetByIds(ids, args, PermissionActions.Update, cancellation: default) :
                await ToEntitiesResult(null);

            // Check that the saved entities satisfy the user's row level security filter
            await CheckActionPermissionsAfter(updateFilter, ids, result.Data);

            // Perform side effects of save that are not transactional, just before committing the transaction
            await NonTransactionalSideEffectsForSave(entities, result.Data);

            // Complete the transaction and return
            trx.Complete();
            return result;
        }

        /// <summary>
        /// Deletes all the entities whose Id is one of the given <paramref name="ids"/>.
        /// </summary>
        public virtual async Task DeleteByIds(List<TKey> ids)
        {
            await Initialize();

            if (ids == null || !ids.Any())
            {
                return;
            }

            // Permissions
            var deleteFilter = await UserPermissionsFilter(PermissionActions.Delete, cancellation: default);
            ids = await CheckActionPermissionsBefore(deleteFilter, ids);

            // Transaction
            using var trx = TransactionFactory.ReadCommitted();

            try
            {
                // Execute
                await DeleteExecuteAsync(ids);

                trx.Complete();
            }
            catch (ForeignKeyViolationException)
            {
                // Suppress the existing transaction since it was aborted
                using var suppress = TransactionFactory.Suppress();
                var meta = await GetMetadata(cancellation: default);
                suppress.Complete();

                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        /// <summary>
        /// Deletes the entity whose Id is equal to the given <paramref name="id"/>.
        /// </summary>
        public virtual async Task DeleteById(TKey id)
        {
            var singleton = new List<TKey> { id };
            await DeleteByIds(singleton);
        }

        /// <summary>
        /// Exports the entities by their Ids as specified in <paramref name="args"/> into a CSV file
        /// that is suitable for import via <see cref="Import"/>.
        /// </summary>
        /// <param name="args">The specifications of the export operation.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A <see cref="Stream"/> containing the exported CSV data.</returns>
        public async Task<Stream> ExportByIds(ExportByIdsArguments<TKey> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var metaForSave = await GetMetadataForSave(cancellation);
            var meta = await GetMetadata(cancellation);

            // Get the default mapping, auto calculated from the entity for save metadata
            MappingInfo mapping = await GetDefaultMapping(metaForSave, meta, cancellation);

            // Create headers
            string[] headers = HeadersFromMapping(mapping);

            // Load entities
            string select = SelectFromMapping(mapping);
            var result = await GetByIds(args.I, new SelectExpandArguments
            {
                Select = select
            },
            cancellation);

            // Create content
            var dataWithoutHeaders = _composer.Compose(result.Data, mapping);

            // Final result
            var data = new List<string[]> { headers }.Concat(dataWithoutHeaders);
            var packager = new CsvPackager();
            return packager.Package(data);
        }

        /// <summary>
        /// Exports the entities as specified in <paramref name="args"/> into a CSV file
        /// that is suitable for import via <see cref="Import"/>.
        /// </summary>
        /// <param name="args">The specifications of the export operation.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A <see cref="Stream"/> containing the exported CSV data.</returns>
        public async Task<Stream> Export(ExportArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var metaForSave = await GetMetadataForSave(cancellation);
            var meta = await GetMetadata(cancellation);

            // Get the default mapping, auto calculated from the entity for save metadata
            MappingInfo mapping = await GetDefaultMapping(metaForSave, meta, cancellation);

            // Create headers
            string[] headers = HeadersFromMapping(mapping);

            // Load entities
            string select = SelectFromMapping(mapping);
            var result = await GetEntities(new GetArguments
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

            var entities = result.Data;

            // Create content
            var dataWithoutHeaders = _composer.Compose(entities, mapping);

            // Final result
            var data = new List<string[]> { headers }.Concat(dataWithoutHeaders);
            var packager = new CsvPackager();
            return packager.Package(data);
        }

        /// <summary>
        /// Export the default template that can be used for importing entities through this service.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A <see cref="Stream"/> containing the exported CSV template.</returns>
        public async Task<Stream> CsvTemplate(CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var metaForSave = await GetMetadataForSave(cancellation);
            var meta = await GetMetadata(cancellation);

            // Get the default mapping, auto calculated from the entity for save metadata
            var mapping = await GetDefaultMapping(metaForSave, meta, cancellation);

            // Get the headers from the mapping
            string[] headers = HeadersFromMapping(mapping);

            // Create a CSV file containing only those headers
            var data = new List<string[]> { headers };
            var packager = new CsvPackager();
            return packager.Package(data);
        }

        /// <summary>
        /// Imports entities from a CSV or Excel file as specified in <paramref name="args"/>.<br/>
        /// The import logic relies on the headers in the file to create a mapping from every column
        /// to a field on the entity. Then it parses the file contents and uses it to create and hydrate 
        /// a list of entities which it then validates and inserts or updates into the database depending
        /// on the import mode specified in <paramref name="args"/>.
        /// </summary>
        /// <param name="fileStream">A <see cref="Stream"/> containing the imported CSV or Excel file.</param>
        /// <param name="fileName">The name of the imported file.</param>
        /// <param name="contentType">The mime type of the imported file (CSV and Excel are supported).</param>
        /// <param name="args">The specifications of the import operation.</param>
        /// <returns>Few statistics about the import operation.</returns>
        public async Task<ImportResult> Import(Stream fileStream, string fileName, string contentType, ImportArguments args)
        {
            var sw = new Stopwatch();
            sw.Start();

            await Initialize();

            // Validation

            args.Mode ??= ImportModes.Insert; // Default
            if (!ImportModes.All.Contains(args.Mode))
            {
                var allowedValues = string.Join(", ", ImportModes.All);
                throw new ServiceException(_localizer["Error_UnknownImportMode0AllowedValuesAre1", args.Mode, allowedValues]);
            }

            if (args.Mode != ImportModes.Insert && string.IsNullOrWhiteSpace(args.Key))
            {
                // Key parameter is required for import modes update and merge
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, _localizer["KeyProperty"]]);
            }

            if (fileStream == null)
            {
                throw new ServiceException(_localizer["Error_NoFileWasUploaded"]);
            }

            // Extract the raw data from the file stream
            IEnumerable<string[]> data = BaseUtil.ExtractStringsFromFile(fileStream, fileName, contentType, _localizer);
            if (!data.Any())
            {
                throw new ServiceException(_localizer["Error_UploadedFileWasEmpty"]);
            }

            // Map the columns
            var importErrors = new ImportErrors();
            var headers = data.First();
            var mapping = await MappingFromHeaders(headers, importErrors, cancellation: default);
            importErrors.ThrowIfInvalid(_localizer);

            // Parse the data to entities
            var entitiesEnum = await _parser.ParseAsync<TEntityForSave>(data.Skip(1), mapping, importErrors);
            importErrors.ThrowIfInvalid(_localizer);

            // Handle Update and Merge modes
            await HydrateIds(entitiesEnum, args, mapping, importErrors);
            importErrors.ThrowIfInvalid(_localizer);

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

                return new ImportResult(inserted, updated, sw.ElapsedMilliseconds);
            }
            catch (ValidationException ex)
            {
                // Map errors to row numbers
                var validationErrors = ex.ModelState;
                if (validationErrors.IsValid)
                {
                    throw new InvalidOperationException($"Bug: {nameof(ValidationException)} without validation errors.");
                }

                MapErrors(validationErrors, importErrors, entities);
                if (importErrors.IsValid)
                {
                    throw new InvalidOperationException($"Bug: {nameof(ValidationException)} validation errors were incorrectly mapped to an empty collection.");
                }

                string errorMsg = importErrors.ToString(_localizer);
                throw new ServiceException(errorMsg);
            }
        }

        #endregion

        #region Helpers

        #region Save

        /// <summary>
        /// If 2 or more entities in <paramref name="entities"/> have the same Id that isn't null or 0, 
        /// an appropriate error is added to the <see cref="ValidationErrorsDictionary"/>.
        /// </summary>
        private void ValidateUniqueIds(List<TEntityForSave> entities)
        {
            // Check that Ids are unique
            var duplicateIds = entities.Where(e => !(e.GetId()?.Equals(0) ?? true)) // takes away the nulls too
                .GroupBy(e => e.GetId())
                .Where(g => g.Count() > 1);

            if (duplicateIds.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<TEntityForSave, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateIds in duplicateIds)
                {
                    foreach (var entity in groupWithDuplicateIds)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        ModelState.AddError($"[{index}].Id", _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", entity.GetId()]);
                    }
                }
            }
        }

        /// <summary>
        /// Performs any preprocessing on the entities before they are saved. This method is optional.
        /// </summary>
        /// <param name="entities">The entities to preprocess.</param>
        /// <returns>The preprocessed entities.</returns>
        protected virtual Task<List<TEntityForSave>> SavePreprocessAsync(List<TEntityForSave> entities)
        {
            return Task.FromResult(entities);
        }

        /// <summary>
        /// Implementations perform two steps:<br/>
        /// 1) Validate <paramref name="entities"/>. <br/>
        /// 2) If invalid: throws a <see cref="ValidationException"/> containing all the errors. <br/>
        /// 3) If valid: persists <paramref name="entities"/> in the store, either creating or updating them.
        /// <para/>
        /// Note: the call to this method is already wrapped inside a transaction, the user is trusted
        /// to have the necessary permissions and duplicate Ids are already validated against.
        /// </summary>
        /// <exception cref="ValidationException"></exception>
        protected abstract Task<List<TKey>> SaveExecuteAsync(List<TEntityForSave> entities, bool returnIds);

        /// <summary>
        /// Any save side effects to Save that are not transactional (such as saving to Blob storage or
        /// sending emails) should be implemented in this method, this method is the last step after checking 
        /// the user's update permissions before  committing the save transaction, so an error here is the 
        /// last opportunity to roll back the transaction.
        /// </summary>
        protected virtual Task NonTransactionalSideEffectsForSave(List<TEntityForSave> entities, IReadOnlyList<TEntity> data) => Task.CompletedTask;

        /// <summary>
        /// Verifies that the user has the necessary permissions to save the <paramref name="entities"/>.
        /// </summary>
        /// <param name="entities">The entities being saved.</param>
        /// <returns>The user's update <see cref="ExpressionFilter"/>.</returns>
        private async Task<ExpressionFilter> CheckUpdatePermissionBefore(List<TEntityForSave> entities)
        {
            if (entities == null || !entities.Any())
            {
                // No point verifying anything
                return null;
            }

            var updateFilter = await UserPermissionsFilter(PermissionActions.Update, cancellation: default);
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
                    var dbEntities = await QueryFactory()
                                    .EntityQuery<TEntity>()
                                    .Select("Id")
                                    .FilterByIds(allIds)
                                    .ToListAsync(QueryContext(), cancellation: default);

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
                var baseQuery = QueryFactory()
                                .EntityQuery<TEntity>()
                                .FilterByIds(updatedIds);

                var updatableIdsCount = await baseQuery
                                .Filter(updateFilter)
                                .CountAsync(QueryContext(), cancellation: default);

                if (updatableIdsCount == updatedIdsCount)
                {
                    // All updated DB entities satisfy the filter => good to go, but a post check is required
                    return updateFilter;
                }
                else
                {
                    // Check that all Ids are readable
                    var readFilter = await UserPermissionsFilter(PermissionActions.Read, cancellation: default);

                    var readableIds = await baseQuery
                                .Select("Id")
                                .Filter(readFilter)
                                .ToListAsync(QueryContext(), cancellation: default);

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

        #endregion

        #region Delete

        /// <summary>
        /// Implementations perform three steps:<br/>
        /// 1) Validate that all entities whose Id is one of the given <paramref name="ids"/> can indeed be deleted. <br/>
        /// 2) If invalid: throws a <see cref="ValidationException"/> containing all the errors. <br/>
        /// 3) If valid: delete from the database all entities whose Id is one of the given <paramref name="ids"/>.<br/>
        /// 4) Any non transactional side effects at the end (optional).
        /// <para/>
        /// Note: the call to this method is already wrapped inside a transaction, the user is already trusted
        /// to have the necessary permissions to delete. Also the call is wrapped inside a try that catches any
        /// <see cref="ForeignKeyViolationException"/> and translates it into an appropriate error message.
        /// </summary>
        protected abstract Task DeleteExecuteAsync(List<TKey> ids);

        #endregion

        #region Import & Export

        private async Task HydrateIds(IEnumerable<TEntityForSave> entities, ImportArguments args, MappingInfo mapping, ImportErrors errors)
        {
            if (args.Mode == ImportModes.Insert)
            {
                return;
            }

            var propMapping = mapping.SimplePropertyByName(args.Key);
            if (propMapping == null)
            {
                throw new ServiceException(_localizer["Error_KeyProperty0MustBeInTheImportedFile", args.Key]);
            }

            var propMetaForSave = propMapping.MetadataForSave;
            var propDescForSave = propMetaForSave.Descriptor;
            if (propDescForSave.Type != typeof(string) && propDescForSave.Type != typeof(int) && propDescForSave.Type != typeof(int?))
            {
                throw new ServiceException(_localizer["Error_KeyProperty0NotValidItMustIntOrString", propMetaForSave.Display()]);
            }

            Func<Entity, object> forSaveKeyGet = propDescForSave.GetValue;

            // For update mode, check that all keys are present
            if (args.Mode == ImportModes.Update)
            {
                foreach (var entity in entities.Where(e => forSaveKeyGet(e) == null || forSaveKeyGet(e).Equals(0)))
                {
                    // In update mode, the 
                    string errorMsg = _localizer["Error_Property0IsKeyPropertyThereforeRequiredForUpdate", propMetaForSave.Display()];
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
                    string errorMsg = _localizer["Error_Value0IsDuplicatedEvenThoughItIsKey1", duplicateKeyValue, propMetaForSave.Display()];
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

            // If key property is Id, there is nothing to hydrate
            if (args.Key == "Id")
            {
                return;
            }

            // Load entities from the DB
            var userKeys = entities.Select(e => forSaveKeyGet(e)).Where(e => e != null);
            var getArgs = new SelectExpandArguments { Select = propDescForSave.Name };
            var result = await GetByPropertyValues(propDescForSave.Name, userKeys, getArgs, cancellation: default);
            var dbEntities = result.Data;
            if (dbEntities.Any())
            {
                // Prepare the key property description of TEntity
                var typeDesc = TypeDescriptor.Get<TEntity>();
                var prop = typeDesc.Property(args.Key);
                if (prop == null)
                {
                    throw new InvalidOperationException($"Bug: Type {typeof(TEntityForSave).Name} has property {args.Key} but not type {nameof(TEntity)}");
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
                                var typeDisplay = mapping.MetadataForSave.SingularDisplay();
                                var keyPropDisplay = propMetaForSave.Display();
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
                            var typeDisplay = mapping.MetadataForSave.SingularDisplay();
                            var keyPropDisplay = propMetaForSave.Display();
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
        /// Gets the default <see cref="PropertyMappingInfo"/> of an entity type, without the weak collections.
        /// </summary>
        protected static List<PropertyMappingInfo> GetDefaultSimplePropertyMappings(TypeMetadata metaForSave, TypeMetadata meta, int nextAvailableIndex)
        {
            var fkNames = meta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);

            // Prepare simple props
            var simpleProps = new List<PropertyMappingInfo>();
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
                    simpleProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty)
                    {
                        Index = nextAvailableIndex++,
                    });
                }
                else
                {
                    // Simple property
                    simpleProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                    {
                        Index = nextAvailableIndex++,
                    });
                }
            }

            return simpleProps;
        }

        /// <summary>
        /// Returns the default mapping based on the properties in meta (not meta for save).
        /// </summary>
        protected virtual Task<MappingInfo> GetDefaultMapping(TypeMetadata metaForSave, TypeMetadata meta, CancellationToken cancellation)
        {
            // Inner recursive function, returns the mapping and the next available column index
            static (MappingInfo mapping, int nextAvailableIndex) GetDefaultMappingInner(TypeMetadata metaForSave, TypeMetadata meta, int nextAvailableIndex, CollectionPropertyMetadata collPropMetaForSave = null, CollectionPropertyMetadata collPropMeta = null)
            {
                var simpleProps = GetDefaultSimplePropertyMappings(metaForSave, meta, nextAvailableIndex);
                nextAvailableIndex += simpleProps.Count;

                // Prepare collection props
                var collectionProps = new List<MappingInfo>();
                foreach (var nextCollPropMetaForSave in metaForSave.CollectionProperties)
                {
                    string propName = nextCollPropMetaForSave.Descriptor.Name;
                    var nextCollPropMeta = meta.CollectionProperty(propName) ??
                        throw new InvalidOperationException($"Bug: Collection property '{propName}' exists on type for save {metaForSave.Descriptor.Name} but not on {meta.Descriptor.Name}");

                    TypeMetadata nextMetaForSave = nextCollPropMetaForSave.CollectionTargetTypeMetadata;
                    TypeMetadata nextMeta = nextCollPropMeta.CollectionTargetTypeMetadata;

                    // Recursive call
                    var (nextMapping, nextIndex) = GetDefaultMappingInner(nextMetaForSave, nextMeta, nextAvailableIndex, nextCollPropMetaForSave, nextCollPropMeta);
                    collectionProps.Add(nextMapping);
                    nextAvailableIndex = nextIndex;
                }

                // Return the mapping and the next available index
                var mapping = new MappingInfo(metaForSave, meta, simpleProps, collectionProps, collPropMetaForSave, collPropMeta);
                return (mapping, nextAvailableIndex);
            }

            // Call the inner recursive function and return the result;
            var (mapping, _) = GetDefaultMappingInner(metaForSave, meta, 0);
            var result = ProcessDefaultMapping(mapping);
            return Task.FromResult(result);
        }

        /// <summary>
        /// Provides a configuration point for services to alter the default mapping info used for
        /// generating the import template and exporting for import.
        /// </summary>
        protected virtual MappingInfo ProcessDefaultMapping(MappingInfo mapping)
        {
            return mapping;
        }

        protected virtual IEnumerable<string> AdditionalSelectForExport()
        {
            yield break;
        }

        private string SelectFromMapping(MappingInfo mapping)
        {
            static void SelectFromMappingInner(MappingInfo mapping, HashSet<string> selectHash, string prefix)
            {
                string prefixDot = prefix == null ? "" : $"{prefix}.";
                foreach (var simpleProp in mapping.SimpleProperties)
                {
                    string select = prefixDot;
                    if (simpleProp.SelectPrefix != null)
                    {
                        select += $"{simpleProp.SelectPrefix}.";
                    }

                    // Append the property name
                    if (simpleProp is ForeignKeyMappingInfo fkProp && fkProp.NotUsingIdAsKey)
                    {
                        // Append navigation property name followed by key. E.g. Resource.Code
                        select += $"{fkProp.NavPropertyMetadata.Descriptor.Name}.{fkProp.KeyPropertyMetadata.Descriptor.Name}";
                    }
                    else
                    {
                        // Append simple property name. E.g. PostingDate
                        select += simpleProp.Metadata.Descriptor.Name;
                    }

                    selectHash.Add(select);
                }

                foreach (var collProp in mapping.CollectionProperties)
                {
                    string nextPrefix = $"{prefixDot}{collProp.Select}";
                    SelectFromMappingInner(collProp, selectHash, nextPrefix);
                }
            }

            HashSet<string> selectHash = new();
            SelectFromMappingInner(mapping, selectHash, prefix: null);
            foreach (var s in AdditionalSelectForExport())
            {
                selectHash.Add(s);
            }

            return string.Join(',', selectHash);
        }

        private static string[] HeadersFromMapping(MappingInfo mapping)
        {
            static string Escape(string propDisplay)
            {
                return propDisplay.Replace("/", "//").Replace("-", "--");
            }

            static void PopulateHeadersArray(string[] headers, MappingInfo mapping, string path = null)
            {
                foreach (var g in mapping.SimpleProperties.GroupBy(e => e.Display()))
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

                foreach (var g in mapping.CollectionProperties.GroupBy(e => e.Display()))
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

        private async Task<MappingInfo> MappingFromHeaders(string[] headers, ImportErrors errors, CancellationToken cancellation)
        {
            // Create the trie of labels
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
            var rootMetadata = await GetMetadata(cancellation);
            var rootMetadataForSave = await GetMetadataForSave(cancellation);

            // Create the mapping recurisvely using the trie
            var defaultMapping = await GetDefaultMapping(rootMetadataForSave, rootMetadata, cancellation);
            var result = trie.CreateMapping(defaultMapping, errors, _localizer);
            return result;
        }

        protected async Task<TypeMetadata> GetMetadataForSave(CancellationToken cancellation)
        {
            int? tenantId = TenantId;
            int? definitionId = DefinitionId;
            Type typeForSave = typeof(TEntityForSave);
            IMetadataOverridesProvider overrides = await FactBehavior.GetMetadataOverridesProvider(cancellation);

            return _metadata.GetMetadata(tenantId, typeForSave, definitionId, overrides);
        }

        /// <summary>
        /// Splits header label into a collection of steps and an optional key.
        /// Header example: "Step1 / Step2 / Step3 - Key", non structural slashes 
        /// and minus signs should be escaped by repeating them.
        /// </summary>
        protected static (List<string> steps, string key) SplitHeader(string headerLabel)
        {
            var result = new List<string>();
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

            var (prop, key) = SplitLastStep(builder.ToString());
            result.Add(prop);

            return (result, key);
        }

        /// <summary>
        /// Splits the last step of a header label into a property label and an
        /// optional key property label.
        /// </summary>
        private static (string prop, string key) SplitLastStep(string stepLabel)
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

        /// <summary>
        /// Translates the errors from <paramref name="modelErrors"/> which groups errors by property
        /// paths to <paramref name="importErrors"/> which groups errors by row and column coordinates.
        /// This is needed during import operations which reuse the validation logic of regular save logic.
        /// </summary>
        /// <param name="modelErrors">The source containing the errors.</param>
        /// <param name="importErrors">The destination to add the mapped errors to.</param>
        /// <param name="entities">The entities being imported.</param>
        private static void MapErrors(ValidationErrorsDictionary modelErrors, ImportErrors importErrors, List<TEntityForSave> entities)
        {
            foreach (var (key, errorMessages) in modelErrors.AllErrors)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new InvalidOperationException($"Bug: Empty validation error key.");
                }

                var steps = key.Split('.').Select(e => e.Trim());

                // Get the root index
                string firstStep = steps.First();
                if (!firstStep.StartsWith('[') || !firstStep.EndsWith(']'))
                {
                    throw new InvalidOperationException($"Bug: validation error key '{key}' should start with the root index in square brackets [].");
                }

                var rootIndexString = firstStep[1..^1]; // = Substring(1)
                if (!int.TryParse(rootIndexString, out int rootIndex))
                {
                    throw new InvalidOperationException($"Bug: root index '{rootIndexString}' could not be parsed into an integer.");
                }

                if (rootIndex >= entities.Count)
                {
                    throw new InvalidOperationException($"Bug: root index '{rootIndexString}' is larger than the size of the indexed list {entities.Count}.");
                }

                Entity currentEntity = entities[rootIndex];
                TypeDescriptor currentTypeDesc = TypeDescriptor.Get<TEntityForSave>();
                PropertyMappingInfo propertyMapping = null; // They property that the error key may optionally terminate with
                bool lastPropWasCollectionWithoutIndexer = false;
                bool lastPropWasSimple = false;

                foreach (var step in steps.Skip(1))
                {
                    if (currentEntity == null)
                    {
                        throw new InvalidOperationException($"Bug: step '{step}' on validation error key '{key}' is applied to a null entity.");
                    }
                    if (lastPropWasCollectionWithoutIndexer)
                    {
                        throw new InvalidOperationException($"Bug: step '{step}' on validation error key '{key}' is applied to a list.");
                    }
                    if (lastPropWasSimple)
                    {
                        throw new InvalidOperationException($"Bug: step '{step}' on validation error key '{key}' is applied to a simple property.");
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
                            throw new InvalidOperationException($"Bug: validation error key '{key}' contains index '{rootIndexString}' that could not be parsed into an integer.");
                        }

                        var propName = string.Join('[', split.SkipLast(1));
                        if (string.IsNullOrWhiteSpace(propName))
                        {
                            throw new InvalidOperationException($"Bug: validation error key '{key}' cannot contain a lone indexer in the middle of it.");
                        }

                        // Retrieve the next entity using descriptors
                        var propDesc = currentTypeDesc.CollectionProperty(propName) ??
                            throw new InvalidOperationException($"Bug: collection property '{propName}' on validation error key '{key}' could not be found on type {currentTypeDesc.Name}.");

                        currentEntity = ((propDesc.GetValue(currentEntity) as IList)[index] as Entity) ??
                            throw new InvalidOperationException($"Bug: step '{step}' on validation error key '{key}' refers to a null entity.");

                        currentTypeDesc = propDesc.CollectionTypeDescriptor;

                        // Retrieve the next mapping if possible
                        //       var nextMapping = currentMapping?.CollectionProperty(propName);
                    }
                    else // Property: either collection, navigation, or simple
                    {
                        var propName = step;
                        var propDesc = currentTypeDesc.Property(propName);

                        if (propDesc is null)
                        {
                            throw new InvalidOperationException($"Bug: property '{propName}' on validation error key '{key}' could not be found on type {currentTypeDesc.Name}.");
                        }
                        else if (propDesc is CollectionPropertyDescriptor collPropDesc)
                        {
                            // A collection property without indexer cannot by succeeded by more steps
                            lastPropWasCollectionWithoutIndexer = true; // To prevent further steps
                        }
                        else if (propDesc is NavigationPropertyDescriptor)
                        {
                            // Won't implement for now, there aren't any cases in our model
                            throw new NotImplementedException("Navigation property errors not implemented.");
                        }
                        else // Simple prop
                        {
                            // Retrieve the property mapping if possible
                            var baseEntity = currentEntity.EntityMetadata.BaseEntity ?? currentEntity;
                            var baseMapping = baseEntity.EntityMetadata.MappingInfo as MappingInfo;
                            var simpleProps = baseMapping.SimplePropertiesByName(propName);
                            propertyMapping = simpleProps?.FirstOrDefault(p => p.GetTerminalEntityForSave(baseEntity) == currentEntity);

                            lastPropWasSimple = true; // To prevent further steps
                        }
                    }
                }

                // Now to use the goods
                currentEntity = currentEntity.EntityMetadata.BaseEntity ?? currentEntity;
                int row = currentEntity.EntityMetadata.RowNumber;
                int? column = propertyMapping?.ColumnNumber;

                foreach (var errorMessage in errorMessages)
                {
                    importErrors.AddImportError(row, column, errorMessage);
                }
            }
        }

        /// <summary>
        /// Data structure useful for creating <see cref="MappingInfo"/> efficiently.
        /// </summary>
        protected class LabelPathTrie : Dictionary<string, LabelPathTrie>
        {
            private readonly HashSet<LabelPathProperty> _props = new();

            public void AddPath(IEnumerable<string> steps, string key, int index)
            {
                if (steps == null || !steps.Any())
                {
                    throw new InvalidOperationException($"Bug: Attempt to add an empty header to the trie");
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

            public MappingInfo CreateMapping(MappingInfo defaultMapping, ImportErrors errors, IStringLocalizer localizer)
            {
                ///////// (1) Simple Properties
                var simpleProps = new Dictionary<string, PropertyMappingInfo>();
                foreach (var g in defaultMapping.SimpleProperties.GroupBy(p => p.Display()))
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

                var simplePropMappings = new List<PropertyMappingInfo>();
                HashSet<string> simplePropsLabels = null;
                HashSet<string> simplePropsLabelsIgnoreCase = null;
                foreach (var prop in _props)
                {
                    if (!simpleProps.TryGetValue(prop.PropLabel, out PropertyMappingInfo propMapping))
                    {
                        simplePropsLabels ??= defaultMapping.SimpleProperties.Select(e => e.Display()).ToHashSet();
                        simplePropsLabelsIgnoreCase ??= defaultMapping.SimpleProperties.Select(e => e.Display()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (simplePropsLabels.Contains(prop.PropLabel))
                        {
                            // Common mistake: label isn't unique and must be postfixed with a number to disambiguate it
                            errors.AddImportError(1, prop.ColumnNumber, localizer["Error_Label0MatchesMultipleFieldsOnType1", prop.PropLabel, defaultMapping.MetadataForSave.SingularDisplay()]);
                        }
                        else if (simplePropsLabelsIgnoreCase.TryGetValue(prop.PropLabel, out string actualLabel))
                        {
                            // Common mistake: using the wrong case
                            errors.AddImportError(1, prop.ColumnNumber, localizer["Error_Label0DoesNotMatchAnyFieldOnType1DidYouMean2", prop.PropLabel, defaultMapping.MetadataForSave.SingularDisplay(), actualLabel]);
                        }
                        else
                        {
                            errors.AddImportError(1, prop.ColumnNumber, localizer["Error_Label0DoesNotMatchAnyFieldOnType1", prop.PropLabel, defaultMapping.MetadataForSave.SingularDisplay()]);
                        }

                        continue;
                    }
                    else if (propMapping is ForeignKeyMappingInfo fkPropMapping)
                    {
                        // Foreign Key
                        string keyLabel = prop.KeyLabel;
                        if (string.IsNullOrWhiteSpace(keyLabel))
                        {
                            // FK without a key property
                            errors.AddImportError(1, prop.ColumnNumber, localizer["Error_KeyPropertyIsRequiredToSet0Field", propMapping.Display()]);
                            continue;
                        }

                        var navPropMeta = fkPropMapping.NavPropertyMetadata;
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

                        simplePropMappings.Add(new ForeignKeyMappingInfo(fkPropMapping, keyPropMetadata)
                        {
                            Index = prop.Index,
                        });
                    }
                    else
                    {
                        // This is a simple prop
                        simplePropMappings.Add(new PropertyMappingInfo(propMapping)
                        {
                            Index = prop.Index,
                        });
                    }
                }

                ///////// (2) Collection Properties
                var collectionProps = new Dictionary<string, MappingInfo>();
                foreach (var g in defaultMapping.CollectionProperties.GroupBy(p => p.Display()))
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

                List<MappingInfo> collectionPropMappings = new();
                HashSet<string> collectionPropsLabels = null;
                HashSet<string> collectionPropsLabelsIgnoreCase = null;
                foreach (var (collectionPropName, trie) in this)
                {
                    if (!collectionProps.TryGetValue(collectionPropName, out MappingInfo collectionMapping))
                    {
                        collectionPropsLabels ??= defaultMapping.CollectionProperties.Select(e => e.Display()).ToHashSet();
                        collectionPropsLabelsIgnoreCase ??= defaultMapping.CollectionProperties.Select(e => e.Display()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (collectionPropsLabels.Contains(collectionPropName))
                        {
                            // Common mistake: label isn't unique and must be postfixed with a number to disambiguate it
                            errors.AddImportError(1, trie.FirstColumnNumber(), localizer["Error_Label0MatchesMultipleCollectionsOnType1", collectionPropName, defaultMapping.MetadataForSave.SingularDisplay()]);
                        }
                        else if (collectionPropsLabelsIgnoreCase.TryGetValue(collectionPropName, out string actualLabel))
                        {
                            // Common mistake: using the wrong case
                            errors.AddImportError(1, trie.FirstColumnNumber(), localizer["Error_Label0DoesNotMatchAnyCollectionOnType1DidYouMean2", collectionPropName, defaultMapping.MetadataForSave.SingularDisplay(), actualLabel]);
                        }
                        else
                        {
                            errors.AddImportError(1, trie.FirstColumnNumber(), localizer["Error_Label0DoesNotMatchAnyCollectionOnType1", collectionPropName, defaultMapping.MetadataForSave.SingularDisplay()]);
                        }

                        continue;
                    }

                    collectionPropMappings.Add(trie.CreateMapping(collectionMapping, errors, localizer));
                }

                return new MappingInfo(defaultMapping, simplePropMappings, collectionPropMappings);
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

        #endregion

        #region Validation

        /// <summary>
        /// Localizes every error in the collection and adds it to <see cref="ServiceBase.ModelState"/>.
        /// </summary>
        public void AddLocalizedErrors(IEnumerable<ValidationError> errors) => AddLocalizedErrors(errors, _localizer);

        /// <summary>
        /// Localizes every error in the collection and adds it to <see cref="ServiceBase.ModelState"/>. <br/>
        /// If the model state contains errors throws a <see cref="ValidationException"/>.
        /// </summary>
        /// <exception cref="ValidationException"></exception>
        public void AddErrorsAndThrowIfInvalid(IEnumerable<ValidationError> errors)
        {
            AddLocalizedErrors(errors);
            ModelState.ThrowIfInvalid();
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments
    /// and allow selecting a certain record by Id, as well as updating, deleting and importing lists
    /// of that entity.
    /// </summary>
    public abstract class CrudServiceBase<TEntityForSave, TEntity, TKey> : CrudServiceBase<TEntityForSave, TEntity, TKey, EntitiesResult<TEntity>, EntityResult<TEntity>>
        where TEntityForSave : EntityWithKey<TKey>
        where TEntity : EntityWithKey<TKey>
    {
        public CrudServiceBase(CrudServiceDependencies deps) : base(deps)
        {
        }

        protected override Task<EntitiesResult<TEntity>> ToEntitiesResult(List<TEntity> data, int? count = null, CancellationToken cancellation = default)
        {
            var result = new EntitiesResult<TEntity>(data, count);
            return Task.FromResult(result);
        }

        protected override Task<EntityResult<TEntity>> ToEntityResult(TEntity data, CancellationToken cancellation = default)
        {
            var result = new EntityResult<TEntity>(data);
            return Task.FromResult(result);
        }
    }
}
